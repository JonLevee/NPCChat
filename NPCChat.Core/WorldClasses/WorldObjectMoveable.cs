#nullable enable
using NPCChat.Core.BehaviorClasses;
using NPCChat.Core.CharacterClasses;
using NPCChat.Core.QuestClasses;

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

        /// <summary>
        /// Behavior component. Non-null for Actors (NPCs with behaviors).
        /// Null for the player and non-actor moveables.
        /// Owned by the simulation thread.
        /// </summary>
        public ActorComponent? Actor { get; set; }

        /// <summary>
        /// Character data (name, archetype, personality). Non-null for NPCs.
        /// Null for the player and non-character moveables.
        /// </summary>
        public Character? Character { get; set; }

        /// <summary>
        /// Radius (in grid units) within which loose items (Kind == Item) are auto-picked up.
        /// 0 = auto-pickup disabled.
        /// </summary>
        public float ItemPickupRadius { get; init; }

        /// <summary>
        /// Radius (in grid units) within which resource nodes are auto-harvested.
        /// 0 = auto-harvest disabled.
        /// </summary>
        public float ResourcePickupRadius { get; init; }

        /// <summary>
        /// Quest journal. Non-null for the player; null for NPCs and mobs.
        /// Accessed exclusively on the UI thread.
        /// </summary>
        public QuestLog? QuestLog { get; set; }
    }
}
