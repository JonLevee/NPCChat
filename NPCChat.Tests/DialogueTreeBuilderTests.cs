using NPCChat.Core.DialogueClasses;

namespace NPCChat.Tests
{
    [TestClass]
    public class DialogueTreeBuilderTests
    {
        // ── Build validation ─────────────────────────────────────────────────────

        [TestMethod]
        public void Build_MissingRoot_Throws()
        {
            var b = new DialogueTreeBuilder("tree1");
            b.AddNpcLine("line1", "Hello");
            Assert.ThrowsExactly<InvalidOperationException>(() => b.Build());
        }

        [TestMethod]
        public void Build_RootNodeNotAdded_Throws()
        {
            var b = new DialogueTreeBuilder("tree1");
            b.Root("missing");
            Assert.ThrowsExactly<InvalidOperationException>(() => b.Build());
        }

        [TestMethod]
        public void Build_ValidTree_ReturnsDialogueTree()
        {
            var tree = new DialogueTreeBuilder("tree1")
                .Root("line1")
                .AddNpcLine("line1", "Hello!")
                .Build();

            Assert.IsNotNull(tree);
            Assert.AreEqual("tree1", tree.Id);
            Assert.AreEqual("line1", tree.RootNodeId);
        }

        // ── AddLine / AddNpcLine ─────────────────────────────────────────────────

        [TestMethod]
        public void AddLine_NodeAccessibleByKey()
        {
            var tree = new DialogueTreeBuilder("t")
                .Root("n1")
                .AddLine("n1", "player", "Sure thing.")
                .Build();

            var node = tree.GetNode("n1") as DialogueLineNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("player", node.Speaker);
            Assert.AreEqual("Sure thing.", node.Text);
        }

        [TestMethod]
        public void AddNpcLine_SetsSpeakerToNpc()
        {
            var tree = new DialogueTreeBuilder("t")
                .Root("n1")
                .AddNpcLine("n1", "Greetings!")
                .Build();

            var node = (DialogueLineNode)tree.GetNode("n1")!;
            Assert.AreEqual("npc", node.Speaker);
        }

        [TestMethod]
        public void AddNpcLine_NextNodeId_SetOnNode()
        {
            var tree = new DialogueTreeBuilder("t")
                .Root("n1")
                .AddNpcLine("n1", "Hello!", nextNodeId: "n2")
                .AddNpcLine("n2", "Bye!")
                .Build();

            var node = (DialogueLineNode)tree.GetNode("n1")!;
            Assert.AreEqual("n2", node.NextNodeId);
        }

        // ── AddChoice ────────────────────────────────────────────────────────────

        [TestMethod]
        public void AddChoice_CreatesChoiceNodeWithChoices()
        {
            var tree = new DialogueTreeBuilder("t")
                .Root("c1")
                .AddChoice("c1", "What do you want?", b =>
                {
                    b.Add("Buy something", nextNodeId: "shop");
                    b.Add("Nothing, thanks");
                })
                .AddNpcLine("shop", "Here's the shop.")
                .Build();

            var node = tree.GetNode("c1") as DialogueChoiceNode;
            Assert.IsNotNull(node);
            Assert.AreEqual("What do you want?", node.NpcText);
            Assert.HasCount(2, node.Choices);
            Assert.AreEqual("Buy something", node.Choices[0].Label);
        }

