using System.Collections.Generic;
using System.Drawing;
using YamlDotNet.Serialization;

namespace NPCChatLib.YamlImport
{
    public class YamlWorld
    {
        [YamlMember(Alias = "world_size")]
        public Size WorldSize { get; set; } = new Size(10, 10);

        [YamlMember(Alias = "spacing_offset")]
        public int SpacingOffset { get; set; }

        [YamlMember(Alias = "occupied")]
        public YamlOccupied Occupied { get; set; } = [];

        public YamlWorld()
        {
        }
    }
}
