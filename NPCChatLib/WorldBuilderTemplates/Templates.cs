using System;
using System.Drawing;
using NPCChatLib.Attributes;
using NPCChatLib.Builders;
using NPCChatLib.WorldClasses;

namespace NPCChatLib.WorldBuilderTemplates
{
    public static class BuildingSize
    {
        /// <summary>
        /// Width:5, Height: 3
        /// </summary>
        public readonly static Size Small = new(5, 3);
    }

    [Transient]
    public partial class Templates(WorldDataBuilder builder) : IDisposable
    {
        public Templates AddShop(int x, int y, Size size) => Add(WorldObjectType.Building, x, y, size);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private Templates Add(WorldObjectType type, int x, int y, Size size)
        {
            var o = new WorldObject
            {
                Id = builder.Options.GetNextId(),
                Type = type,
                Bounds = new(x, y, size),
            };
            builder.World.Add(o);
            return this;
        }
    }
}
