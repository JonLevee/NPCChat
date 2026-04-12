using NPCChat.Core.WorldClasses;

namespace NPCChat.Tests
{
    [TestClass]
    public class ObjectHandleManagerTests
    {
        private static WorldObject MakeObj() => new WorldObjectStatic
        {
            Kind   = WorldObjectKind.Building,
            Bounds = new Bounds(0, 0, 1, 1)
        };

        // ── Allocation ───────────────────────────────────────────────────────────

        [TestMethod]
        public void GetNewHandle_ReturnsNonDefaultHandle()
        {
            var mgr = new ObjectHandleManager();
            var h   = mgr.GetNewHandle(MakeObj());
            Assert.IsFalse(h.IsDefault);
        }

        [TestMethod]
        public void GetNewHandle_TwoAllocations_DifferentHandles()
        {
            var mgr = new ObjectHandleManager();
            var h1  = mgr.GetNewHandle(MakeObj());
            var h2  = mgr.GetNewHandle(MakeObj());
            Assert.AreNotEqual(h1, h2);
        }

        [TestMethod]
        public void GetNewHandle_AddsToActiveHandles()
        {
            var mgr = new ObjectHandleManager();
            var h   = mgr.GetNewHandle(MakeObj());
            Assert.IsTrue(mgr.ActiveHandles.Contains(h));
        }

        [TestMethod]
        public void GetNewHandle_AddsToSlots()
        {
            var mgr = new ObjectHandleManager();
            mgr.GetNewHandle(MakeObj());
            Assert.AreEqual(1, mgr.Slots.Count);
        }

        // ── TryGetSlot ───────────────────────────────────────────────────────────

        [TestMethod]
        public void TryGetSlot_ValidHandle_ReturnsOccupiedSlot()
        {
            var mgr = new ObjectHandleManager();
            var h   = mgr.GetNewHandle(MakeObj());
            Assert.IsTrue(mgr.TryGetSlot(h, out var slot));
            Assert.IsTrue(slot.IsOccupied);
        }

        [TestMethod]
        public void TryGetSlot_DefaultHandle_ReturnsFalse()
        {
            var mgr = new ObjectHandleManager();
            Assert.IsFalse(mgr.TryGetSlot(ObjectHandle.None, out _));
        }

        [TestMethod]
        public void TryGetSlot_SlotStoresCorrectObject()
        {
            var mgr = new ObjectHandleManager();
            var obj = MakeObj();
            var h   = mgr.GetNewHandle(obj);
            mgr.TryGetSlot(h, out var slot);
            Assert.AreSame(obj, slot.Object);
        }

        // ── Deallocation ─────────────────────────────────────────────────────────

        [TestMethod]
        public void RemoveSlot_ValidHandle_RemovesFromActive()
        {
            var mgr = new ObjectHandleManager();
            var h   = mgr.GetNewHandle(MakeObj());
            mgr.RemoveSlot(h);
            Assert.IsFalse(mgr.ActiveHandles.Contains(h));
        }

        [TestMethod]
        public void RemoveSlot_ValidHandle_SlotBecomesUnoccupied()
        {
            // RemoveSlot marks the slot as unoccupied (TryGetSlot returns false).
            // It does NOT push to FreeSlotIds — that's managed externally if slot reuse is needed.
            var mgr = new ObjectHandleManager();
            var h   = mgr.GetNewHandle(MakeObj());
            mgr.RemoveSlot(h);
            Assert.IsFalse(mgr.TryGetSlot(h, out _));
            Assert.AreEqual(1, mgr.Slots.Count);  // slot still exists, just unoccupied
        }

        [TestMethod]
        public void RemoveSlot_ThenGetNew_AllocatesNewSlot()
        {
            // Without manually pushing to FreeSlotIds, GetNewHandle appends a new slot.
            var mgr  = new ObjectHandleManager();
            var h1   = mgr.GetNewHandle(MakeObj());
            mgr.RemoveSlot(h1);
            var h2   = mgr.GetNewHandle(MakeObj());
            // A new slot is allocated (index 1), not the freed one (index 0)
            Assert.AreNotEqual(h1.Id, h2.Id);
            Assert.AreEqual(2, mgr.Slots.Count);
        }

        [TestMethod]
        public void RemoveSlot_ThenTryGetSlot_ReturnsFalse()
        {
            var mgr = new ObjectHandleManager();
            var h   = mgr.GetNewHandle(MakeObj());
            mgr.RemoveSlot(h);
            Assert.IsFalse(mgr.TryGetSlot(h, out _));
        }

        [TestMethod]
        public void RemoveSlot_StaleHandle_Throws()
        {
            var mgr  = new ObjectHandleManager();
            var h1   = mgr.GetNewHandle(MakeObj());
            mgr.RemoveSlot(h1);
            mgr.GetNewHandle(MakeObj()); // reuses slot, increments generation

            // h1 now points to the old generation — should throw
            Assert.ThrowsExactly<InvalidOperationException>(() => mgr.RemoveSlot(h1));
        }

        // ── Active handles count ─────────────────────────────────────────────────

        [TestMethod]
        public void ActiveHandles_ReflectsAllocations()
        {
            var mgr = new ObjectHandleManager();
            Assert.AreEqual(0, mgr.ActiveHandles.Count);
            mgr.GetNewHandle(MakeObj());
            mgr.GetNewHandle(MakeObj());
            Assert.AreEqual(2, mgr.ActiveHandles.Count);
        }

        [TestMethod]
        public void ActiveHandles_AfterRemove_CountDecreases()
        {
            var mgr = new ObjectHandleManager();
            var h   = mgr.GetNewHandle(MakeObj());
            mgr.GetNewHandle(MakeObj());
            mgr.RemoveSlot(h);
            Assert.AreEqual(1, mgr.ActiveHandles.Count);
        }
    }
}
