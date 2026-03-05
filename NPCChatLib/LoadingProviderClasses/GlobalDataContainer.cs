using NPCChatLib.Attributes;
using System;
using System.Collections.Generic;

namespace NPCChatLib.LoadingProviderClasses
{
    [Singleton]
    public class GlobalDataContainer
    {
        public Dictionary<string, byte> DispositionIds { get; set; } = [];
        public byte[] MoodIds { get; set; } = [];
        public byte[] GateIds { get; set; } = [];
    }
}
