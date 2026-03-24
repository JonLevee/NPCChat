using System;
using NPCChatLib.WorldClasses;

namespace NPCChatLib.Extensions
{
    public static class WorldExtensions
    {
        public static bool IsStatic(this WorldObject wObject) => wObject.Type < WorldObjectType.Player;

        public static string GetDescription(this WorldObject o)
        {
            return $"Id: {o.Id}, Type: {o.Type}, Bounds: [{o.Bounds}]";
        }

    }
}
