namespace NPCChatLib.WorldClasses
{
    public enum WorldObjectType
    {
        // type < Player = static (buildings, chests, etc.)
        Building,
        DungeonEntrance,
        Obstacle,
        Waypoint,
        // type >= Player = dynamic (npc's, mobs, things that can move)
        Player,
        Npc,
        Mob,

    }

}
