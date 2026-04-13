using System;
using System.Collections.Generic;
using System.Linq;

namespace NPCChat.Core.QuestClasses
{
    /// <summary>
    /// Player-owned quest journal. Tracks active and completed quests.
    /// Accessed exclusively on the UI thread (modified via dialogue effects,
    /// read by the quest panel).
    /// </summary>
    public sealed class QuestLog
    {
        private readonly List<QuestRecord> _active = [];
        private readonly HashSet<string> _completedIds =
            new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyList<QuestRecord> ActiveQuests => _active;

        public bool HasActiveQuest(string questId)
            => _active.Any(r => r.Def.Id.Equals(questId, StringComparison.OrdinalIgnoreCase));

        public bool HasCompletedQuest(string questId)
            => _completedIds.Contains(questId);

        public bool CanStartQuest(string questId)
            => !HasActiveQuest(questId) && !HasCompletedQuest(questId);

        /// <summary>
        /// Adds the quest to the active list. No-op if already active or completed.
        /// </summary>
        public void StartQuest(QuestDef def)
        {
            if (!CanStartQuest(def.Id)) return;
            _active.Add(new QuestRecord(def));
        }

        // ── Serialization ─────────────────────────────────────────────────────

        /// <summary>Returns active quest IDs and completed quest IDs for serialization.</summary>
        public (IEnumerable<string> ActiveIds, IEnumerable<string> CompletedIds) SaveState()
            => (_active.Select(r => r.Def.Id), _completedIds);

        /// <summary>
        /// Restores quest state from a save record.
        /// Unknown quest IDs (no longer in StaticData) are silently skipped.
        /// </summary>
        public void RestoreState(
            IEnumerable<string> activeIds,
            IEnumerable<string> completedIds,
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
            Func<string, QuestDef?> getQuest)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            _active.Clear();
            _completedIds.Clear();
            foreach (var id in completedIds)
                _completedIds.Add(id);
            foreach (var id in activeIds)
            {
                var def = getQuest(id);
                if (def is not null) _active.Add(new QuestRecord(def));
            }
        }

        /// <summary>
        /// Marks the quest complete and moves it to the completed set.
        /// Returns true on success, false if the quest was not active.
        /// </summary>
        public bool TryComplete(string questId)
        {
            var record = _active.FirstOrDefault(r =>
                r.Def.Id.Equals(questId, StringComparison.OrdinalIgnoreCase));
            if (record is null) return false;

            record.Status = QuestStatus.Complete;
            _active.Remove(record);
            _completedIds.Add(questId);
            return true;
        }
    }
}
