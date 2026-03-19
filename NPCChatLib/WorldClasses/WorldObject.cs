namespace NPCChatLib.WorldClasses
{
    public sealed class WorldObject
    {
        public int Id { get; init; }
        public WorldObjectType Type { get; init; }
        public Bounds Bounds { get; set; }
        public bool IsStatic { get; init; }
    }

}
