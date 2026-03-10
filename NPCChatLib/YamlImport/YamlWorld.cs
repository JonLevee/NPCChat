using System;
using System.Collections.Generic;
using System.Drawing;
using NPCChatLib.Attributes;
using NPCChatLib.LocalEventArgs;
using YamlDotNet.Serialization;

namespace NPCChatLib.YamlImport
{
    [Scoped]
    public class YamlWorld
    {
        public event EventHandler<ChangeEventArgs<Size>> WorldSizeChanged;

        private Size worldSize = new(10, 10);
        [YamlMember(Alias = "world_size")]
        public Size WorldSize
        {
            get => worldSize;
            set => ChangeEventArgUpdator.Update(WorldSizeChanged, ref worldSize, value);
        }

        [YamlMember(Alias = "spacing_offset")]
        public int SpacingOffset { get; set; }

        [YamlMember(Alias = "occupied")]
        public YamlOccupied Occupied { get; set; } = new();

        public YamlWorld()
        {
        }
    }
}
