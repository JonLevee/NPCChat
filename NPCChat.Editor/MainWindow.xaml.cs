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
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace NPCChat
{
    public partial class MainWindow : Window
    {
        private WorldData _world = null!;
        private WorldDataBuilder _worldBuilder = null!;
        private IServiceScope serviceScope = null!;

        private readonly DispatcherTimer _gameTimer = new();

        private bool _autoStartGame = true;
        private string originalTitle = string.Empty;
        private Point lastMousePosition;
        private Point lastMouseDownPosition;
        private IsometricTransform _isoTransform = null!;

        private readonly UserSettingsRepository _userSettingsRepository;


        public MainWindow(UserSettingsRepository userSettingsRepository)
        {
            _userSettingsRepository = userSettingsRepository;
            InitializeComponent();

            Loaded += (s, e) => _userSettingsRepository.RestoreWindow(this);
            Closing += (s, e) => _userSettingsRepository.SaveWindow(this);

            originalTitle = Title;
            this.LocationChanged += (s, e) => UpdateTitle();
            this.SizeChanged += (s, e) => UpdateTitle();

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

        private void UpdateTitle()
        {
            Title = $"{originalTitle} - {_userSettingsRepository.GetUserSettingsText(this)} Clicked = {GridCoordLabel(lastMouseDownPosition)} Mouse = {GridCoordLabel(lastMousePosition)}";
        }

        private void _gameTimer_Tick(object? sender, EventArgs e)
        {
            if (_autoStartGame)
            {
                _autoStartGame = false;
                StartStopButton_Click(this, new RoutedEventArgs());
                BuildDemoTownButton_Click(this, new RoutedEventArgs());
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

            _isoTransform = new IsometricTransform(CellSize, maxBottom);

            MapCanvas.Width = _isoTransform.CanvasWidth(maxRight, maxBottom);
            MapCanvas.Height = _isoTransform.CanvasHeight(maxRight, maxBottom);
            MapCanvas.Background = Brushes.Transparent;

            DrawGrid(maxRight, maxBottom);

            // Painter's algorithm: draw back-to-front so near objects overlap far ones.
            // Static objects first, dynamic always on top at same depth.
            var sorted = objects
                .OrderBy(o => o.Bounds.Right + o.Bounds.Bottom)
                .ThenBy(o => o.Category == WorldObjectCategory.Dynamic ? 1 : 0);

            foreach (var obj in sorted)
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
            int chunkSize = _worldBuilder.Options.ChunkInfo.ChunkSize;

            // Diagonal lines running down-left (constant gridX columns)
            for (int x = 0; x <= widthInCells; x++)
            {
                var from = _isoTransform.GridToScreen(x, 0);
                var to   = _isoTransform.GridToScreen(x, heightInCells);
                bool isChunk = x % chunkSize == 0;
                MapCanvas.Children.Add(new Line
                {
                    X1 = from.X, Y1 = from.Y,
                    X2 = to.X,   Y2 = to.Y,
                    Stroke = CreateBrush(45, 55, 72),
                    StrokeThickness = isChunk ? 1.5 : 0.5,
                    Opacity = isChunk ? 0.80 : 0.45
                });
            }

            // Diagonal lines running down-right (constant gridY rows)
            for (int y = 0; y <= heightInCells; y++)
            {
                var from = _isoTransform.GridToScreen(0, y);
                var to   = _isoTransform.GridToScreen(widthInCells, y);
                bool isChunk = y % chunkSize == 0;
                MapCanvas.Children.Add(new Line
                {
                    X1 = from.X, Y1 = from.Y,
                    X2 = to.X,   Y2 = to.Y,
                    Stroke = CreateBrush(45, 55, 72),
                    StrokeThickness = isChunk ? 1.5 : 0.5,
                    Opacity = isChunk ? 0.80 : 0.45
                });
            }
        }

        private void DrawObject(WorldObject obj)
        {
            // Four corners of the grid-aligned rectangle become the four diamond vertices.
            // Order: top, right, bottom, left — forming a clockwise diamond.
            // When height/walls are added later, this top-face footprint stays and
            // wall geometry is drawn between this and a lower parallel diamond.
            var top    = _isoTransform.GridToScreen(obj.Bounds.Left,  obj.Bounds.Top);
            var right  = _isoTransform.GridToScreen(obj.Bounds.Right, obj.Bounds.Top);
            var bottom = _isoTransform.GridToScreen(obj.Bounds.Right, obj.Bounds.Bottom);
            var left   = _isoTransform.GridToScreen(obj.Bounds.Left,  obj.Bounds.Bottom);

            var diamond = new Polygon
            {
                Points = new PointCollection { top, right, bottom, left },
                Fill = GetFillBrush(obj),
                Stroke = Brushes.Black,
                StrokeThickness = obj.Category == WorldObjectCategory.Dynamic ? 1.5 : 1.0,
                ToolTip = $"{obj.Kind}\n{obj.Category}\n{obj.Bounds}\n{obj.Handle}"
            };
            MapCanvas.Children.Add(diamond);

            // Label at the visual center of the diamond
            var center = _isoTransform.GridToScreen(
                (obj.Bounds.Left + obj.Bounds.Right) / 2.0,
                (obj.Bounds.Top  + obj.Bounds.Bottom) / 2.0);

            var label = new TextBlock
            {
                Text = GetLabel(obj),
                FontSize = Math.Max(9, CellSize * 0.38),
                Foreground = Brushes.White,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(label, center.X - CellSize * 0.15);
            Canvas.SetTop(label,  center.Y - CellSize * 0.22);
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

        private void MapCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            lastMouseDownPosition = e.GetPosition(this.MapCanvas);
            UpdateTitle();
        }

        private void MapCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lastMousePosition = e.GetPosition(this.MapCanvas);
            UpdateTitle();
        }

        private string GridCoordLabel(Point screenPoint)
        {
            if (_isoTransform is null) return screenPoint.ToString();
            var (gx, gy) = _isoTransform.ScreenToGrid(screenPoint);
            return $"({gx}, {gy})";
        }
    }
}