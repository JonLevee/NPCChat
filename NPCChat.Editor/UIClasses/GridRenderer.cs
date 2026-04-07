using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using NPCChat.Core.SupportClasses;
using NPCChat.Core.WorldClasses;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

namespace NPCChat.Editor.UIClasses
{
    public class GridRenderer
    {
        private const int RenderScale = 32;

        private readonly Canvas _mapCanvas;
        private readonly TextBlock _statusTextBlock;
        private readonly ChunkInfo _chunkInfo;

        private readonly Dictionary<ObjectHandle, Polygon> _polygonMap = new();
        private ObjectHandle _highlightedHandle = ObjectHandle.None;
        private ObjectHandle _selectedHandle = ObjectHandle.None;

        public event Action<WorldObject?>? ObjectHovered;
        public event Action<WorldObject?>? ObjectSelected;

        public IsometricTransform? IsoTransform { get; private set; }

        public GridRenderer(MainWindow window, ChunkInfo chunkInfo)
        {
            _mapCanvas = window.MapCanvas;
            _statusTextBlock = window.StatusTextBlock;
            _chunkInfo = chunkInfo;

            _mapCanvas.MouseLeftButtonUp += (s, e) => ClearSelection();
        }

        // Called from CharacterSummary MouseEnter
        public void HighlightObject(ObjectHandle handle) => HighlightPolygon(handle);

        // Called from CharacterSummary MouseLeave
        public void ClearHighlight() => ClearHoverHighlight();

        // Called externally to clear selection (e.g. on world rebuild)
        public void ClearSelection()
        {
            if (_selectedHandle != ObjectHandle.None && _selectedHandle != _highlightedHandle)
                SetPolygonNormal(_selectedHandle);
            _selectedHandle = ObjectHandle.None;
            ObjectSelected?.Invoke(null);
        }

        private void HighlightPolygon(ObjectHandle handle)
        {
            if (_highlightedHandle != ObjectHandle.None && _highlightedHandle != _selectedHandle)
                SetPolygonNormal(_highlightedHandle);

            if (_polygonMap.TryGetValue(handle, out var polygon))
            {
                polygon.Stroke = Brushes.White;
                polygon.StrokeThickness = 2.5;
            }
            _highlightedHandle = handle;
        }

        private void ClearHoverHighlight()
        {
            if (_highlightedHandle != ObjectHandle.None && _highlightedHandle != _selectedHandle)
                SetPolygonNormal(_highlightedHandle);
            _highlightedHandle = ObjectHandle.None;
        }

        private void ApplySelection(ObjectHandle handle)
        {
            if (_selectedHandle != ObjectHandle.None && _selectedHandle != _highlightedHandle)
                SetPolygonNormal(_selectedHandle);

            if (_polygonMap.TryGetValue(handle, out var polygon))
            {
                polygon.Stroke = Brushes.White;
                polygon.StrokeThickness = 2.5;
            }
            _selectedHandle = handle;
            ObjectSelected?.Invoke((WorldObject)_polygonMap[handle].Tag!);
        }

        private void SetPolygonNormal(ObjectHandle handle)
        {
            if (_polygonMap.TryGetValue(handle, out var polygon))
            {
                var obj = (WorldObject)polygon.Tag!;
                polygon.Stroke = Brushes.Black;
                polygon.StrokeThickness = obj is WorldObjectMoveable ? 1.5 : 1.0;
            }
        }

        public void RenderWorld(WorldData world)
        {
            var objects = world.EnumerateWorldObjects();
            _polygonMap.Clear();
            _highlightedHandle = ObjectHandle.None;
            _selectedHandle = ObjectHandle.None;
            _mapCanvas.Children.Clear();

            const int minCells = 32;
            const int paddingCells = 2;

            int maxRight = objects.Count == 0
                ? minCells
                : Math.Max(minCells, objects.Max(x => x.Bounds.Right) + paddingCells);

            int maxBottom = objects.Count == 0
                ? minCells
                : Math.Max(minCells, objects.Max(x => x.Bounds.Bottom) + paddingCells);

            IsoTransform = new IsometricTransform(RenderScale, maxBottom);

            _mapCanvas.Width = IsoTransform.CanvasWidth(maxRight, maxBottom);
            _mapCanvas.Height = IsoTransform.CanvasHeight(maxRight, maxBottom);
            _mapCanvas.Background = Brushes.Transparent;

            DrawGrid(maxRight, maxBottom);

            var sorted = objects
                .OrderBy(o => o.Bounds.Right + o.Bounds.Bottom)
                .ThenBy(o => o is WorldObjectMoveable ? 1 : 0);

            foreach (var obj in sorted)
                DrawObject(obj);

            _statusTextBlock.Text =
                $"Objects: {objects.Count}" +
                $"   |   Static: {objects.Count(x => x is WorldObjectStatic)}" +
                $"   |   Moveable: {objects.Count(x => x is WorldObjectMoveable)}" +
                $"   |   Chunks: {world.Chunks.Count}" +
                $"   |   ChunkSize: {_chunkInfo.ChunkSize}";
        }

