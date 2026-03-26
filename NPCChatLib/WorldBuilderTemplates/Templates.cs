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
        public Templates AddShop(int x, int y, Size size) => Add(WorldObjectCategory.Static, WorldObjectKind.Building, x, y, size);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private Templates Add(WorldObjectCategory category, WorldObjectKind kind, int x, int y, Size size)
        {
            var o = new WorldObject
            {
                Kind = kind,
                Category = category,
                Handle = ObjectHandle.None,
                Bounds = new Bounds(x, y, size)
            };

            builder.World.AddObject(o);
            return this;
        }
    }
}
