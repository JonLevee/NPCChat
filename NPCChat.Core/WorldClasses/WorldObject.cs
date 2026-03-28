using System;

namespace NPCChatLib.WorldClasses
{
    public class WorldObject
    {
        public static readonly WorldObject None = new()
        {
            Kind = WorldObjectKind.None,
            Category = WorldObjectCategory.None,
            Handle = ObjectHandle.None,
            Bounds = Bounds.None
        };

        public WorldObjectKind Kind { get; init; }
        public WorldObjectCategory Category { get; init; }
        public ObjectHandle Handle { get; set; } = ObjectHandle.None;
        public Bounds Bounds { get; set; } = Bounds.None;
    }
}
