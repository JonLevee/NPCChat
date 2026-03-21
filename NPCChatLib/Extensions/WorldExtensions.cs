using NPCChatLib.WorldClasses;
using System;

namespace NPCChatLib.Extensions
{
    public static class WorldExtensions
    {
        public static bool IsStatic(this WorldObject wObject) => wObject.Type < WorldObjectType.Player;
        public static bool IsDynamic(this WorldObject wObject) => wObject.Type >= WorldObjectType.Player;
        public static ChunkCoord ToChunkCoord(this WorldCoord point, IWorldDataOptions options)
        {
            return new ChunkCoord(
                (int)Math.Floor((double)(point.X/options.ChunkSize)), 
                (int)Math.Floor((double)(point.Y/options.ChunkSize))
                );
        }
        public static WorldCoord ToWorldCoord(this ChunkCoord point, IWorldDataOptions options)
        {
            return new WorldCoord(
                point.X * options.ChunkSize,
                point.Y * options.ChunkSize
                );
        }
    }
}
