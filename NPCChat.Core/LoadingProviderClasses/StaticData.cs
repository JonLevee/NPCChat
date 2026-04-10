#nullable enable
using NPCChat.Core.Attributes;
using NPCChat.Core.ItemClasses;
using NPCChat.Core.QuestClasses;

namespace NPCChat.Core.LoadingProviderClasses
{
    /// <summary>
    /// Singleton registry for all static (read-only after startup) game data:
    /// item definitions, quest definitions, future archetype tables, etc.
    /// Populated at startup via Templates.RegisterSeedItems/RegisterSeedQuests or YAML loaders.
    /// </summary>
    [Singleton]
    public class StaticData
    {
        private readonly Dictionary<string, ItemDef> _items = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, QuestDef> _quests = new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyDictionary<string, ItemDef> Items => _items;
        public IReadOnlyDictionary<string, QuestDef> Quests => _quests;

        public void RegisterItem(ItemDef item)
        {
            ArgumentNullException.ThrowIfNull(item);
            _items[item.Id] = item;
        }

        public ItemDef? GetItem(string id)
            => _items.TryGetValue(id, out var def) ? def : null;

        public void RegisterQuest(QuestDef quest)
        {
            ArgumentNullException.ThrowIfNull(quest);
            _quests[quest.Id] = quest;
        }

        public QuestDef? GetQuest(string id)
            => _quests.TryGetValue(id, out var def) ? def : null;

        // Legacy: may be populated by future YAML loaders.
        public Dictionary<string, byte> DispositionIds { get; set; } = [];
        public byte[] MoodIds { get; set; } = [];
        public byte[] GateIds  { get; set; } = [];
    }
}
