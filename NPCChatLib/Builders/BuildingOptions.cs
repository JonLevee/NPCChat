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

        private NextOpenSpaceLocatorStrategy locatorStrategy = NextOpenSpaceLocatorStrategy.Clockwise;
        public NextOpenSpaceLocatorStrategy LocatorStrategy
        {
            get => locatorStrategy;
            set => ChangeEventArgUpdator.Update(LocatorStrategyChanged, ref locatorStrategy, value);
        }

        private Size worldSize = new Size(10, 10);
        public Size WorldSize
        {
            get => worldSize;
            set => ChangeEventArgUpdator.Update(WorldSizeChanged, ref worldSize, value);
        }

        private Size defaultShopSize = new Size(2, 2);
        public Size DefaultShopSize
        {
            get => defaultShopSize;
            set => ChangeEventArgUpdator.Update(DefaultShopSizeChanged, ref defaultShopSize, value);
        }
        private int defaultPeoplePerShop = 2;
        public int DefaultPeoplePerShop
        {
            get => defaultPeoplePerShop;
            set => ChangeEventArgUpdator.Update(DefaultPeoplePerShopChanged, ref defaultPeoplePerShop, value);
        }

        private int spacingOffset;
        public int SpacingOffset
        {
            get => spacingOffset;
            set => ChangeEventArgUpdator.Update(SpacingOffsetChanged, ref spacingOffset, value);
        }
    }
}
