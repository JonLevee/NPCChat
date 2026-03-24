using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPCChatLib.Attributes;
using NPCChatLib.Exceptions;
using NPCChatLib.Extensions;
using NPCChatLib.LocalEventArgs;
using NPCChatLib.WorldBuilderTemplates;
using NPCChatLib.WorldClasses;
using NPCChatLib.YamlImport;

namespace NPCChatLib.Builders
{
    [Scoped]
    public class WorldDataBuilder(
        WorldDataOptions options,
        WorldData world)
    {

        public WorldDataOptions Options { get; } = options;
        public WorldData World { get; } = world;
        public Templates GetTemplates() => new(this);
    }
}
