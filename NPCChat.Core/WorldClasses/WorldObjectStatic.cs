namespace NPCChatLib.WorldClasses
{
    /// <summary>
    /// A world object that never moves: buildings, obstacles, waypoints, dungeon entrances.
    /// Stored in chunk StaticInfos (bounds cached in chunk for fast lookup).
    /// </summary>
    public sealed class WorldObjectStatic : WorldObject
    {
        public override WorldObjectCategory Category => WorldObjectCategory.Static;

        /// <summary>
        /// The handle of the WorldObjectMoveable that owns this object, if any.
        /// Example: a blacksmith shop owned by its NPC proprietor.
        /// Null if unowned.
        /// </summary>
        public ObjectHandle? Owner { get; set; }
    }
}
