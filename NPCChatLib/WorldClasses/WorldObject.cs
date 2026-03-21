namespace NPCChatLib.WorldClasses
{
    public class WorldObject
    {
        public int Id { get; init; }
        public WorldObjectType Type { get; init; }
        public Bounds Bounds { get; set; }
    }
}
