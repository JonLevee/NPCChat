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
using NPCChatLib.YamlImport;

namespace NPCChatLib.Builders
{
    [Scoped]
    public class WorldBuilder
    {
        public YamlWorld World { get; }
        public BuildingOptions Options { get; }
        public NextOpenSpaceLocator SpaceLocator { get; }

        public WorldBuilder(BuildingOptions options, YamlWorld yamlWorld, NextOpenSpaceLocator spaceLocator)
        {
            Options = options;
            World = yamlWorld;
            SpaceLocator = spaceLocator;
        }

        public WorldBuilder SetOptions(Action<BuildingOptions> setFunc)
        {
            setFunc(Options);
            return this;
        }

        public YamlWorld Build()
        {
            return World;
        }

        internal void CreateShopkeeper(YamlBuilding building)
        {
            throw new NotImplementedException();
        }

        internal void CreateShopkeeperAssistant(YamlBuilding building)
        {
            throw new NotImplementedException();
        }
    }
}
