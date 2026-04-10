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
