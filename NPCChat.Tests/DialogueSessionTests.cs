using NPCChat.Core.BehaviorClasses;
using NPCChat.Core.DialogueClasses;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Tests
{
    [TestClass]
    public class DialogueSessionTests
    {
        // ── Helpers ──────────────────────────────────────────────────────────────

        private static DialogueTree LineTree(string text, string? nextId = null)
            => new DialogueTreeBuilder("t")
                .Root("start")
                .AddNpcLine("start", text, nextNodeId: nextId)
                .Build();

        private static (DialogueSession session, DialogueContext ctx) MakeSession(
            DialogueTree tree,
            string? entryNode = null,
            float[]? npcMood = null)
        {
            var npc = TestActorFactory.MakeNpc(tree, moodVector: npcMood);
            var ctx = TestActorFactory.MakeCtx(npc);
            var session = new DialogueSession(npc, entryNode ?? tree.RootNodeId);
            return (session, ctx);
        }

        // ── NpcLine node ─────────────────────────────────────────────────────────

        [TestMethod]
        public void Advance_LineNode_SetsDisplayText()
        {
            var (session, ctx) = MakeSession(LineTree("Hello there."));
            session.Advance(ctx);
            Assert.AreEqual("Hello there.", session.DisplayText);
        }

        [TestMethod]
        public void Advance_LineNode_StateIsNpcLine()
        {
            var (session, ctx) = MakeSession(LineTree("Hello there."));
            session.Advance(ctx);
            Assert.AreEqual(DialogueSessionState.NpcLine, session.State);
        }

        [TestMethod]
        public void Advance_LineNode_VisibleChoicesEmpty()
        {
            var (session, ctx) = MakeSession(LineTree("Hello there."));
            session.Advance(ctx);
            Assert.IsEmpty(session.VisibleChoices);
        }

        [TestMethod]
        public void Advance_AfterLastLine_StateIsComplete()
        {
            var (session, ctx) = MakeSession(LineTree("Goodbye."));
            session.Advance(ctx);  // shows line, pushes nothing (no nextNodeId)
            session.Advance(ctx);  // nothing left → Complete
            Assert.AreEqual(DialogueSessionState.Complete, session.State);
        }

        [TestMethod]
        public void Advance_ChainedLines_FollowsNextNodeId()
        {
            var tree = new DialogueTreeBuilder("t")
                .Root("n1")
                .AddNpcLine("n1", "Line one.", nextNodeId: "n2")
                .AddNpcLine("n2", "Line two.")
                .Build();

            var (session, ctx) = MakeSession(tree);
            session.Advance(ctx);
            Assert.AreEqual("Line one.", session.DisplayText);

            session.Advance(ctx);
            Assert.AreEqual("Line two.", session.DisplayText);
        }

        // ── PlayerChoice node ────────────────────────────────────────────────────

        [TestMethod]
        public void Advance_ChoiceNode_StateIsPlayerChoice()
        {
            var tree = new DialogueTreeBuilder("t")
                .Root("c1")
                .AddChoice("c1", "What now?", b =>
                {
                    b.Add("Option A");
                    b.Add("Option B");
                })
                .Build();

            var (session, ctx) = MakeSession(tree);
            session.Advance(ctx);
            Assert.AreEqual(DialogueSessionState.PlayerChoice, session.State);
        }

        [TestMethod]
        public void Advance_ChoiceNode_ExposesChoices()
        {
            var tree = new DialogueTreeBuilder("t")
                .Root("c1")
                .AddChoice("c1", "What now?", b =>
                {
                    b.Add("Option A");
                    b.Add("Option B");
                })
                .Build();

            var (session, ctx) = MakeSession(tree);
            session.Advance(ctx);
            Assert.HasCount(2, session.VisibleChoices);
        }

        [TestMethod]
        public void Advance_ChoiceNode_HidesChoicesWithFalseCondition()
        {
            var tree = new DialogueTreeBuilder("t")
                .Root("c1")
                .AddChoice("c1", "What now?", b =>
                {
                    b.Add("Always visible");
                    b.Add("Never visible", condition: _ => false);
                })
                .Build();

            var (session, ctx) = MakeSession(tree);
            session.Advance(ctx);
            Assert.HasCount(1, session.VisibleChoices);
            Assert.AreEqual("Always visible", session.VisibleChoices[0].Choice.Label);
        }

        [TestMethod]
        public void Advance_WhileInPlayerChoiceState_IsNoOp()
        {
            var tree = new DialogueTreeBuilder("t")
                .Root("c1")
                .AddChoice("c1", "Pick:", b => b.Add("A"))
                .Build();

            var (session, ctx) = MakeSession(tree);
            session.Advance(ctx);
            Assert.AreEqual(DialogueSessionState.PlayerChoice, session.State);

            session.Advance(ctx);  // no-op — still waiting for player input
            Assert.AreEqual(DialogueSessionState.PlayerChoice, session.State);
        }

        // ── Select ───────────────────────────────────────────────────────────────

        [TestMethod]
        public void Select_ValidIndex_ReturnsTrue()
        {
            var tree = new DialogueTreeBuilder("t")
                .Root("c1")
                .AddChoice("c1", "?", b => b.Add("Go"))
                .Build();

            var (session, ctx) = MakeSession(tree);
            session.Advance(ctx);
            Assert.IsTrue(session.Select(1, ctx));
        }

        [TestMethod]
        public void Select_InvalidIndex_ReturnsFalse()
        {
            var tree = new DialogueTreeBuilder("t")
                .Root("c1")
                .AddChoice("c1", "?", b => b.Add("Go"))
                .Build();

            var (session, ctx) = MakeSession(tree);
            session.Advance(ctx);
            Assert.IsFalse(session.Select(9, ctx));
        }

        [TestMethod]
        public void Select_NotInPlayerChoiceState_ReturnsFalse()
        {
            var (session, ctx) = MakeSession(LineTree("Hello"));
            session.Advance(ctx);
            Assert.AreEqual(DialogueSessionState.NpcLine, session.State);
            Assert.IsFalse(session.Select(1, ctx));
        }

        [TestMethod]
        public void Select_ChoiceWithNextNode_AdvancesToNode()
        {
            var tree = new DialogueTreeBuilder("t")
                .Root("c1")
                .AddChoice("c1", "?", b => b.Add("Continue", nextNodeId: "result"))
                .AddNpcLine("result", "You chose well.")
                .Build();

            var (session, ctx) = MakeSession(tree);
            session.Advance(ctx);
            session.Select(1, ctx);
            Assert.AreEqual("You chose well.", session.DisplayText);
        }

        [TestMethod]
        public void Select_ChoiceWithNoNextNode_CompletesSession()
        {
            var tree = new DialogueTreeBuilder("t")
                .Root("c1")
                .AddChoice("c1", "?", b => b.Add("Farewell"))
                .Build();

            var (session, ctx) = MakeSession(tree);
            session.Advance(ctx);
            session.Select(1, ctx);
            Assert.AreEqual(DialogueSessionState.Complete, session.State);
        }

        [TestMethod]
        public void Select_OnSelectEffect_IsApplied()
        {
            bool fired = false;
            var tree = new DialogueTreeBuilder("t")
                .Root("c1")
                .AddChoice("c1", "?", b =>
                    b.Add("Fire effect",
                        onSelect: new DialogueEffect(_ => fired = true)))
                .Build();

            var (session, ctx) = MakeSession(tree);
            session.Advance(ctx);
            session.Select(1, ctx);
            Assert.IsTrue(fired);
        }

        // ── Sequence node ────────────────────────────────────────────────────────

        [TestMethod]
        public void Advance_SequenceNode_ProcessesChildrenInOrder()
        {
            var tree = new DialogueTreeBuilder("t")
                .Root("seq")
                .AddSequence("seq", ["n1", "n2", "n3"])
                .AddNpcLine("n1", "First.")
                .AddNpcLine("n2", "Second.")
                .AddNpcLine("n3", "Third.")
                .Build();

            var (session, ctx) = MakeSession(tree);

            session.Advance(ctx);
            Assert.AreEqual("First.", session.DisplayText);

            session.Advance(ctx);
            Assert.AreEqual("Second.", session.DisplayText);

            session.Advance(ctx);
            Assert.AreEqual("Third.", session.DisplayText);

            session.Advance(ctx);
            Assert.AreEqual(DialogueSessionState.Complete, session.State);
        }

        // ── Condition gate ───────────────────────────────────────────────────────

        [TestMethod]
        public void Advance_NodeConditionFalse_NodeIsSkipped()
        {
            var tree = new DialogueTreeBuilder("t")
                .Root("n1")
                .AddNpcLine("n1", "Skipped.", nextNodeId: "n2",
                    condition: _ => false)
                .AddNpcLine("n2", "Reached.")
                .Build();

            // n1 has condition=false → skipped; n2 should be shown
            // But n1 is the root pushed onto the stack. When condition fails, the node
            // is skipped but its NextNodeId is not pushed (session just continues popping).
            // That means n2 won't be reached automatically from n1's condition fail.
            // Instead, the session will reach Complete.
            var (session, ctx) = MakeSession(tree);
            session.Advance(ctx);
            Assert.AreEqual(DialogueSessionState.Complete, session.State);
        }

        [TestMethod]
        public void Advance_NodeConditionTrue_NodeIsEntered()
        {
            var tree = new DialogueTreeBuilder("t")
                .Root("n1")
                .AddNpcLine("n1", "Entered.", condition: _ => true)
                .Build();

            var (session, ctx) = MakeSession(tree);
            session.Advance(ctx);
            Assert.AreEqual("Entered.", session.DisplayText);
            Assert.AreEqual(DialogueSessionState.NpcLine, session.State);
        }

        // ── OnEnter effects ──────────────────────────────────────────────────────

        [TestMethod]
        public void Advance_OnEnterEffect_AppliedWhenNodeEntered()
        {
            bool fired = false;
            var tree = new DialogueTreeBuilder("t")
                .Root("n1")
                .AddLine("n1", "npc", "Hello",
                    onEnter: [new DialogueEffect(_ => fired = true)])
                .Build();

            var (session, ctx) = MakeSession(tree);
            session.Advance(ctx);
            Assert.IsTrue(fired);
        }

        // ── NpcName ──────────────────────────────────────────────────────────────

        [TestMethod]
        public void NpcName_ReflectsCharacterName()
        {
            var tree = LineTree("Hi");
            var npc = TestActorFactory.MakeNpc(tree, name: "Brutus");
            var session = new DialogueSession(npc, "start");
            Assert.AreEqual("Brutus", session.NpcName);
        }

        [TestMethod]
        public void NpcName_NoCharacter_DefaultsToQuestionMarks()
        {
            var tree = LineTree("Hi");
            var actor = new ActorComponent { DialogueTree = tree };
            var npc = new WorldObjectMoveable
            {
                Handle = new ObjectHandle(2, 1),
                Actor = actor
                // Character = null intentionally
            };
            var session = new DialogueSession(npc, "start");
            Assert.AreEqual("???", session.NpcName);
        }
    }
}
