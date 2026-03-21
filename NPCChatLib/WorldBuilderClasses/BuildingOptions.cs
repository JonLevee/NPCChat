using System.Drawing;
using NPCChatLib.Attributes;

namespace NPCChatLib.Builders
{
    [Scoped]
    public class BuildingOptions
    {
        public Size WorldSize { get; set; } = new Size(10, 10);

        public BuildingOptions()
        {
        }
    }
}