        [TestMethod]
        public void AddChoice_TenthChoice_Throws()
        {
            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                new DialogueTreeBuilder("t")
                    .Root("c1")
                    .AddChoice("c1", "Pick one", b =>
                    {
                        for (int i = 0; i < 10; i++)
                            b.Add($"Option {i}");
                    })
                    .Build();
            });
        }

        // ── AddPool ──────────────────────────────────────────────────────────────

        [TestMethod]
        public void AddPool_CreatesPoolNodeWithEntries()
        {
            var tree = new DialogueTreeBuilder("t")
                .Root("p1")
                .AddPool("p1", b =>
                {
                    b.Add("Hello there.");
                    b.Add("Nice weather.");
                })
                .Build();

            var node = tree.GetNode("p1") as DialoguePoolNode;
            Assert.IsNotNull(node);
            Assert.HasCount(2, node.Entries);
        }

        [TestMethod]
        public void AddPool_EntryIds_AreUniqueWithinPool()
        {
            var tree = new DialogueTreeBuilder("t")
                .Root("p1")
                .AddPool("p1", b =>
                {
                    b.Add("Line A");
                    b.Add("Line B");
                    b.Add("Line C");
                })
                .Build();

            var pool = (DialoguePoolNode)tree.GetNode("p1")!;
            var ids = pool.Entries.Select(e => e.Id).ToList();
            Assert.HasCount(ids.Distinct().Count(), ids);
        }

        [TestMethod]
        public void AddPool_EntryIds_ContainTreeId()
        {
            var tree = new DialogueTreeBuilder("myTree")
                .Root("p1")
                .AddPool("p1", b => b.Add("Hi"))
                .Build();

            var pool = (DialoguePoolNode)tree.GetNode("p1")!;
            Assert.StartsWith("myTree", pool.Entries[0].Id);
        }

        // ── AddSequence ──────────────────────────────────────────────────────────

        [TestMethod]
        public void AddSequence_CreatesSequenceNodeWithChildIds()
        {
            var tree = new DialogueTreeBuilder("t")
                .Root("seq1")
                .AddSequence("seq1", ["n1", "n2"])
                .AddNpcLine("n1", "First line.")
                .AddNpcLine("n2", "Second line.")
                .Build();

            var node = tree.GetNode("seq1") as DialogueSequenceNode;
            Assert.IsNotNull(node);
            Assert.HasCount(2, node.NodeIds);
            Assert.AreEqual("n1", node.NodeIds[0]);
            Assert.AreEqual("n2", node.NodeIds[1]);
        }

        // ── MoodAxes ─────────────────────────────────────────────────────────────

        [TestMethod]
        public void MoodAxes_SetOnBuilder_PresentInBuiltTree()
        {
            var tree = new DialogueTreeBuilder("t")
                .MoodAxes("friendliness", "greed")
                .Root("n1")
                .AddNpcLine("n1", "Hi")
                .Build();

            Assert.HasCount(2, tree.MoodAxes);
            Assert.AreEqual("friendliness", tree.MoodAxes[0]);
            Assert.AreEqual("greed", tree.MoodAxes[1]);
        }

        [TestMethod]
        public void AddPool_MoodHints_NormalizedToUnitVector()
        {
            var tree = new DialogueTreeBuilder("t")
                .MoodAxes("friendliness", "greed")
                .Root("p1")
                .AddPool("p1", b =>
                {
                    b.Add("Hi!", moodHints: new Dictionary<string, float>
                    {
                        { "friendliness", 3f },
                        { "greed", 4f }
                    });
                })
                .Build();

            var pool = (DialoguePoolNode)tree.GetNode("p1")!;
            var v = pool.Entries[0].MoodVector;
            // Vector (3,4) has magnitude 5; normalized = (0.6, 0.8)
            Assert.HasCount(2, v);
            Assert.AreEqual(0.6f, v[0], 1e-5f);
            Assert.AreEqual(0.8f, v[1], 1e-5f);
        }

        // ── Node conditions & effects ────────────────────────────────────────────

        [TestMethod]
        public void AddNpcLine_WithCondition_ConditionStoredOnNode()
        {
            bool triggered = false;
            var tree = new DialogueTreeBuilder("t")
                .Root("n1")
                .AddNpcLine("n1", "Hey", condition: _ => { triggered = true; return true; })
                .Build();

            var node = (DialogueLineNode)tree.GetNode("n1")!;
            Assert.IsNotNull(node.Condition);
            Assert.IsFalse(triggered, "Condition must not be invoked during tree construction.");
        }

        [TestMethod]
        public void AddNpcLine_WithOnEnterEffect_EffectStoredOnNode()
        {
            bool fired = false;
            var tree = new DialogueTreeBuilder("t")
                .Root("n1")
                .AddLine("n1", "npc", "Hello",
                    onEnter: [new DialogueEffect(_ => fired = true)])
                .Build();

            var node = (DialogueLineNode)tree.GetNode("n1")!;
            Assert.HasCount(1, node.OnEnterEffects);
            Assert.IsFalse(fired, "OnEnter effect must not fire during tree construction.");
        }
    }
}
