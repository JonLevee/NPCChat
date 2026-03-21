using NPCChatLib.WorldClasses;

namespace NPCChatLib.Extensions
{
    public static class WorldExtensions
    {
        public static bool IsStatic(this WorldObject wObject) => wObject.Type < WorldObjectType.Player;
        public static bool IsDynamic(this WorldObject wObject) => wObject.Type >= WorldObjectType.Player;
    }
}
