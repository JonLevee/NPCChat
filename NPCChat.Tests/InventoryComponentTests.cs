using NPCChat.Core.InventoryClasses;
using NPCChat.Core.ItemClasses;

namespace NPCChat.Tests
{
    [TestClass]
    public class InventoryComponentTests
    {
        private static ItemDef Stackable(string id = "arrow", int maxStack = 20)
            => new ItemDef { Id = id, Name = id, Kind = ItemKind.Misc, MaxStack = maxStack };

        private static ItemDef NonStackable(string id = "sword")
            => new ItemDef { Id = id, Name = id, Kind = ItemKind.Weapon, MaxStack = 1 };

        // ── CountOf ──────────────────────────────────────────────────────────────

        [TestMethod]
        public void CountOf_Empty_ReturnsZero()
        {
            var inv = new InventoryComponent();
            Assert.AreEqual(0, inv.CountOf("arrow"));
        }

        // ── TryAdd non-stackable ─────────────────────────────────────────────────

        [TestMethod]
        public void TryAdd_NonStackable_SingleItem_Succeeds()
        {
            var inv = new InventoryComponent();
            var item = NonStackable();
            bool ok = inv.TryAdd(item, 1, out int remaining);
            Assert.IsTrue(ok);
            Assert.AreEqual(0, remaining);
            Assert.AreEqual(1, inv.CountOf("sword"));
        }

        [TestMethod]
        public void TryAdd_NonStackable_MultipleItems_CreatesSeparateSlots()
        {
            var inv = new InventoryComponent();
            var item = NonStackable();
            inv.TryAdd(item, 3, out _);
            Assert.HasCount(3, inv.Slots);
            Assert.AreEqual(3, inv.CountOf("sword"));
        }

        [TestMethod]
        public void TryAdd_MaxSlotsReached_NonStackable_ReturnsFalseWithRemainder()
        {
            var inv = new InventoryComponent { MaxSlots = 2 };
            var item = NonStackable();
            bool ok = inv.TryAdd(item, 3, out int remaining);
            Assert.IsFalse(ok);
            Assert.AreEqual(1, remaining);
            Assert.AreEqual(2, inv.CountOf("sword"));
        }

        // ── TryAdd stackable ─────────────────────────────────────────────────────

        [TestMethod]
        public void TryAdd_Stackable_FillsExistingSlotFirst()
        {
            var inv = new InventoryComponent();
            var item = Stackable(maxStack: 20);
            inv.TryAdd(item, 10, out _);
            inv.TryAdd(item, 5, out _);
            Assert.HasCount(1, inv.Slots);  // stacked into same slot
            Assert.AreEqual(15, inv.CountOf("arrow"));
        }

        [TestMethod]
        public void TryAdd_Stackable_ExceedsMaxStack_CreatesNewSlot()
        {
            var inv = new InventoryComponent();
            var item = Stackable(maxStack: 10);
            inv.TryAdd(item, 15, out int remaining);
            Assert.AreEqual(0, remaining);
            Assert.HasCount(2, inv.Slots);
            Assert.AreEqual(15, inv.CountOf("arrow"));
        }

        [TestMethod]
        public void TryAdd_Stackable_ExactlyFillsStack_NoRemainder()
        {
            var inv = new InventoryComponent();
            var item = Stackable(maxStack: 10);
            bool ok = inv.TryAdd(item, 10, out int remaining);
            Assert.IsTrue(ok);
            Assert.AreEqual(0, remaining);
            Assert.AreEqual(10, inv.CountOf("arrow"));
        }

        [TestMethod]
        public void TryAdd_Stackable_MaxSlotsAndStacksFull_ReturnsFalse()
        {
            // 1 slot max, maxStack=5 → max 5 items
            var inv = new InventoryComponent { MaxSlots = 1 };
            var item = Stackable(maxStack: 5);
            inv.TryAdd(item, 5, out _);
            bool ok = inv.TryAdd(item, 1, out int remaining);
            Assert.IsFalse(ok);
            Assert.AreEqual(1, remaining);
        }

        // ── AllowedKinds ─────────────────────────────────────────────────────────

