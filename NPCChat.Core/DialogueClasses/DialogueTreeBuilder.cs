#nullable enable
namespace NPCChat.Core.DialogueClasses
{
    /// <summary>
    /// Fluent builder for DialogueTree. Used by Templates to author NPC dialogue
    /// in code until Phase 12 (YAML serialization) takes over.
    /// </summary>
    public sealed class DialogueTreeBuilder
    {
        private readonly string _id;
        private string _rootNodeId = string.Empty;
        private string[] _moodAxes = [];
        private readonly Dictionary<string, DialogueNode> _nodes = new();
        private int _poolEntryCounter;

        public DialogueTreeBuilder(string id) => _id = id;

        public DialogueTreeBuilder Root(string nodeId) { _rootNodeId = nodeId; return this; }

        public DialogueTreeBuilder MoodAxes(params string[] axes) { _moodAxes = axes; return this; }

        // ── Node adders ────────────────────────────────────────────────────────

        public DialogueTreeBuilder AddLine(
            string id,
            string speaker,
            string text,
            string? nextNodeId = null,
            Func<DialogueContext, bool>? condition = null,
            DialogueEffect[]? onEnter = null)
        {
            _nodes[id] = new DialogueLineNode
            {
                Id             = id,
                Speaker        = speaker,
                Text           = text,
                NextNodeId     = nextNodeId,
                Condition      = condition,
                OnEnterEffects = onEnter ?? []
            };
            return this;
        }

        public DialogueTreeBuilder AddNpcLine(
            string id,
            string text,
            string? nextNodeId = null,
            Func<DialogueContext, bool>? condition = null)
            => AddLine(id, "npc", text, nextNodeId, condition);

        public DialogueTreeBuilder AddChoice(
            string id,
            string npcText,
            Action<ChoiceBuilder> buildChoices,
            Func<DialogueContext, bool>? condition = null,
            DialogueEffect[]? onEnter = null)
        {
            var cb = new ChoiceBuilder();
            buildChoices(cb);
            _nodes[id] = new DialogueChoiceNode
            {
                Id             = id,
                NpcText        = npcText,
                Choices        = cb.Choices.AsReadOnly(),
                Condition      = condition,
                OnEnterEffects = onEnter ?? []
            };
            return this;
        }

        public DialogueTreeBuilder AddPool(
            string id,
            Action<PoolBuilder> buildPool,
            string? nextNodeId = null,
            Func<DialogueContext, bool>? condition = null,
            DialogueEffect[]? onEnter = null)
        {
            var pb = new PoolBuilder(_id, _moodAxes, ref _poolEntryCounter);
            buildPool(pb);
            _nodes[id] = new DialoguePoolNode
            {
                Id             = id,
                Entries        = pb.Entries.AsReadOnly(),
                NextNodeId     = nextNodeId,
                Condition      = condition,
                OnEnterEffects = onEnter ?? []
            };
            return this;
        }

        public DialogueTreeBuilder AddSequence(
            string id,
            string[] nodeIds,
            string? nextNodeId = null,
            Func<DialogueContext, bool>? condition = null,
            DialogueEffect[]? onEnter = null)
        {
            _nodes[id] = new DialogueSequenceNode
            {
                Id             = id,
                NodeIds        = nodeIds,
                NextNodeId     = nextNodeId,
                Condition      = condition,
                OnEnterEffects = onEnter ?? []
            };
            return this;
        }

        public DialogueTree Build()
        {
            if (string.IsNullOrWhiteSpace(_id))
                throw new InvalidOperationException("DialogueTree Id must be set.");
            if (string.IsNullOrWhiteSpace(_rootNodeId))
                throw new InvalidOperationException($"Tree '{_id}': RootNodeId must be set.");
            if (!_nodes.ContainsKey(_rootNodeId))
                throw new InvalidOperationException(
                    $"Tree '{_id}': root node '{_rootNodeId}' was not added.");

            return new DialogueTree
            {
                Id         = _id,
                RootNodeId = _rootNodeId,
                MoodAxes   = _moodAxes,
                Nodes      = _nodes
            };
        }
    }

