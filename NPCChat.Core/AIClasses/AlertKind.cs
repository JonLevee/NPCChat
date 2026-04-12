namespace NPCChat.Core.AIClasses
{
    public enum AlertKind
    {
        /// <summary>An actor has sighted a hostile entity at the given position.</summary>
        PlayerDetected,
        /// <summary>A loud sound was heard at the given position.</summary>
        NoiseHeard,
        /// <summary>A melee attack was landed at the given position.</summary>
        CombatNoise
    }
}