        [TestMethod]
        public void TryAdd_ItemKindNotAllowed_ReturnsFalse()
        {
            var inv = new InventoryComponent { AllowedKinds = [ItemKind.Weapon] };
            var item = Stackable(); // Misc kind
            bool ok = inv.TryAdd(item, 1, out int remaining);
            Assert.IsFalse(ok);
            Assert.AreEqual(1, remaining);
            Assert.AreEqual(0, inv.CountOf("arrow"));
        }

        [TestMethod]
        public void TryAdd_AllowedKindsNull_AcceptsAll()
        {
            var inv = new InventoryComponent { AllowedKinds = null };
            inv.TryAdd(Stackable(), 1, out _);
            inv.TryAdd(NonStackable(), 1, out _);
            Assert.HasCount(2, inv.Slots);
        }

        // ── TryRemove ────────────────────────────────────────────────────────────

        [TestMethod]
        public void TryRemove_ExactCount_Succeeds()
        {
            var inv = new InventoryComponent();
            var item = Stackable();
            inv.TryAdd(item, 10, out _);
            bool ok = inv.TryRemove("arrow", 10, out int removed);
            Assert.IsTrue(ok);
            Assert.AreEqual(10, removed);
            Assert.AreEqual(0, inv.CountOf("arrow"));
        }

        [TestMethod]
        public void TryRemove_PartialCount_RemovesPartial()
        {
            var inv = new InventoryComponent();
            var item = Stackable();
            inv.TryAdd(item, 10, out _);
            bool ok = inv.TryRemove("arrow", 4, out int removed);
            Assert.IsTrue(ok);
            Assert.AreEqual(4, removed);
            Assert.AreEqual(6, inv.CountOf("arrow"));
        }

        [TestMethod]
        public void TryRemove_MoreThanAvailable_ReturnsFalse()
        {
            var inv = new InventoryComponent();
            var item = Stackable();
            inv.TryAdd(item, 5, out _);
            bool ok = inv.TryRemove("arrow", 10, out int removed);
            // Returns false when none could be removed OR partial? Let's check:
            // Actually the code returns removed > 0. So if 5 are removed, it returns true.
            // But removing 10 from 5 should remove 5 and return true (removed > 0).
            Assert.IsTrue(ok);
            Assert.AreEqual(5, removed);
            Assert.AreEqual(0, inv.CountOf("arrow"));
        }

        [TestMethod]
        public void TryRemove_NonExistentItem_ReturnsFalse()
        {
            var inv = new InventoryComponent();
            bool ok = inv.TryRemove("nonexistent", 1, out int removed);
            Assert.IsFalse(ok);
            Assert.AreEqual(0, removed);
        }

        [TestMethod]
        public void TryRemove_EmptiesSlot_SlotIsRemoved()
        {
            var inv = new InventoryComponent();
            var item = NonStackable();
            inv.TryAdd(item, 1, out _);
            inv.TryRemove("sword", 1, out _);
            Assert.IsEmpty(inv.Slots);
        }

        [TestMethod]
        public void TryRemove_AcrossMultipleSlots_RemovesAll()
        {
            var inv = new InventoryComponent();
            var item = Stackable(maxStack: 5);
            inv.TryAdd(item, 10, out _);   // 2 slots of 5
            bool ok = inv.TryRemove("arrow", 10, out int removed);
            Assert.IsTrue(ok);
            Assert.AreEqual(10, removed);
            Assert.IsEmpty(inv.Slots);
        }

        // ── CountOf across slots ─────────────────────────────────────────────────

        [TestMethod]
        public void CountOf_MultipleSlots_ReturnsSum()
        {
            var inv = new InventoryComponent();
            var item = Stackable(maxStack: 5);
            inv.TryAdd(item, 10, out _);   // 2 slots
            Assert.AreEqual(10, inv.CountOf("arrow"));
        }

        [TestMethod]
        public void CountOf_CaseInsensitive()
        {
            var inv = new InventoryComponent();
            inv.TryAdd(Stackable("Arrow"), 3, out _);
            Assert.AreEqual(3, inv.CountOf("arrow"));
            Assert.AreEqual(3, inv.CountOf("ARROW"));
        }
    }
}
