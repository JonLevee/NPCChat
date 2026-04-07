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
        private readonly Size CharacterSize = new Size(2, 2);

        public Templates AddPlayer(int x, int y) => Add(WorldObjectCategory.Dynamic, WorldObjectKind.Player, x, y, CharacterSize);
        public Templates AddNPC(int x, int y) => Add(WorldObjectCategory.Dynamic, WorldObjectKind.NPC, x, y, CharacterSize);
        public Templates AddShop(int x, int y, Size size) => Add(WorldObjectCategory.Static, WorldObjectKind.Building, x, y, size);

        public Templates AddSmallTown()
        {
            AddShop(2, 2, BuildingSize.Small);
            AddShop(10, 2, BuildingSize.Small);
            AddShop(18, 2, BuildingSize.Small);
            AddShop(2, 8, BuildingSize.Small);
            AddShop(12, 9, BuildingSize.Small);
            AddShop(22, 8, BuildingSize.Small);
            AddShop(6, 16, BuildingSize.Small);
            AddShop(20, 16, BuildingSize.Small);
            AddPlayer(16, 14);
            AddNPC(8, 14);
            AddNPC(24, 14);

            return this;
        }

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
