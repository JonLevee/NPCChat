using System.Drawing;

namespace NPCChat.Core.AIClasses
{
    /// <summary>
    /// An alert broadcast by one actor so that nearby actors can react on the next tick.
    /// Posted via SimContext.PostAlert; consumed via SimContext.GetNearbyAlerts.
    /// Sim-thread-only.
    /// </summary>
    public readonly struct AlertEvent
    {
        /// <summary>World-grid tile where the alert originated.</summary>
        public Point Position { get; init; }

        public AlertKind Kind { get; init; }

        /// <summary>Simulation tick on which the alert was posted.</summary>
        public int Tick { get; init; }
    }
}
