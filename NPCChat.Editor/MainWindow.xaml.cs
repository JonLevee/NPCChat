using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using NPCChat.Core.SupportClasses;
using NPCChat.Core.Validation;
using NPCChat.Editor;
using NPCChat.Editor.Persistence;
using NPCChatLib.Builders;
using NPCChatLib.Extensions;
using NPCChatLib.WorldBuilderTemplates;
using NPCChatLib.WorldClasses;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Control = System.Windows.Controls.Control;
using Panel = System.Windows.Controls.Panel;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace NPCChat
{
    public partial class MainWindow : Window
    {
        private WorldData _world = null!;
        private WorldDataBuilder _worldBuilder = null!;
        private IServiceScope serviceScope = null!;

        private readonly DispatcherTimer _gameTimer = new();

        private bool _autoStartGame = false;

        private readonly UserSettingsRepository _userSettingsRepository;


        public MainWindow(UserSettingsRepository userSettingsRepository)
        {
            _userSettingsRepository = userSettingsRepository;
            InitializeComponent();

            Loaded += (s, e) => _userSettingsRepository.RestoreWindow(this);
            Closing += (s, e) => _userSettingsRepository.SaveWindow(this);

            var title = Title;
            this.LocationChanged += (s, e) => Title = $"{title} - {_userSettingsRepository.GetUserSettingsText(this)}";
            this.SizeChanged += (s, e) => Title = $"{title} - {_userSettingsRepository.GetUserSettingsText(this)}";

            DataContext = this;

            Title = "NPCChat Sandbox - Map View";
            _gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            _gameTimer.Tick += _gameTimer_Tick;

            var chunkInfo = App.Services.GetRequiredService<ChunkInfo>();
            CellSizeListBox.Items.Clear();
            chunkInfo.ChunkSizes.ForEach(size => CellSizeListBox.Items.Add(size));
            CellSizeListBox.SelectedItem = chunkInfo.ChunkSize;

            _gameTimer.Start();
        }

        private void _gameTimer_Tick(object? sender, EventArgs e)
        {
            if (_autoStartGame)
            {
                _autoStartGame = false;
                StartStopButton_Click(this, new RoutedEventArgs());
            }
        }

        private int CellSize => (int)CellSizeListBox.SelectedItem;

        private void ClearWorld()
        {
            _world.Clear();
        }

        private void RenderWorld()
        {
            if (serviceScope is null)
                return;

            var objects = _world.EnumerateWorldObjects();
            MapCanvas.Children.Clear();

            const int minCells = 32;
            const int paddingCells = 2;

            int maxRight = objects.Count == 0
                ? minCells
                : Math.Max(minCells, objects.Max(x => x.Bounds.Right) + paddingCells);

            int maxBottom = objects.Count == 0
                ? minCells
                : Math.Max(minCells, objects.Max(x => x.Bounds.Bottom) + paddingCells);

            double pixelWidth = maxRight * CellSize;
            double pixelHeight = maxBottom * CellSize;

            MapCanvas.Width = pixelWidth;
            MapCanvas.Height = pixelHeight;
            MapCanvas.Background = Brushes.Transparent;

            DrawGrid(maxRight, maxBottom);

            foreach (var obj in objects)
            {
                DrawObject(obj);
            }

            StatusTextBlock.Text =
                $"Objects: {objects.Count}" +
                $"   |   Static: {objects.Count(x => x.Category == WorldObjectCategory.Static)}" +
                $"   |   Dynamic: {objects.Count(x => x.Category == WorldObjectCategory.Dynamic)}" +
                $"   |   Chunks: {_world.Chunks.Count}" +
                $"   |   ChunkSize: {_worldBuilder.Options.ChunkInfo.ChunkSize}";
        }

        private void DrawGrid(int widthInCells, int heightInCells)
        {
            for (int x = 0; x <= widthInCells; x++)
            {
                double px = x * CellSize;
                MapCanvas.Children.Add(new Line
                {
                    X1 = px,
                    Y1 = 0,
                    X2 = px,
                    Y2 = heightInCells * CellSize,
                    Stroke = CreateBrush(45, 55, 72),
                    StrokeThickness = x % _worldBuilder.Options.ChunkInfo.ChunkSize == 0 ? 1.5 : 0.5,
                    Opacity = x % _worldBuilder.Options.ChunkInfo.ChunkSize == 0 ? 0.80 : 0.45
                });
            }

            for (int y = 0; y <= heightInCells; y++)
            {
                double py = y * CellSize;
                MapCanvas.Children.Add(new Line
                {
                    X1 = 0,
                    Y1 = py,
                    X2 = widthInCells * CellSize,
                    Y2 = py,
                    Stroke = CreateBrush(45, 55, 72),
                    StrokeThickness = y % _worldBuilder.Options.ChunkInfo.ChunkSize == 0 ? 1.5 : 0.5,
                    Opacity = y % _worldBuilder.Options.ChunkInfo.ChunkSize == 0 ? 0.80 : 0.45
                });
            }
        }

        private void DrawObject(WorldObject obj)
        {
            var rectangle = new Rectangle
            {
                Width = Math.Max(1, obj.Bounds.Width * CellSize),
                Height = Math.Max(1, obj.Bounds.Height * CellSize),
                Fill = GetFillBrush(obj),
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                RadiusX = obj.Category == WorldObjectCategory.Dynamic ? 8 : 2,
                RadiusY = obj.Category == WorldObjectCategory.Dynamic ? 8 : 2,
                ToolTip = $"{obj.Kind}\n{obj.Category}\n{obj.Bounds}\n{obj.Handle}"
            };

            Canvas.SetLeft(rectangle, obj.Bounds.Left * CellSize);
            Canvas.SetTop(rectangle, obj.Bounds.Top * CellSize);
            Panel.SetZIndex(rectangle, obj.Category == WorldObjectCategory.Dynamic ? 10 : 1);
            MapCanvas.Children.Add(rectangle);

            var label = new TextBlock
            {
                Text = GetLabel(obj),
                FontSize = Math.Max(10, CellSize * 0.42),
                Foreground = Brushes.White,
                IsHitTestVisible = false
            };

            Canvas.SetLeft(label, obj.Bounds.Left * CellSize + 4);
            Canvas.SetTop(label, obj.Bounds.Top * CellSize + 2);
            Panel.SetZIndex(label, obj.Category == WorldObjectCategory.Dynamic ? 11 : 2);
            MapCanvas.Children.Add(label);
        }

        private static Brush GetFillBrush(WorldObject obj)
        {
            return obj.Kind switch
            {
                WorldObjectKind.Building => CreateBrush(59, 130, 246),
                WorldObjectKind.Player => CreateBrush(34, 197, 94),
                WorldObjectKind.Npc => CreateBrush(245, 158, 11),
                WorldObjectKind.Mob => CreateBrush(239, 68, 68),
                _ => CreateBrush(148, 163, 184)
            };
        }

        private static SolidColorBrush CreateBrush(byte r, byte g, byte b)
        {
            return new SolidColorBrush(Color.FromArgb(255, r, g, b));
        }

        private static string GetLabel(WorldObject obj)
        {
            return obj.Kind switch
            {
                WorldObjectKind.Building => "B",
                WorldObjectKind.Player => "P",
                WorldObjectKind.Npc => "N",
                WorldObjectKind.Mob => "M",
                WorldObjectKind.DungeonEntrance => "D",
                _ => "?"
            };
        }

        private void CreateWorldScope()
        {
            Require.IsNull(serviceScope);

            serviceScope = App.Services.CreateScope();
            _worldBuilder = serviceScope.ServiceProvider.GetRequiredService<WorldDataBuilder>();
            _world = serviceScope.ServiceProvider.GetRequiredService<WorldData>();
            RenderWorld();
        }

        private void ClearWorldScope()
        {
            if (serviceScope != null)
            {
                serviceScope?.Dispose();
                serviceScope = null!;
                MapCanvas.Children.Clear();
                StatusTextBlock.Text = string.Empty;
                _world.Dispose();
                _world = null!;
                _worldBuilder = null!;
                BuildDemoTownButton.IsEnabled = false;
                RedrawButton.IsEnabled = false;
                CellSizeListBox.IsEnabled = true;
            }
        }

        private void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            switch (StartStopButton.Content)
            {
                case "Start":
                    CreateWorldScope();
                    BuildDemoTownButton.IsEnabled = true;
                    RedrawButton.IsEnabled = true;
                    CellSizeListBox.IsEnabled = false;
                    StartStopButton.Content = "Stop";
                    break;
                case "Stop":
                    ClearWorldScope();
                    BuildDemoTownButton.IsEnabled = false;
                    RedrawButton.IsEnabled = false;
                    CellSizeListBox.IsEnabled = true;
                    StartStopButton.Content = "Start";
                    break;
            }
        }

        private void BuildDemoTownButton_Click(object sender, RoutedEventArgs e)
        {
            ClearWorld();

            using var templates = _worldBuilder.GetTemplates();
            templates.AddSmallTown();

            RenderWorld();

        }

        private void RedrawButton_Click(object sender, RoutedEventArgs e)
        {
            RenderWorld();
        }

        private void CellSizeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}