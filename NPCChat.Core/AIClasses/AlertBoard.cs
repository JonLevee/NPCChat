using System;
using System.Collections.Generic;
using System.Drawing;

namespace NPCChat.Core.AIClasses
{
    /// <summary>
    /// Double-buffered alert bus for the simulation thread.
    /// Actors post alerts this tick; all actors can read last tick's alerts.
    /// WorldData calls BeginTick() once at the start of each AdvanceBehaviors pass.
    /// </summary>
    public sealed class AlertBoard
    {
        private AlertEvent[] _readBuffer  = [];
        private readonly List<AlertEvent> _writeBuffer = new();

        /// <summary>
        /// Swaps the buffers. Call once at the start of each AdvanceBehaviors pass.
        /// </summary>
        public void BeginTick()
        {
            _readBuffer = _writeBuffer.Count > 0 ? _writeBuffer.ToArray() : [];
            _writeBuffer.Clear();
        }

        /// <summary>
        /// Post an alert. Available to all actors on the NEXT tick.
        /// </summary>
        public void Post(AlertEvent alert) => _writeBuffer.Add(alert);

        /// <summary>
        /// Returns alerts from the previous tick within Chebyshev distance
        /// <paramref name="radius"/> of <paramref name="center"/>.
        /// </summary>
        public AlertEvent[] GetNearbyAlerts(Point center, int radius)
        {
            if (_readBuffer.Length == 0) return [];

            var result = new List<AlertEvent>();
            foreach (var a in _readBuffer)
            {
                int dist = Math.Max(
                    Math.Abs(a.Position.X - center.X),
                    Math.Abs(a.Position.Y - center.Y));
                if (dist <= radius)
                    result.Add(a);
            }
            return result.Count > 0 ? result.ToArray() : [];
        }
    }
}
