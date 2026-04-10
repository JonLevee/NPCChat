#nullable enable
using NPCChat.Core.InventoryClasses;

namespace NPCChat.Core.WorldClasses
{
    public abstract class WorldObject
    {
        public abstract WorldObjectCategory Category { get; }
        public WorldObjectKind Kind { get; init; }
        public ObjectHandle Handle { get; set; } = ObjectHandle.None;
        public Bounds Bounds { get; set; } = Bounds.None;

        /// <summary>
        /// Inventory attached to this object.
        /// Non-null for players, NPCs, and Container objects; null for everything else.
        /// </summary>
        public InventoryComponent? Inventory { get; set; }
    }
}
