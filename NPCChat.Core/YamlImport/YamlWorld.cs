using System;
using System.Collections.Generic;
using System.Drawing;
using NPCChat.Core.Attributes;
using NPCChat.Core.LocalEventArgs;
using YamlDotNet.Serialization;

namespace NPCChat.Core.YamlImport
{
    [Scoped]
    public class YamlWorld
    {
        public event EventHandler<ChangeEventArgs<Size>> WorldSizeChanged;
        public event EventHandler<ChangeEventArgs<int>> SpacingOffsetChanged;

        private Size worldSize = new(10, 10);
        private int spacingOffset = 3;

        [YamlMember(Alias = "world_size")]
        public Size WorldSize
        {
            get => worldSize;
            set => ChangeEventArgUpdator.Update(WorldSizeChanged, ref worldSize, value);
        }

        [YamlMember(Alias = "spacing_offset")]
        public int SpacingOffset
        {
            get => spacingOffset;
            set => ChangeEventArgUpdator.Update(SpacingOffsetChanged, ref spacingOffset, value);
        }


        [YamlMember(Alias = "occupied")]
        public YamlOccupied Occupied { get; set; } = new();

        public YamlWorld()
        {
        }
    }
}
