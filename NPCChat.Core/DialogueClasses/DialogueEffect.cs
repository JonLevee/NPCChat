namespace NPCChat.Core.DialogueClasses
{
    /// <summary>
    /// A side effect applied when entering a node or selecting a choice.
    /// Examples: set a quest flag, update NPC mood, give the player an item.
    /// Phase 5 uses simple delegates; Phase 7+ will add typed effect subtypes.
    /// </summary>
    public sealed class DialogueEffect
    {
        private readonly Action<DialogueContext> _apply;

        public DialogueEffect(Action<DialogueContext> apply)
        {
            _apply = apply;
        }

        public void Apply(in DialogueContext ctx) => _apply(ctx);
    }
}
