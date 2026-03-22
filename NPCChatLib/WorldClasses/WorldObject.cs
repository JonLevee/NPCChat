using System.Drawing;

namespace NPCChatLib.WorldClasses
{
    public class WorldObject
    {
        public int Id { get; set; } = -1;
        public WorldObjectType Type { get; init; }
        public Position Position { get; set; }
        public Size Size { get; init; }
    }
}
