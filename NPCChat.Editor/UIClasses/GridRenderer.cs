using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using NPCChat.Core.DialogueClasses;
using NPCChat.Core.SupportClasses;
using NPCChat.Core.WorldClasses;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Cursors = System.Windows.Input.Cursors;
using DrawingPoint = System.Drawing.Point;
using Orientation = System.Windows.Controls.Orientation;
using Panel = System.Windows.Controls.Panel;

namespace NPCChat.Editor.UIClasses
{
    public class GridRenderer
    {
        private const int RenderScale = 32;

        private readonly Canvas _mapCanvas;
        private readonly TextBlock _statusTextBlock;
        private readonly ChunkInfo _chunkInfo;

        private readonly Dictionary<ObjectHandle, Polygon> _polygonMap = new();
        private readonly Dictionary<ObjectHandle, TextBlock> _labelMap = new();
        private ObjectHandle _highlightedHandle = ObjectHandle.None;
        private ObjectHandle _selectedHandle = ObjectHandle.None;

        public event Action<WorldObject?>? ObjectHovered;
        public event Action<WorldObject?>? ObjectSelected;

        /// <summary>
        /// Fired when the player clicks an interaction option badge on the canvas.
        /// Parameters: actor handle, the selected option.
        /// </summary>
        public event Action<ObjectHandle, InteractionOption>? InteractionOptionSelected;

        public IsometricTransform? IsoTransform { get; private set; }
        public ObjectHandle SelectedHandle => _selectedHandle;

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
            _labelMap.Clear();
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
                $"   |   Chunks: {world.ChunkCount}" +
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
            _labelMap[obj.Handle] = label;
        }

        private static Brush GetFillBrush(WorldObject obj) =>
            obj.Kind switch
            {
                WorldObjectKind.Building  => CreateBrush(59, 130, 246),
                WorldObjectKind.Player    => CreateBrush(34, 197, 94),
                WorldObjectKind.NPC       => CreateBrush(245, 158, 11),
                WorldObjectKind.Mob       => CreateBrush(239, 68, 68),
                WorldObjectKind.Item      => CreateBrush(250, 204, 21),   // gold-yellow
                WorldObjectKind.Container => CreateBrush(180, 120, 60),   // brown
                _                         => CreateBrush(148, 163, 184)
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
                WorldObjectKind.Item            => "i",
                WorldObjectKind.Container       => "C",
                _                               => "?"
            };

        /// <summary>
        /// Moves moveable polygons and their labels to reflect current world positions.
        /// Called from the UI timer without a full redraw.
        /// </summary>
        public void UpdateMoveablePositions((ObjectHandle Handle, Bounds Bounds)[] positions)
        {
            if (IsoTransform is null) return;
            foreach (var (handle, bounds) in positions)
            {
                if (!_polygonMap.TryGetValue(handle, out var polygon)) continue;

                var top    = IsoTransform.GridToScreen(bounds.Left,  bounds.Top);
                var right  = IsoTransform.GridToScreen(bounds.Right, bounds.Top);
                var bottom = IsoTransform.GridToScreen(bounds.Right, bounds.Bottom);
                var left   = IsoTransform.GridToScreen(bounds.Left,  bounds.Bottom);
                polygon.Points = new PointCollection { top, right, bottom, left };

                if (_labelMap.TryGetValue(handle, out var label))
                {
                    var center = IsoTransform.GridToScreen(
                        (bounds.Left + bounds.Right) / 2.0,
                        (bounds.Top  + bounds.Bottom) / 2.0);
                    Canvas.SetLeft(label, center.X - RenderScale * 0.15);
                    Canvas.SetTop(label,  center.Y - RenderScale * 0.22);
                }
            }
        }

        private const string PathPreviewTag = "PathPreview";

