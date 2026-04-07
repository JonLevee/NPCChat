using NPCChat.Core.Attributes;
using System;
using System.Collections.Generic;

namespace NPCChat.Core.LoadingProviderClasses
{
    [Singleton]
    public class GlobalDataContainer
    {
        public Dictionary<string, byte> DispositionIds { get; set; } = [];
        public byte[] MoodIds { get; set; } = [];
        public byte[] GateIds { get; set; } = [];
    }
}
