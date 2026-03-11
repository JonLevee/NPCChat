using System;
using System.Drawing;
using NPCChatLib.Attributes;
using NPCChatLib.LocalEventArgs;
using NPCChatLib.YamlImport;
using YamlDotNet.Serialization;

namespace NPCChatLib.Builders
{
    [Scoped]
    public class BuildingOptions
    {
        public event EventHandler<ChangeEventArgs<NextOpenSpaceLocatorStrategy>> LocatorStrategyChanged;
        public event EventHandler<ChangeEventArgs<Size>> WorldSizeChanged;
        public event EventHandler<ChangeEventArgs<Size>> DefaultShopSizeChanged;
        public event EventHandler<ChangeEventArgs<int>> DefaultPeoplePerShopChanged;
        public event EventHandler<ChangeEventArgs<int>> SpacingOffsetChanged;

        private readonly YamlWorld yamlWorld;
        private NextOpenSpaceLocatorStrategy locatorStrategy = NextOpenSpaceLocatorStrategy.Clockwise;
        private Size worldSize = new Size(10, 10);
        private Size defaultShopSize = new Size(2, 2);
        private int defaultPeoplePerShop = 2;
        private int spacingOffset = 3;

        public BuildingOptions(YamlWorld yamlWorld)
        {
            this.yamlWorld = yamlWorld;
            yamlWorld.WorldSizeChanged += WorldSizeChanged;
            WorldSizeChanged += (sender, args) => yamlWorld.WorldSize = args.NewValue;
            SpacingOffsetChanged += (sender, args) => yamlWorld.SpacingOffset = args.NewValue;
        }

        public NextOpenSpaceLocatorStrategy LocatorStrategy
        {
            get => locatorStrategy;
            set => ChangeEventArgUpdator.Update(LocatorStrategyChanged, ref locatorStrategy, value);
        }

        public Size WorldSize
        {
            get => worldSize;
            set => ChangeEventArgUpdator.Update(WorldSizeChanged, ref worldSize, value);
        }

        public Size DefaultShopSize
        {
            get => defaultShopSize;
            set => ChangeEventArgUpdator.Update(DefaultShopSizeChanged, ref defaultShopSize, value);
        }
        public int DefaultPeoplePerShop
        {
            get => defaultPeoplePerShop;
            set => ChangeEventArgUpdator.Update(DefaultPeoplePerShopChanged, ref defaultPeoplePerShop, value);
        }

        public int SpacingOffset
        {
            get => spacingOffset;
            set => ChangeEventArgUpdator.Update(SpacingOffsetChanged, ref spacingOffset, value);
        }
    }
}
