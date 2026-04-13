#nullable enable
namespace NPCChat.Core.DialogueClasses
{
    /// <summary>
    /// Per-speaker cooldown state for pool entries.
    /// Tracks the last time a given response (or cooldown group) was used.
    /// One instance lives on each NPC's ActorComponent.
    /// </summary>
    public sealed class CooldownTracker
    {
        private readonly Dictionary<string, double> _lastBySelfKey  = new(StringComparer.Ordinal);
        private readonly Dictionary<string, double> _lastByGroupKey = new(StringComparer.Ordinal);

        /// <summary>Returns true when the entry is not currently on cooldown.</summary>
        public bool IsOffCooldown(string speakerId, DialoguePoolEntry entry, double nowSeconds)
        {
            if (entry.Cooldown.SelfSeconds > 0f)
            {
                var key = SelfKey(speakerId, entry.Id);
                if (_lastBySelfKey.TryGetValue(key, out var last) &&
                    nowSeconds - last < entry.Cooldown.SelfSeconds)
                    return false;
            }

            if (!string.IsNullOrEmpty(entry.Cooldown.Group) && entry.Cooldown.GroupSeconds > 0f)
            {
                var gkey = GroupKey(speakerId, entry.Cooldown.Group!);
                if (_lastByGroupKey.TryGetValue(gkey, out var last) &&
                    nowSeconds - last < entry.Cooldown.GroupSeconds)
                    return false;
            }

            return true;
        }

        /// <summary>Records that this entry was used right now.</summary>
        public void MarkUsed(string speakerId, DialoguePoolEntry entry, double nowSeconds)
        {
            if (entry.Cooldown.SelfSeconds > 0f)
                _lastBySelfKey[SelfKey(speakerId, entry.Id)] = nowSeconds;

            if (!string.IsNullOrEmpty(entry.Cooldown.Group) && entry.Cooldown.GroupSeconds > 0f)
                _lastByGroupKey[GroupKey(speakerId, entry.Cooldown.Group!)] = nowSeconds;
        }

        // ── Serialization ─────────────────────────────────────────────────────

        /// <summary>
        /// Returns the raw key→timestamp dictionaries for serialization.
        /// Keys use the internal compound format and do not need further processing.
        /// </summary>
        public (IReadOnlyDictionary<string, double> Self, IReadOnlyDictionary<string, double> Group)
            SaveState() => (_lastBySelfKey, _lastByGroupKey);

        /// <summary>Restores cooldown state from a save record.</summary>
        public void RestoreState(
            IEnumerable<KeyValuePair<string, double>> self,
            IEnumerable<KeyValuePair<string, double>> group)
        {
            _lastBySelfKey.Clear();
            foreach (var kv in self)  _lastBySelfKey[kv.Key]  = kv.Value;

            _lastByGroupKey.Clear();
            foreach (var kv in group) _lastByGroupKey[kv.Key] = kv.Value;
        }

        private static string SelfKey(string speakerId, string entryId)  => speakerId + "||" + entryId;
        private static string GroupKey(string speakerId, string group)    => speakerId + "||G||" + group;
    }

    /// <summary>Cooldown configuration for a pool entry.</summary>
    public sealed class CooldownSpec
    {
        /// <summary>Seconds before this specific entry can be used again by the same speaker.</summary>
        public float SelfSeconds { get; init; }

        /// <summary>Shared cooldown group name. Null/empty means no group cooldown.</summary>
        public string? Group { get; init; }

        /// <summary>Seconds before any entry in the same group can be used by the same speaker.</summary>
        public float GroupSeconds { get; init; }
    }
}
