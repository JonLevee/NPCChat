namespace NPCChat.Core.WorldClasses
{
    /// <summary>
    /// A world object that moves under its own volition: Player, NPC, Mob.
    /// Stored in chunk DynamicInfos. Movement is driven by the background simulation thread.
    /// </summary>
    public sealed class WorldObjectMoveable : WorldObject
    {
        public override WorldObjectCategory Category => WorldObjectCategory.Moveable;

        /// <summary>
        /// Maximum movement speed in world grid units per second.
        /// </summary>
        public float MaxSpeed { get; init; }

        /// <summary>
        /// Current movement state. Owned by the background simulation thread once
        /// movement begins. The UI thread reads position via PositionPatch (Phase 1).
        /// </summary>
        public MovementState Movement { get; } = new();
    }
}
