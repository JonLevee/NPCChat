using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using NPCChat.Core.Validation;
using NPCChat.Editor;
using NPCChatLib.Builders;
using NPCChatLib.Extensions;
using NPCChatLib.WorldBuilderTemplates;
using NPCChatLib.WorldClasses;

namespace NPCChat
{
    public partial class MainWindow : Window
    {
        private readonly WorldData _world;
        private readonly WorldDataBuilder _worldBuilder;
        private IServiceScope? serviceScope;
        private Control[] _gameControls;

        private readonly DispatcherTimer _gameTimer = new();

        public RelayCommand CreateWorldScopeCommand { get; }
        public RelayCommand ClearWorldScopeCommand { get; }
        public RelayCommand BuildDemoTownCommand { get; }
        public RelayCommand ClearDemoTownCommand { get; }
        public RelayCommand RedrawCommand { get; }

        public MainWindow(WorldData world, WorldDataBuilder worldBuilder)
        {
            _world = world;
            _worldBuilder = worldBuilder;

            CreateWorldScopeCommand = new RelayCommand(
                execute: _ => CreateWorldScope(),
                canExecute: _ => serviceScope is null);

            ClearWorldScopeCommand = new RelayCommand(
                execute: _ => ClearWorldScope(),
                canExecute: _ => serviceScope is not null);

            BuildDemoTownCommand = new RelayCommand(
                execute: _ => BuildDemoTown(),
                canExecute: _ => serviceScope is not null);

            ClearDemoTownCommand = new RelayCommand(
                execute: _ =>
                {
                    ClearWorld();
                    RenderWorld();
                },
                canExecute: _ => serviceScope is not null);

            RedrawCommand = new RelayCommand(
                execute: _ => RenderWorld(),
                canExecute: _ => serviceScope is not null);

            InitializeComponent();

            DataContext = this;

            _gameControls = [BuildDemoTownButton, RedrawButton, CellSizeBox];

            Title = "NPCChat Sandbox - Map View";
            _gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            _gameTimer.Tick += _gameTimer_Tick;

            _gameControls.ForEach(c => c.IsEnabled = false);
            RefreshCommands();
        }

        private void _gameTimer_Tick(object? sender, EventArgs e)
        {
        }

        private int CellSize => GetCellSize();

        private int GetCellSize()
        {
            if (!int.TryParse(CellSizeBox.Text, out var value))
                value = 24;

            return Math.Max(8, Math.Min(64, value));
        }

        private void CellSizeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RenderWorld();
        }

        private void BuildDemoTown()
        {
            ClearWorld();

            using var templates = _worldBuilder.GetTemplates();
            templates.AddSmallTown();

            RenderWorld();
        }

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
                $"Objects: {objects.Count}   |   Static: {objects.Count(x => x.Category == WorldObjectCategory.Static)}   |   Dynamic: {objects.Count(x => x.Category == WorldObjectCategory.Dynamic)}   |   Chunks: {_world.Chunks.Count}   |   ChunkSize: {_worldBuilder.Options.ChunkSize}";
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
                    StrokeThickness = x % _worldBuilder.Options.ChunkSize == 0 ? 1.5 : 0.5,
                    Opacity = x % _worldBuilder.Options.ChunkSize == 0 ? 0.80 : 0.45
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
                    StrokeThickness = y % _worldBuilder.Options.ChunkSize == 0 ? 1.5 : 0.5,
                    Opacity = y % _worldBuilder.Options.ChunkSize == 0 ? 0.80 : 0.45
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
            _gameControls.ForEach(c => c.IsEnabled = true);
            _gameTimer.Start();
            RenderWorld();
            RefreshCommands();
        }

        private void ClearWorldScope()
        {
            Require.IsNotNull(serviceScope);

            _gameTimer.Stop();
            serviceScope?.Dispose();
            serviceScope = null;
            _gameControls.ForEach(c => c.IsEnabled = false);
            MapCanvas.Children.Clear();
            StatusTextBlock.Text = string.Empty;
            RefreshCommands();
        }

        private void RefreshCommands()
        {
            CreateWorldScopeCommand.RaiseCanExecuteChanged();
            ClearWorldScopeCommand.RaiseCanExecuteChanged();
            BuildDemoTownCommand.RaiseCanExecuteChanged();
            ClearDemoTownCommand.RaiseCanExecuteChanged();
            RedrawCommand.RaiseCanExecuteChanged();
        }

        public sealed class RelayCommand : ICommand
        {
            private readonly Action<object?> _execute;
            private readonly Func<object?, bool>? _canExecute;

            public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return _canExecute?.Invoke(parameter) ?? true;
            }

            public void Execute(object? parameter)
            {
                _execute(parameter);
            }

            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}