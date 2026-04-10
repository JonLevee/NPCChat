using NPCChat.Core.Attributes;
using NPCChat.Core.WorldBuilderTemplates;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.Builders
{
    [Scoped]
    public class WorldDataBuilder(
        WorldOptions options,
        WorldData world)
    {

        public WorldOptions Options { get; } = options;
        public WorldData World { get; } = world;
        public Templates GetTemplates() => new(this);
    }
}
