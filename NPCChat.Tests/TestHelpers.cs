using NPCChat.Core.BehaviorClasses;
using NPCChat.Core.CharacterClasses;
using NPCChat.Core.DialogueClasses;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Tests
{
    // ── Stub task ────────────────────────────────────────────────────────────────

    /// <summary>Minimal IActorTask for ActionQueue tests.</summary>
    internal sealed class StubTask : IActorTask
    {
        public int Priority { get; }
        public int MaxTurns => int.MaxValue;
        public int TurnsElapsed { get; private set; }
        public LinkedList<SubTask> SubTasks { get; } = new();
        public LinkedListNode<SubTask>? CurrentSubTaskNode => null;
        public int TurnsRemainingInCurrentSubTask { get; set; }

        public StubTask(int priority) => Priority = priority;

        public void Begin(in SimContext ctx) { }
        public bool Tick(in SimContext ctx) { TurnsElapsed++; return false; }
        public void Interrupt(in SimContext ctx) { }
        public ITaskToken ToToken() => throw new NotSupportedException("StubTask is not serializable.");
    }

    // ── NPC actor factory ────────────────────────────────────────────────────────

    internal static class TestActorFactory
    {
        /// <summary>
        /// Creates a WorldObjectMoveable suitable for passing to DialogueSession.
        /// Handle is pre-set to (id=1, gen=1) so speakerId is non-empty.
        /// </summary>
        public static WorldObjectMoveable MakeNpc(
            DialogueTree tree,
            string name = "TestNpc",
            float[]? moodVector = null)
        {
            var actor = new ActorComponent { DialogueTree = tree };
            return new WorldObjectMoveable
            {
                Handle = new ObjectHandle(1, 1),
                Actor = actor,
                Character = new Character
                {
                    Name = name,
                    MoodVector = moodVector ?? []
                }
            };
        }

        /// <summary>Minimal DialogueContext for session-driven tests (no pool picking).</summary>
        public static DialogueContext MakeCtx(WorldObjectMoveable npc,
            int gameHour = 12, int gameTick = 0)
            => new DialogueContext { Actor = npc, GameHour = gameHour, GameTick = gameTick };
    }

    // ── Pool entry factory ───────────────────────────────────────────────────────

    internal static class TestEntryFactory
    {
        public static DialoguePoolEntry MakeEntry(
            string id,
            string text = "hello",
            float weight = 1f,
            float[]? moodVector = null,
            float minAffinity = 0f,
            string[]? intent = null,
            CooldownSpec? cooldown = null,
            Dictionary<string, MinMax>? requires = null,
            Dictionary<string, MinMax>? forbids = null)
            => new DialoguePoolEntry
            {
                Id          = id,
                Text        = text,
                Weight      = weight,
                MoodVector  = moodVector ?? [],
                MinAffinity = minAffinity,
                Intent      = intent ?? [],
                Cooldown    = cooldown ?? new CooldownSpec(),
                Requires    = requires ?? new Dictionary<string, MinMax>(StringComparer.OrdinalIgnoreCase),
                Forbids     = forbids  ?? new Dictionary<string, MinMax>(StringComparer.OrdinalIgnoreCase)
            };

        public static DialoguePoolNode MakePool(params DialoguePoolEntry[] entries)
            => new DialoguePoolNode
            {
                Id      = "pool",
                Entries = entries.ToList().AsReadOnly()
            };
    }
}
