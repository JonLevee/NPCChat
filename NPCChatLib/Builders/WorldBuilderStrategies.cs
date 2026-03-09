using System.Drawing;
using NPCChatLib.Attributes;

namespace NPCChatLib.Builders
{
    [Transient]
    public class WorldBuilderStrategies
    {
        public NextOpenSpaceLocatorStrategy LocatorStrategy = NextOpenSpaceLocatorStrategy.Clockwise;
        public Size WorldSize { get; set; } = new Size(10, 10);
        public Size DefaultShopSize { get; set; } = new Size(2, 2);
        public int DefaultPeoplePerShop { get; set; } = 2;
    }
}
