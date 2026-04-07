using System;
using System.Windows;
using Point = System.Windows.Point;

namespace NPCChat.Editor
{
    public sealed class IsometricTransform
    {
        private readonly double _halfTileW;
        private readonly double _halfTileH;
        private readonly double _originX;
        private readonly double _originY;

        // cellSize: pixels per grid unit (half-width of one diamond tile)
        // worldMaxBottom: height of world in grid units (used to offset origin so x >= 0)
        public IsometricTransform(int cellSize, int worldMaxBottom, double topPadding = 4)
        {
            _halfTileW = cellSize;
            _halfTileH = cellSize / 2.0;
            _originX = worldMaxBottom * _halfTileW;
            _originY = topPadding;
        }

        public double CanvasWidth(int maxRight, int maxBottom) =>
            (maxRight + maxBottom) * _halfTileW;

        public double CanvasHeight(int maxRight, int maxBottom) =>
            _originY + (maxRight + maxBottom) * _halfTileH;

        // Converts grid coordinates to canvas pixel position.
        // gridX/Y can be fractional (useful for computing diamond centers).
        public Point GridToScreen(double gridX, double gridY) => new(
            _originX + (gridX - gridY) * _halfTileW,
            _originY + (gridX + gridY) * _halfTileH
        );

        // Converts canvas pixel position back to integer grid coordinates.
        public (int gridX, int gridY) ScreenToGrid(Point screen)
        {
            double dx = (screen.X - _originX) / _halfTileW;
            double dy = (screen.Y - _originY) / _halfTileH;
            return (
                (int)Math.Floor((dx + dy) / 2.0),
                (int)Math.Floor((dy - dx) / 2.0)
            );
        }
    }
}
