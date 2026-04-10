namespace NPCChat.Core.DialogueClasses
{
    /// <summary>
    /// An optional min/max range used in pool entry requires/forbids gates.
    /// A stat value passes (is within range) when it satisfies all present bounds.
    /// </summary>
    public readonly struct MinMax
    {
        public readonly float  Min;
        public readonly float  Max;
        public readonly bool   HasMin;
        public readonly bool   HasMax;

        public MinMax(float min, bool hasMin, float max, bool hasMax)
        {
            Min    = min;
            Max    = max;
            HasMin = hasMin;
            HasMax = hasMax;
        }

        /// <summary>Returns true when the value satisfies this range.</summary>
        public bool Contains(float v)
        {
            if (HasMin && v < Min) return false;
            if (HasMax && v > Max) return false;
            return true;
        }
    }
}
