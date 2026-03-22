using System;
using System.Collections.Generic;
using NPCChatLib.WorldClasses;

namespace NPCChatLib.Extensions
{
    public static class WorldExtensions
    {
        public static bool IsStatic(this WorldObject wObject) => wObject.Type < WorldObjectType.Player;
        public static bool IsDynamic(this WorldObject wObject) => wObject.Type >= WorldObjectType.Player;
        public static ChunkCoord ToChunkCoord(this WorldCoord point, WorldDataOptions options)
        {
            return new ChunkCoord(
                (int)Math.Floor((double)(point.X / options.ChunkSize)),
                (int)Math.Floor((double)(point.Y / options.ChunkSize))
                );
        }
        public static WorldCoord ToWorldCoord(this ChunkCoord point, WorldDataOptions options)
        {
            return new WorldCoord(
                point.X * options.ChunkSize,
                point.Y * options.ChunkSize
                );
        }


        public static bool TryAdd(this WorldData world, WorldObject o, out WorldObject conflict)
        {

        }
        public static BoundInfos GetBounds(this WorldData world, WorldObject o)
        {
            var index = (o.IsStatic() ? world.StaticIndex : world.DynamicIndex);

            if (!index.TryGetValue(o.ChunkCoord, out BoundInfos list))
            {
                list = [];
                index[o.ChunkCoord] = list;
            }
            return list;
        }

    }
}
