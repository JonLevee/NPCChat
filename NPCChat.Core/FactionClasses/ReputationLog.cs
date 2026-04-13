using System;
using System.Collections.Generic;

namespace NPCChat.Core.FactionClasses
{
    /// <summary>
    /// Player-owned faction reputation tracker.
    /// Accessed exclusively on the UI thread (modified via dialogue effects,
    /// read by the reputation panel and dialogue conditions).
    /// </summary>
    public sealed class ReputationLog
    {
        private readonly Dictionary<string, int> _scores =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Returns the player's current reputation score with the given faction.
        /// Returns 0 (Neutral) for unknown factions.
        /// </summary>
        public int GetReputation(string factionId)
            => _scores.TryGetValue(factionId, out var score) ? score : 0;

        /// <summary>
        /// Adjusts the reputation score for a faction by <paramref name="delta"/> points.
        /// Scores are clamped to [-1000, 1000].
        /// </summary>
        public void AddReputation(string factionId, int delta)
        {
            _scores.TryGetValue(factionId, out var current);
            _scores[factionId] = Math.Clamp(current + delta, -1000, 1000);
        }

        /// <summary>
        /// Returns a snapshot of all tracked faction scores, keyed by faction ID.
        /// </summary>
        public IReadOnlyDictionary<string, int> AllScores => _scores;

        // ── Serialization ─────────────────────────────────────────────────────

        /// <summary>Restores faction scores from a save record, replacing any existing scores.</summary>
        public void RestoreState(IEnumerable<KeyValuePair<string, int>> scores)
        {
            _scores.Clear();
            foreach (var kv in scores) _scores[kv.Key] = kv.Value;
        }

        /// <summary>
        /// Returns the reputation tier for a given faction based on <paramref name="def"/>'s thresholds.
        /// </summary>
        public ReputationTier GetTier(FactionDef def)
        {
            int score = GetReputation(def.Id);
            return score switch
            {
                >= 300 => ReputationTier.Exalted,
                >= 200 => ReputationTier.Revered,
                >= 100 => ReputationTier.Honored,
                _      => score >= def.FriendlyThreshold ? ReputationTier.Friendly
                        : score >= def.HostileThreshold  ? (score >= 0 ? ReputationTier.Neutral
                                                                        : ReputationTier.Unfriendly)
                        : ReputationTier.Hostile
            };
        }
    }
}
