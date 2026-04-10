namespace NPCChat.Core.BehaviorClasses
{
    /// <summary>
    /// An inclusive hour range within a 24-hour game day (0–23).
    /// Supports overnight ranges where StartHour > EndHour (e.g., 22–6).
    /// </summary>
    public readonly struct GameTimeRange
    {
        public int StartHour { get; }
        public int EndHour { get; }

        public GameTimeRange(int startHour, int endHour)
        {
            StartHour = startHour;
            EndHour = endHour;
        }

        public bool Contains(int hour)
        {
            if (StartHour <= EndHour)
                return hour >= StartHour && hour <= EndHour;

            // Overnight range (e.g., 22 to 6)
            return hour >= StartHour || hour <= EndHour;
        }
    }
}
