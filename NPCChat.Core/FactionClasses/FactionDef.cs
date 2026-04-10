namespace NPCChat.Core.FactionClasses
{
    /// <summary>
    /// Immutable definition of a faction. Registered in StaticData at startup.
    /// </summary>
    public sealed class FactionDef
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;

        // ── Reputation thresholds ─────────────────────────────────────────────
        // Scores are open-ended integers. These thresholds define tier boundaries.

        /// <summary>Minimum score to be considered Friendly (interactions open up).</summary>
        public int FriendlyThreshold { get; init; } = 50;

        /// <summary>Score below which the player is Hostile (interactions locked out).</summary>
        public int HostileThreshold { get; init; } = -25;
    }
}
