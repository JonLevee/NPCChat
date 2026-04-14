#nullable enable
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.DialogueClasses
{
    public enum DialogueSessionState
    {
        /// <summary>Displaying a text line — waiting for the player to continue (Space/Enter).</summary>
        NpcLine,
        /// <summary>Displaying choices — waiting for the player to press 1–9.</summary>
        PlayerChoice,
        /// <summary>The conversation has ended.</summary>
        Complete
    }

    /// <summary>
    /// Runtime execution engine for a single conversation with one NPC.
    /// Owns a pending-node queue and advances through the DialogueTree in response
    /// to player input. Constructed and driven by the UI thread.
    /// </summary>
    public sealed class DialogueSession
    {
        // Stack used as a LIFO queue: Push = "schedule next", Pop = "process next".
        // SequenceNodes push children in reverse; the top of the stack is always
        // the next thing to show.
        private readonly Stack<string> _pending = new();
        private readonly DialoguePicker _picker = new();

        public WorldObjectMoveable Actor { get; }
        public DialogueTree Tree { get; }

        /// <summary>Display name for the actor (NPC header in the dialogue panel).</summary>
        public string NpcName { get; }

        /// <summary>Current speech text (for NpcLine state: NPC's line or pool pick).</summary>
        public string DisplayText { get; private set; } = string.Empty;

        /// <summary>
        /// Visible player choices in the current ChoiceNode, ordered 1–9.
        /// Empty when State != PlayerChoice.
        /// </summary>
        public IReadOnlyList<(int Index, DialogueChoice Choice)> VisibleChoices { get; private set; } = new (int, DialogueChoice)[0];

        public DialogueSessionState State { get; private set; } = DialogueSessionState.NpcLine;

        /// <param name="actor">The NPC being talked to.</param>
        /// <param name="entryNodeId">Entry point in the actor's DialogueTree.</param>
        public DialogueSession(WorldObjectMoveable actor, string entryNodeId)
        {
            Actor    = actor;
            Tree     = actor.Actor!.DialogueTree!;
            NpcName  = actor.Character?.Name ?? "???";
            _pending.Push(entryNodeId);
        }

        /// <summary>
        /// Must be called once after construction to drive to the first displayable node.
        /// Also called after the player continues past a line node.
        /// No-op when State == PlayerChoice (use Select instead).
        /// </summary>
        public void Advance(in DialogueContext ctx)
        {
            if (State == DialogueSessionState.PlayerChoice) return;
            if (!TryAdvanceToNextDisplayable(ctx))
                State = DialogueSessionState.Complete;
        }

        /// <summary>
        /// Selects a choice by 1-based index. Returns false if the index is invalid
        /// or the current state is not PlayerChoice.
        /// </summary>
        public bool Select(int oneBasedIndex, in DialogueContext ctx)
        {
            if (State != DialogueSessionState.PlayerChoice) return false;

            var item = VisibleChoices.FirstOrDefault(c => c.Index == oneBasedIndex);
            if (item.Choice is null) return false;

            foreach (var effect in item.Choice.OnSelectEffects)
                effect.Apply(ctx);

            // Schedule the choice's continuation (if any) before advancing.
            if (item.Choice.NextNodeId is not null)
                _pending.Push(item.Choice.NextNodeId);

            if (!TryAdvanceToNextDisplayable(ctx))
                State = DialogueSessionState.Complete;

            return true;
        }

        // ── Private helpers ────────────────────────────────────────────────────

        /// <summary>
        /// Pops and processes nodes until one requires player input (line or choice).
        /// Returns true if a displayable state was reached, false if the queue drained.
        /// </summary>
        private bool TryAdvanceToNextDisplayable(in DialogueContext ctx)
        {
            while (_pending.Count > 0)
            {
                var nodeId = _pending.Pop();
                var node   = Tree.GetNode(nodeId);
                if (node is null) continue;

                if (node.Condition is not null && !node.Condition(ctx)) continue;

                foreach (var effect in node.OnEnterEffects)
                    effect.Apply(ctx);

                switch (node)
                {
                    case DialogueLineNode line:
                        DisplayText    = line.Text;
                        VisibleChoices = new (int, DialogueChoice)[0];
                        State          = DialogueSessionState.NpcLine;
                        if (line.NextNodeId is not null)
                            _pending.Push(line.NextNodeId);
                        return true;

                    case DialoguePoolNode pool:
                        var entry = PickPoolEntry(pool, ctx);
                        DisplayText    = entry?.Text ?? "(...)";
                        VisibleChoices = new (int, DialogueChoice)[0];
                        State          = DialogueSessionState.NpcLine;
                        var next = entry?.NextNodeId ?? pool.NextNodeId;
                        if (next is not null) _pending.Push(next);
                        return true;

                    case DialogueChoiceNode choice:
                        DisplayText = choice.NpcText;
                        var ctxCopy = ctx; // lambdas can't capture 'in' params directly
                        var visible = choice.Choices
                            .Where(c => c.Condition is null || c.Condition(ctxCopy))
                            .Take(9)
                            .Select((c, i) => (i + 1, c))
                            .ToList();
                        VisibleChoices = visible;
                        State = visible.Count > 0
                            ? DialogueSessionState.PlayerChoice
                            : DialogueSessionState.NpcLine;
                        return true;

                    case DialogueSequenceNode seq:
                        // Push children onto the stack in reverse so the first runs next.
                        if (seq.NextNodeId is not null) _pending.Push(seq.NextNodeId);
                        for (int i = seq.NodeIds.Count - 1; i >= 0; i--)
                            _pending.Push(seq.NodeIds[i]);
                        continue; // loop — sequence itself is not displayed
                }
            }
            return false;
        }

        private DialoguePoolEntry? PickPoolEntry(DialoguePoolNode pool, in DialogueContext ctx)
        {
            var character = ctx.Actor.Character;
            return _picker.PickOne(
                pool,
                character?.MoodVector ?? Array.Empty<float>(),
                (IReadOnlyDictionary<string, float>?)character?.Stats
                    ?? new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase),
                desiredIntent: null,
                speakerId:     ctx.Actor.Handle.ToString(),
                cooldowns:     ctx.Actor.Actor!.DialogueCooldowns,
                nowSeconds:    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0
            );
        }
    }
}
