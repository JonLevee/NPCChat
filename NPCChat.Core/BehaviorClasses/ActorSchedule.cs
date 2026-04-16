using System.Collections.Generic;

namespace NPCChat.Core.BehaviorClasses
{
    /// <summary>
    /// Maps game-time hour ranges to actor mode strings.
    /// Entries are evaluated in order; the first matching range wins.
    /// </summary>
    public sealed class ActorSchedule
    {
        private readonly List<(GameTimeRange Range, string Mode)> _entries = new List<(GameTimeRange, string)>();

        public ActorSchedule Add(GameTimeRange range, string mode)
        {
            _entries.Add((range, mode));
            return this;
        }

        /// <summary>
        /// Returns the mode for the given game hour, or an empty string if no
        /// schedule entry matches.
        /// </summary>
        public string GetModeForTime(int gameHour)
        {
            foreach (var (range, mode) in _entries)
            {
                if (range.Contains(gameHour))
                    return mode;
            }
            return string.Empty;
        }
    }
}
