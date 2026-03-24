using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using NPCChatLib.Extensions;

namespace NPCChatLib.WorldClasses
{
    [DebuggerDisplay("{DebugText}")]
    public class WorldObject : IAutoDebugDisplay
    {
        [DebugDisplay]
        public int Id { get; set; } = -1;
        [DebugDisplay]
        public WorldObjectType Type { get; init; }
        [DebugDisplay]
        public Bounds Bounds { get; set; }
    }

    public interface IAutoDebugDisplay
    {
        string DebugText => this.GetAutoDebugDisplayText();
    }

    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Field,
        AllowMultiple = false,
        Inherited = true)]
    public class DebugDisplayAttribute : Attribute
    {
    }
}