    // ── Inner builders ─────────────────────────────────────────────────────────

    /// <summary>Accumulates choices for a DialogueChoiceNode. Max 9.</summary>
    public sealed class ChoiceBuilder
    {
        internal readonly List<DialogueChoice> Choices = [];

        public ChoiceBuilder Add(
            string label,
            string? nextNodeId = null,
            Func<DialogueContext, bool>? condition = null,
            params DialogueEffect[] onSelect)
        {
            if (Choices.Count >= 9)
                throw new InvalidOperationException(
                    "A DialogueChoiceNode supports at most 9 choices (keyboard shortcuts 1–9).");

            Choices.Add(new DialogueChoice
            {
                Label            = label,
                NextNodeId       = nextNodeId,
                Condition        = condition,
                OnSelectEffects  = onSelect
            });
            return this;
        }
    }

    /// <summary>Accumulates pool entries for a DialoguePoolNode.</summary>
    public sealed class PoolBuilder
    {
        private readonly string   _treeId;
        private readonly string[] _moodAxes;
        private int _counter; // ref to parent's counter so IDs are globally unique per tree

        internal readonly List<DialoguePoolEntry> Entries = [];

        internal PoolBuilder(string treeId, string[] moodAxes, ref int counter)
        {
            _treeId   = treeId;
            _moodAxes = moodAxes;
            _counter  = counter;
        }

        /// <summary>
        /// Adds a pool entry.
        /// moodHints: sparse axis→raw-value map; the builder normalizes it to a unit vector.
        /// requires/forbids: axis→(min?,max?) hard gates in 0..1 stat space.
        /// </summary>
        public PoolBuilder Add(
            string text,
            float  weight      = 1f,
            string? nextNodeId = null,
            string[]? intent   = null,
            Dictionary<string, float>? moodHints           = null,
            Dictionary<string, (float? min, float? max)>? requires = null,
            Dictionary<string, (float? min, float? max)>? forbids  = null,
            float minAffinity  = 0f,
            CooldownSpec? cooldown = null)
        {
            var entryId = $"{_treeId}__pool_{_counter++}";

            Entries.Add(new DialoguePoolEntry
            {
                Id          = entryId,
                Text        = text,
                Weight      = weight,
                NextNodeId  = nextNodeId,
                Intent      = intent ?? [],
                MoodVector  = BuildMoodVector(moodHints),
                Requires    = BuildGates(requires),
                Forbids     = BuildGates(forbids),
                MinAffinity = minAffinity,
                Cooldown    = cooldown ?? new CooldownSpec()
            });
            return this;
        }

        private float[] BuildMoodVector(Dictionary<string, float>? hints)
        {
            if (_moodAxes.Length == 0 || hints is null || hints.Count == 0)
                return [];

            var v = new float[_moodAxes.Length];
            for (int i = 0; i < _moodAxes.Length; i++)
                if (hints.TryGetValue(_moodAxes[i], out var val))
                    v[i] = val;

            double sumSq = 0;
            for (int i = 0; i < v.Length; i++) sumSq += v[i] * v[i];
            if (sumSq >= 1e-9)
            {
                var inv = (float)(1.0 / Math.Sqrt(sumSq));
                for (int i = 0; i < v.Length; i++) v[i] *= inv;
            }
            return v;
        }

        private static Dictionary<string, MinMax> BuildGates(
            Dictionary<string, (float? min, float? max)>? src)
        {
            var dst = new Dictionary<string, MinMax>(StringComparer.OrdinalIgnoreCase);
            if (src is null) return dst;
            foreach (var (key, (min, max)) in src)
            {
                bool hasMin = min.HasValue;
                bool hasMax = max.HasValue;
                if (!hasMin && !hasMax) continue;
                dst[key] = new MinMax(min ?? 0f, hasMin, max ?? 0f, hasMax);
            }
            return dst;
        }
    }
}
