#nullable enable
using NPCChat.Core.Attributes;
using NPCChat.Core.ItemClasses;

namespace NPCChat.Core.LoadingProviderClasses
{
    /// <summary>
    /// Singleton registry for all static (read-only after startup) game data:
    /// item definitions, future archetype tables, etc.
    /// Populated at startup via Templates.RegisterSeedItems() or YAML loaders.
    /// </summary>
    [Singleton]
    public class StaticData
    {
        private readonly Dictionary<string, ItemDef> _items = new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyDictionary<string, ItemDef> Items => _items;

        public void RegisterItem(ItemDef item)
        {
            ArgumentNullException.ThrowIfNull(item);
            _items[item.Id] = item;
        }

        public ItemDef? GetItem(string id)
            => _items.TryGetValue(id, out var def) ? def : null;

        // Legacy: may be populated by future YAML loaders.
        public Dictionary<string, byte> DispositionIds { get; set; } = [];
        public byte[] MoodIds { get; set; } = [];
        public byte[] GateIds  { get; set; } = [];
    }
}
