using NPCChatLib.Attributes;
using NPCChatLib.WorldBuilderTemplates;
using NPCChatLib.WorldClasses;

namespace NPCChatLib.Builders
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
