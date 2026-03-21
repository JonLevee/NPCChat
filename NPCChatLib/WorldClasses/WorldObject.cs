using System.Drawing;

namespace NPCChatLib.WorldClasses
{
    public class WorldObject
    {
        public int Id { get; set; } = -1;
        public WorldObjectType Type { get; init; }
        public WorldCoord Coord { get; set; }
        public ChunkCoord ChunkCoord { get; set; }
        public Size Size { get; init; }
    }
}
