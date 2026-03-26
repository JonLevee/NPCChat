using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ABI.Windows.Foundation;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using NPCChatLib.Builders;
using NPCChatLib.WorldBuilderTemplates;
using NPCChatLib.WorldClasses;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NPCChat
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
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

        private int CellSize => Math.Max(8, (int)Math.Round(CellSizeBox.Value));

        private void BuildDemoTown_Click(object sender, RoutedEventArgs e)
        {
            BuildDemoTown();
        }

        private void Redraw_Click(object sender, RoutedEventArgs e)
        {
            RenderWorld();
        }

        private void CellSizeBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            RenderWorld();
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
            MapCanvas.Background = new SolidColorBrush(Colors.Transparent);

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
                    Stroke = new SolidColorBrush(ColorHelper.FromArgb(255, 45, 55, 72)),
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
                    Stroke = new SolidColorBrush(ColorHelper.FromArgb(255, 45, 55, 72)),
                    StrokeThickness = y % _worldBuilder.Options.ChunkSize == 0 ? 1.5 : 0.5,
                    Opacity = y % _worldBuilder.Options.ChunkSize == 0 ? 0.80 : 0.45
                });
            }
        }

        private void DrawObject(WorldObject obj)
        {
            var fill = GetFillBrush(obj);
            var stroke = new SolidColorBrush(Colors.Black);

            var rectangle = new Rectangle
            {
                Width = Math.Max(1, obj.Bounds.Width * CellSize),
                Height = Math.Max(1, obj.Bounds.Height * CellSize),
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = 1,
                RadiusX = obj.Category == WorldObjectCategory.Dynamic ? 8 : 2,
                RadiusY = obj.Category == WorldObjectCategory.Dynamic ? 8 : 2
            };

            ToolTipService.SetToolTip(rectangle, $"{obj.Kind}\n{obj.Category}\n{obj.Bounds}\n{obj.Handle}");

            Canvas.SetLeft(rectangle, obj.Bounds.Left * CellSize);
            Canvas.SetTop(rectangle, obj.Bounds.Top * CellSize);
            Canvas.SetZIndex(rectangle, obj.Category == WorldObjectCategory.Dynamic ? 10 : 1);
            MapCanvas.Children.Add(rectangle);

            var label = new TextBlock
            {
                Text = GetLabel(obj),
                FontSize = Math.Max(10, CellSize * 0.42),
                Foreground = new SolidColorBrush(Colors.White),
                IsHitTestVisible = false
            };

            Canvas.SetLeft(label, obj.Bounds.Left * CellSize + 4);
            Canvas.SetTop(label, obj.Bounds.Top * CellSize + 2);
            Canvas.SetZIndex(label, obj.Category == WorldObjectCategory.Dynamic ? 11 : 2);
            MapCanvas.Children.Add(label);
        }

        private static Brush GetFillBrush(WorldObject obj)
        {
            return obj.Kind switch
            {
                WorldObjectKind.Building => new SolidColorBrush(ColorHelper.FromArgb(255, 59, 130, 246)),
                WorldObjectKind.Player => new SolidColorBrush(ColorHelper.FromArgb(255, 34, 197, 94)),
                WorldObjectKind.Npc => new SolidColorBrush(ColorHelper.FromArgb(255, 245, 158, 11)),
                WorldObjectKind.Mob => new SolidColorBrush(ColorHelper.FromArgb(255, 239, 68, 68)),
                _ => new SolidColorBrush(ColorHelper.FromArgb(255, 148, 163, 184))
            };
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
