namespace NPCChat.Core.WorldClasses
{
    /// <summary>
    /// A world object that can be picked up, owned, bought, and sold.
    /// When Owner is null: the item is lying on the map and is stored in chunk StaticInfos
    /// (it acts as a static obstacle for pathfinding).
    /// When Owner is set: the item is being carried and is removed from all chunks entirely
    /// (its position is implicitly that of the owning WorldObjectMoveable).
    /// </summary>
    public sealed class WorldObjectCarryable : WorldObject
    {
        public override WorldObjectCategory Category => WorldObjectCategory.Carryable;

        /// <summary>
        /// The handle of the WorldObjectMoveable currently carrying this item.
        /// Null if the item is lying on the map.
        /// </summary>
        public ObjectHandle? Owner { get; set; }
    }
}
