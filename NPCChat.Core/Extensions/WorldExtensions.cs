using NPCChatLib.WorldClasses;

namespace NPCChatLib.Extensions
{
    public static class WorldExtensions
    {
        private static readonly WorldObjectKind[] MovableKinds = [WorldObjectKind.Player, WorldObjectKind.NPC, WorldObjectKind.Mob,];

        public static bool CanMove(this WorldObject worldObject)
        {
            return worldObject.Category == WorldObjectCategory.Dynamic && MovableKinds.Contains(worldObject.Kind);
        }
    }
}
