using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using NPCChatLib.Builders;
using NPCChatLib.WorldBuilderTemplates;
using NPCChatLib.WorldClasses;

namespace NPCChat
{
    public partial class MainWindow : Window
    {
        private readonly WorldData _world;
        private readonly WorldDataBuilder _worldBuilder;
        private bool _isUIReady;

        public MainWindow(WorldData world, WorldDataBuilder worldBuilder)
        {
            InitializeComponent();

            _world = world;
            _worldBuilder = worldBuilder;

            Title = "NPCChat Sandbox - Map View";
            BuildDemoTown();
        }

        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isUIReady)
                return;

            _isUIReady = true;
            RenderWorld();
        }

        private int CellSize => GetCellSize();

        private int GetCellSize()
        {
            if (!int.TryParse(CellSizeBox.Text, out var value))
                value = 24;

            return Math.Max(8, Math.Min(64, value));
        }

        private void BuildDemoTown_Click(object sender, RoutedEventArgs e)
        {
            BuildDemoTown();
        }

        private void Redraw_Click(object sender, RoutedEventArgs e)
        {
            RenderWorld();
        }

        private void CellSizeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUIReady)
            {
                RenderWorld();
            }
        }

        private void BuildDemoTown()
        {
            ClearWorld();

            using var templates = _worldBuilder.GetTemplates();

            templates
                .AddShop(2, 2, BuildingSize.Small)
                .AddShop(10, 2, BuildingSize.Small)
                .AddShop(18, 2, BuildingSize.Small)
                .AddShop(2, 8, new System.Drawing.Size(7, 4))
                .AddShop(12, 9, new System.Drawing.Size(5, 5))
                .AddShop(22, 8, new System.Drawing.Size(8, 4))
                .AddShop(6, 16, new System.Drawing.Size(9, 5))
                .AddShop(20, 16, new System.Drawing.Size(6, 6));

            var player = new WorldObject
            {
                Kind = WorldObjectKind.Player,
                Category = WorldObjectCategory.Dynamic,
                Bounds = new Bounds(16, 14, 17, 15)
            };
            _world.AddObject(player);

            var npcA = new WorldObject
            {
                Kind = WorldObjectKind.Npc,
                Category = WorldObjectCategory.Dynamic,
                Bounds = new Bounds(8, 14, 9, 15)
            };
            _world.AddObject(npcA);

            var npcB = new WorldObject
            {
                Kind = WorldObjectKind.Npc,
                Category = WorldObjectCategory.Dynamic,
                Bounds = new Bounds(24, 14, 25, 15)
            };
            _world.AddObject(npcB);

            RenderWorld();
        }

        private void ClearWorld()
        {
            var handles = EnumerateWorldObjects()
                .Select(x => x.Handle)
                .ToArray();

            foreach (var handle in handles)
            {
                _world.RemoveObject(handle);
            }
        }

        private IReadOnlyList<WorldObject> EnumerateWorldObjects()
        {
            var results = new List<WorldObject>();
            var seen = new HashSet<ObjectHandle>();

            foreach (var chunk in _world.Chunks.Values)
            {
                foreach (var item in chunk.StaticInfos)
                {
                    if (seen.Add(item.Handle) && _world.TryGetObject(item.Handle, out var obj) && obj is not null)
                    {
                        results.Add(obj);
                    }
                }

                foreach (var item in chunk.DynamicInfos)
                {
                    if (seen.Add(item.Handle) && _world.TryGetObject(item.Handle, out var obj) && obj is not null)
                    {
                        results.Add(obj);
                    }
                }
            }

            return results
                .OrderBy(x => x.Category)
                .ThenBy(x => x.Kind)
                .ThenBy(x => x.Bounds.Top)
                .ThenBy(x => x.Bounds.Left)
                .ToArray();
        }

        private void RenderWorld()
        {
            if (!_isUIReady)
                return;

            var objects = EnumerateWorldObjects();
            MapCanvas.Children.Clear();

            const int minCells = 32;
            const int paddingCells = 2;

            int maxRight = objects.Count == 0 ? minCells : Math.Max(minCells, objects.Max(x => x.Bounds.Right) + paddingCells);
            int maxBottom = objects.Count == 0 ? minCells : Math.Max(minCells, objects.Max(x => x.Bounds.Bottom) + paddingCells);

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
    }
}
