using NPCChat.Core.Attributes;
using NPCChat.Core.LoadingProviderClasses;
using NPCChat.Core.WorldBuilderTemplates;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.Builders
{
    [Scoped]
    public class WorldDataBuilder(
        WorldOptions options,
        WorldData world,
        StaticData staticData)
    {
        public WorldOptions Options    { get; } = options;
        public WorldData    World      { get; } = world;
        public StaticData   StaticData { get; } = staticData;

        public Templates GetTemplates() => new(this);
    }
}
