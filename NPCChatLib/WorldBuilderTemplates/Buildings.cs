using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Documents;
using NPCChatLib.Attributes;
using NPCChatLib.Builders;
using NPCChatLib.Extensions;
using NPCChatLib.WorldClasses;
using Windows.Media.Devices;

namespace NPCChatLib.WorldBuilderTemplates
{

    [Transient]
    public partial class Templates(WorldDataBuilder builder) : IDisposable
    {
        public Templates AddShopSmall(int x, int y) => Add(WorldObjectType.Building, x, y, 5, 3);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private Templates Add(WorldObjectType type, int x, int y, int width, int height)
        {
            var o = new WorldObject
            {
                Id = builder.Options.GetNextId(),
                Type = type,
                Bounds = new(X: x, Y: y, Width: width, Height: height),
            };
            (var chunkPos, var primary, var nonPrimary) = builder.World.GetChunkAndIndexes(o);
            if (nonPrimary.TryGetConflicts(o.Bounds, out var conflicts))
            {
                var confictText = string.Join("\r\n", conflicts.Select(c => c.GetDescription()));
                throw new InvalidOperationException($"new object {o.GetDescription()} conflicts with:\r\n{confictText}");
            }
            // todo
            if (primary.)
                return this;
        }
    }
}
