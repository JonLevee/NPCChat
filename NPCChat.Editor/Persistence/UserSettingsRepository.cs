using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using NPCChatLib.Attributes;

namespace NPCChat.Editor.Persistence;

[Singleton]
public sealed class UserSettingsRepository
{
    private const double MinimumVisibleWidth = 200;
    private const double MinimumVisibleHeight = 150;
    private readonly string _userSettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Assembly.GetExecutingAssembly().GetName().Name!);
    private readonly string _userSettingsFile = string.Concat(Environment.UserName, "_UserSettings.config");
    private string _userSettingsPath => Path.Combine(_userSettingsDirectory, _userSettingsFile);

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    private record struct UserSettings(
        double Left,
        double Top,
        double Width,
        double Height,
        WindowState WindowState,
        WindowStyle WindowStyle,
        ResizeMode ResizeMode);


    public void RestoreWindow(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        if (!File.Exists(_userSettingsPath))
            return;
        var userSettings = JsonSerializer.Deserialize<UserSettings>(File.ReadAllText(_userSettingsPath), _jsonOptions);

        var rect = new Rectangle((int)userSettings.Left, (int)userSettings.Top, (int)userSettings.Width, (int)userSettings.Height);
        var canRestore = Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(rect));
        if (canRestore)
        {
            window.WindowStyle = userSettings.WindowStyle;
            window.ResizeMode = userSettings.ResizeMode;
            window.Left = userSettings.Left;
            window.Top = userSettings.Top;
            window.Width = userSettings.Width;
            window.Height = userSettings.Height;
            window.WindowState = userSettings.WindowState;
        }
        else
            throw new InvalidOperationException();
    }

    public void SaveWindow(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        var userSettings = new UserSettings(window.Left, window.Top, window.Width, window.Height, window.WindowState, window.WindowStyle, window.ResizeMode);
        var json = JsonSerializer.Serialize(userSettings, _jsonOptions);
        if (!Directory.Exists(_userSettingsDirectory))
        {
            Directory.CreateDirectory(_userSettingsDirectory);
        }
        File.WriteAllText(_userSettingsPath, json);
    }

    public string GetUserSettingsText(Window window)
    {
        return $"Left:{window.Left}, Top = {window.Top}, Width = {window.Width}, Height = {window.Height}, State = {window.WindowState}, Style = {window.WindowStyle}, Mode = {window.ResizeMode}";
    }

    public bool IsUsableWindowBounds(Rect rect)
    {
        if (rect.Width < MinimumVisibleWidth || rect.Height < MinimumVisibleHeight)
        {
            return false;
        }

        Rectangle drawingRect = ToDrawingRectangle(rect);
        var isUsable = Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(drawingRect));
        return isUsable;
    }

    private static Rectangle ToDrawingRectangle(Rect rect)
    {
        return new Rectangle(
            x: (int)Math.Round(rect.Left),
            y: (int)Math.Round(rect.Top),
            width: (int)Math.Round(rect.Width),
            height: (int)Math.Round(rect.Height));
    }
}