        private void DrawGrid(int widthInCells, int heightInCells)
        {
            int chunkSize = _chunkInfo.ChunkSize;

            for (int x = 0; x <= widthInCells; x++)
            {
                var from = IsoTransform!.GridToScreen(x, 0);
                var to = IsoTransform.GridToScreen(x, heightInCells);
                bool isChunk = x % chunkSize == 0;
                _mapCanvas.Children.Add(new Line
                {
                    X1 = from.X, Y1 = from.Y,
                    X2 = to.X,   Y2 = to.Y,
                    Stroke = CreateBrush(45, 55, 72),
                    StrokeThickness = isChunk ? 1.5 : 0.5,
                    Opacity = isChunk ? 0.80 : 0.45
                });
            }

            for (int y = 0; y <= heightInCells; y++)
            {
                var from = IsoTransform!.GridToScreen(0, y);
                var to = IsoTransform.GridToScreen(widthInCells, y);
                bool isChunk = y % chunkSize == 0;
                _mapCanvas.Children.Add(new Line
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
            var top    = IsoTransform!.GridToScreen(obj.Bounds.Left,  obj.Bounds.Top);
            var right  = IsoTransform.GridToScreen(obj.Bounds.Right, obj.Bounds.Top);
            var bottom = IsoTransform.GridToScreen(obj.Bounds.Right, obj.Bounds.Bottom);
            var left   = IsoTransform.GridToScreen(obj.Bounds.Left,  obj.Bounds.Bottom);

            var diamond = new Polygon
            {
                Points = new PointCollection { top, right, bottom, left },
                Fill = GetFillBrush(obj),
                Stroke = Brushes.Black,
                StrokeThickness = obj is WorldObjectMoveable ? 1.5 : 1.0,
                ToolTip = $"{obj.Kind}\n{obj.Category}\n{obj.Bounds}\n{obj.Handle}",
                Tag = obj
            };
            diamond.MouseEnter += (s, e) => { HighlightPolygon(obj.Handle); ObjectHovered?.Invoke(obj); };
            diamond.MouseLeave += (s, e) => { ClearHoverHighlight(); ObjectHovered?.Invoke(null); };
            diamond.MouseLeftButtonUp += (s, e) => { e.Handled = true; ApplySelection(obj.Handle); };
            _polygonMap[obj.Handle] = diamond;
            _mapCanvas.Children.Add(diamond);

            var center = IsoTransform.GridToScreen(
                (obj.Bounds.Left + obj.Bounds.Right) / 2.0,
                (obj.Bounds.Top  + obj.Bounds.Bottom) / 2.0);

            var label = new System.Windows.Controls.TextBlock
            {
                Text = GetLabel(obj),
                FontSize = Math.Max(9, RenderScale * 0.38),
                Foreground = Brushes.White,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(label, center.X - RenderScale * 0.15);
            Canvas.SetTop(label,  center.Y - RenderScale * 0.22);
            _mapCanvas.Children.Add(label);
        }

        private static Brush GetFillBrush(WorldObject obj) =>
            obj.Kind switch
            {
                WorldObjectKind.Building => CreateBrush(59, 130, 246),
                WorldObjectKind.Player   => CreateBrush(34, 197, 94),
                WorldObjectKind.NPC      => CreateBrush(245, 158, 11),
                WorldObjectKind.Mob      => CreateBrush(239, 68, 68),
                _                        => CreateBrush(148, 163, 184)
            };

        private static SolidColorBrush CreateBrush(byte r, byte g, byte b) =>
            new(Color.FromArgb(255, r, g, b));

        private static string GetLabel(WorldObject obj) =>
            obj.Kind switch
            {
                WorldObjectKind.Building        => "B",
                WorldObjectKind.Player          => "P",
                WorldObjectKind.NPC             => "N",
                WorldObjectKind.Mob             => "M",
                WorldObjectKind.DungeonEntrance => "D",
                _                               => "?"
            };
    }
}
