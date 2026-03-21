using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPCChatLib.Attributes;
using NPCChatLib.Extensions;
using NPCChatLib.LocalEventArgs;
using NPCChatLib.WorldBuilderTemplates;
using NPCChatLib.WorldClasses;
using NPCChatLib.YamlImport;

namespace NPCChatLib.Builders
{
    [Scoped]
    public class WorldDataBuilder(
        BuildingOptions options,
        WorldData world)
    {
        public BuildingOptions Options { get; } = options;
        public WorldData World { get; } = world;

        public void Add(WorldObject wObject, int x, int y)
        {
            throw new NotImplementedException();
        }

        public WorldData Build()
        {
            return World;
        }
    }
}
