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
        /// Stable, non-recycling identifier for this object. Assigned once by ObjectHandleManager
        /// and preserved across save/load cycles. 0 = unassigned.
        /// </summary>
        public int WorldId { get; set; }

        /// <summary>
        /// Inventory attached to this object.
        /// Non-null for players, NPCs, and Container objects; null for everything else.
        /// </summary>
        public InventoryComponent? Inventory { get; set; }
    }
}
