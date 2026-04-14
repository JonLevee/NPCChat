#nullable enable
using System;
using System.Collections.Generic;

namespace NPCChat.Core.DialogueClasses
{
    /// <summary>
    /// Selects a DialoguePoolEntry from a pool using mood-vector scoring, hard stat gates,
    /// intent filtering, cooldown checks, and weighted-random selection.
    ///
    /// Stateless — safe to share or create per dialogue session.
    /// The mutable CooldownTracker lives on the actor (ActorComponent.DialogueCooldowns).
    /// </summary>
    public sealed class DialoguePicker
    {
        /// <summary>
        /// Picks one eligible entry from node.Entries, or null if nothing passes all filters.
        /// Falls back to weight-only scoring (ignoring mood) when no entry passes the affinity gate,
        /// ensuring a line is always returned when the pool has eligible entries.
        /// </summary>
        public DialoguePoolEntry? PickOne(
            DialoguePoolNode node,
            float[] npcMoodUnit,
            IReadOnlyDictionary<string, float> npcStats01,
            string? desiredIntent,
            string speakerId,
            CooldownTracker cooldowns,
            double nowSeconds,
            int topN = 8,
            float exponent = 2.0f,
            Random? rng = null)
        {
            rng ??= new Random();

            // First pass: full scoring with mood affinity.
            var scored = Score(node.Entries, npcMoodUnit, npcStats01, desiredIntent,
                               speakerId, cooldowns, nowSeconds, useMood: true);

            // Fallback: ignore mood, score by weight only.
            if (scored.Count == 0)
                scored = Score(node.Entries, npcMoodUnit, npcStats01, desiredIntent,
                               speakerId, cooldowns, nowSeconds, useMood: false);

            if (scored.Count == 0) return null;

            scored.Sort(static (a, b) => b.Score.CompareTo(a.Score));
            if (scored.Count > topN) scored.RemoveRange(topN, scored.Count - topN);

            return WeightedRandom(scored, exponent, rng);
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private static List<(DialoguePoolEntry Entry, float Score)> Score(
            IReadOnlyList<DialoguePoolEntry> entries,
            float[] npcMoodUnit,
            IReadOnlyDictionary<string, float> npcStats01,
            string? desiredIntent,
            string speakerId,
            CooldownTracker cooldowns,
            double nowSeconds,
            bool useMood)
        {
            var results = new List<(DialoguePoolEntry, float)>();

            foreach (var entry in entries)
            {
                if (!PassIntent(entry, desiredIntent))     continue;
                if (!PassHardGates(entry, npcStats01))     continue;
                if (!cooldowns.IsOffCooldown(speakerId, entry, nowSeconds)) continue;

                float score;
                if (useMood)
                {
                    var affinity = Affinity01(npcMoodUnit, entry.MoodVector);
                    if (affinity < entry.MinAffinity) continue;
                    score = affinity * entry.Weight;
                }
                else
                {
                    score = entry.Weight;
                }

                if (score > 0f)
                    results.Add((entry, score));
            }

            return results;
        }

        private static DialoguePoolEntry WeightedRandom(
            List<(DialoguePoolEntry Entry, float Score)> scored,
            float exponent,
            Random rng)
        {
            var weights = new double[scored.Count];
            double total = 0;
            for (int i = 0; i < scored.Count; i++)
            {
                var w = Math.Pow(scored[i].Score, exponent);
                weights[i] = w;
                total += w;
            }

            double roll = rng.NextDouble() * total;
            for (int i = 0; i < scored.Count; i++)
            {
                roll -= weights[i];
                if (roll <= 0) return scored[i].Entry;
            }
            return scored[^1].Entry;
        }

        private static float Affinity01(float[] npcMood, float[] entryMood)
        {
            if (npcMood.Length == 0 || entryMood.Length == 0) return 0.5f;
            float dot = 0f;
            int n = Math.Min(npcMood.Length, entryMood.Length);
            for (int i = 0; i < n; i++) dot += npcMood[i] * entryMood[i];
            return dot <= 0f ? 0f : dot;
        }

        private static bool PassIntent(DialoguePoolEntry entry, string? desired)
        {
            if (string.IsNullOrWhiteSpace(desired)) return true;
            if (entry.Intent.Length == 0)           return true;
            return entry.Intent.Any(i => i.Equals(desired, StringComparison.OrdinalIgnoreCase));
        }

        private static bool PassHardGates(
            DialoguePoolEntry entry,
            IReadOnlyDictionary<string, float> stats)
        {
            foreach (var req in entry.Requires)
            {
                if (!stats.TryGetValue(req.Key, out var v)) return false;
                if (!req.Value.Contains(v)) return false;
            }
            foreach (var fb in entry.Forbids)
            {
                if (!stats.TryGetValue(fb.Key, out var v)) continue;
                if (fb.Value.Contains(v)) return false; // forbidden range matched → reject
            }
            return true;
        }
    }
}
