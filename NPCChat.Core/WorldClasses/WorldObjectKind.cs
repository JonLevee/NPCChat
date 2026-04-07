using System;

namespace NPCChatLib.WorldClasses
{

    public enum WorldObjectKind : UInt16
    {
        None = 0,
        Building,
        DungeonEntrance,
        Obstacle,
        Waypoint,
        Player,
        NPC,
        Mob
    }
}
