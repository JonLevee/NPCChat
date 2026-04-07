namespace NPCChat.Core.WorldClasses
{
    public abstract class WorldObject
    {
        public abstract WorldObjectCategory Category { get; }
        public WorldObjectKind Kind { get; init; }
        public ObjectHandle Handle { get; set; } = ObjectHandle.None;
        public Bounds Bounds { get; set; } = Bounds.None;
    }
}
