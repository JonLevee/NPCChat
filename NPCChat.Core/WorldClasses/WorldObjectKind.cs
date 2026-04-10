using System;

namespace NPCChat.Core.WorldClasses
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
        Mob,
        Item,
        Container
    }
}
