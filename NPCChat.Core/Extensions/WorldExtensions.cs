using NPCChatLib.WorldClasses;

namespace NPCChatLib.Extensions
{
    public static class WorldExtensions
    {
        public static bool CanMove(this WorldObject worldObject)
        {
            return worldObject is WorldObjectMoveable;
        }
    }
}
