namespace NPCChat.Core.CharacterClasses
{
    public class Character
    {
        public string Name      { get; set; } = string.Empty;
        public string Archetype { get; set; } = string.Empty;

        /// <summary>
        /// Normalized unit vector in the owning DialogueTree's MoodAxes space.
        /// Used by DialoguePicker for dot-product affinity scoring.
        /// Index i maps to DialogueTree.MoodAxes[i].
        /// </summary>
        public float[] MoodVector { get; set; } = [];

        /// <summary>
        /// Current stat values in 0..1 space.
        /// Keys are stat/axis names (case-insensitive).
        /// Used by DialoguePicker for requires/forbids gate evaluation.
        /// </summary>
        public Dictionary<string, float> Stats { get; set; } =
            new(StringComparer.OrdinalIgnoreCase);
    }
}
