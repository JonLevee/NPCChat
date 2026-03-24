using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using NPCChatLib.Attributes;
using NPCChatLib.Extensions;

namespace NPCChatLib.WorldClasses
{
    public class WorldObject
    {
        public int Id { get; set; } = -1;
        public WorldObjectType Type { get; init; }
        public Bounds Bounds { get; set; }
    }


}