        /// <summary>
        /// Draws path preview lines for the selected object's remaining path steps.
        /// Pass an empty array to clear without drawing.
        /// </summary>
        public void DrawPathPreview(DrawingPoint[] path)
        {
            ClearPathPreview();
            if (IsoTransform is null || path.Length < 2) return;

            for (int i = 0; i < path.Length - 1; i++)
            {
                var from = IsoTransform.GridToScreen(path[i].X + 0.5, path[i].Y + 0.5);
                var to   = IsoTransform.GridToScreen(path[i + 1].X + 0.5, path[i + 1].Y + 0.5);
                _mapCanvas.Children.Add(new Line
                {
                    X1 = from.X, Y1 = from.Y,
                    X2 = to.X,   Y2 = to.Y,
                    Stroke = Brushes.Cyan,
                    StrokeThickness = 1.5,
                    Opacity = 0.65,
                    IsHitTestVisible = false,
                    Tag = PathPreviewTag
                });
            }
        }

        public void ClearPathPreview()
        {
            var toRemove = _mapCanvas.Children
                .OfType<Line>()
                .Where(l => PathPreviewTag.Equals(l.Tag))
                .ToList();
            foreach (var line in toRemove)
                _mapCanvas.Children.Remove(line);
        }

        // ── Interaction overlays ───────────────────────────────────────────────

        private const string InteractionOverlayTag = "InteractionOverlay";

        private static readonly SolidColorBrush OverlayBg     = new(Color.FromArgb(210, 13, 27, 42));
        private static readonly SolidColorBrush OverlayBorder = new(Color.FromArgb(255, 99, 102, 241));
        private static readonly SolidColorBrush OverlayText   = new(Color.FromRgb(147, 197, 253));

        /// <summary>
        /// Renders numbered verb badges above each actor that has visible interactions.
        /// Clicking a badge fires InteractionOptionSelected.
        /// Pass null or an empty array to just clear.
        /// </summary>
        public void UpdateInteractionOverlays(InteractionSnapshot[]? snapshots)
        {
            ClearInteractionOverlays();
            if (IsoTransform is null || snapshots is null || snapshots.Length == 0) return;

            foreach (var snapshot in snapshots)
            {
                if (snapshot.Options.Length == 0) continue;

                // Position the badge row just above the actor's top-centre vertex.
                var topCenter = IsoTransform.GridToScreen(
                    (snapshot.ActorBounds.Left + snapshot.ActorBounds.Right) / 2.0,
                    snapshot.ActorBounds.Top);

                var row = new StackPanel
                {
                    Orientation         = Orientation.Horizontal,
                    Tag                 = InteractionOverlayTag,
                    IsHitTestVisible    = true
                };

                foreach (var option in snapshot.Options)
                {
                    var capturedOption  = option;
                    var capturedHandle  = snapshot.ActorHandle;

                    var badge = new Border
                    {
                        Background      = OverlayBg,
                        BorderBrush     = OverlayBorder,
                        BorderThickness = new Thickness(1),
                        CornerRadius    = new CornerRadius(3),
                        Padding         = new Thickness(5, 2, 5, 2),
                        Margin          = new Thickness(0, 0, 3, 0),
                        Cursor          = Cursors.Hand,
                        Tag             = InteractionOverlayTag,
                        Child = new TextBlock
                        {
                            Text       = $"[{option.Index}] {option.Label}",
                            FontSize   = 10,
                            Foreground = OverlayText
                        }
                    };

                    badge.MouseLeftButtonUp += (_, e) =>
                    {
                        e.Handled = true;
                        InteractionOptionSelected?.Invoke(capturedHandle, capturedOption);
                    };

                    row.Children.Add(badge);
                }

                // Measure the row width so we can roughly centre it.
                row.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                double rowWidth = row.DesiredSize.Width;

                Canvas.SetLeft(row, topCenter.X - rowWidth / 2);
                Canvas.SetTop(row, topCenter.Y - 28);
                Panel.SetZIndex(row, 5);
                _mapCanvas.Children.Add(row);
            }
        }

        public void ClearInteractionOverlays()
        {
            var toRemove = _mapCanvas.Children
                .OfType<UIElement>()
                .Where(e => InteractionOverlayTag.Equals((e as FrameworkElement)?.Tag))
                .ToList();
            foreach (var el in toRemove)
                _mapCanvas.Children.Remove(el);
        }
    }
}
