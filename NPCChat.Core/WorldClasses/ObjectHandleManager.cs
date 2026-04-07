using System;
using System.Collections.Generic;
using NPCChat.Core.Attributes;


namespace NPCChat.Core.WorldClasses
{
    public interface IObjectHandleManager
    {
        ObjectHandle GetNewHandle(WorldObject obj);
    }

    [Scoped(serviceType: typeof(IObjectHandleManager))]
    public sealed class ObjectHandleManager
    {
        // slot-id indexed storage
        public readonly List<ObjectSlot> Slots = [];

        // free slot ids available for reuse
        public readonly Stack<int> FreeSlotIds = new();

        // packed list of currently active handles, if you later want iteration
        public readonly HashSet<ObjectHandle> ActiveHandles = [];

        public ObjectHandle GetNewHandle(WorldObject obj)
        {
            int slotId;
            byte generation;

            if (FreeSlotIds.Count > 0)
            {
                slotId = FreeSlotIds.Pop();
                var slot = Slots[slotId];

                if (slot.IsOccupied)
                    throw new InvalidOperationException($"Internal error: free slot {slotId} is still occupied.");

                generation = slot.Generation;
            }
            else
            {
                slotId = Slots.Count;
                if ((uint)slotId > 0x00FFFFFF)
                    throw new InvalidOperationException("Maximum object count exceeded for 24-bit handle ids.");

                generation = 1; // start at 1 so default(0) is never a valid active handle
                Slots.Add(default);
            }

            var handle = new ObjectHandle(slotId, generation);

            if (ActiveHandles.Contains(handle))
                throw new InvalidOperationException($"Duplicate active handle detected: {handle}.");


            Slots[slotId] = new ObjectSlot
            {
                Object = obj,
                Generation = generation,
                IsOccupied = true
            };

            ActiveHandles.Add(handle);
            return handle;
        }

        public void RemoveSlot(ObjectHandle handle)
        {
            int slotId = handle.Id;
            var slot = Slots[slotId];

            if (!slot.IsOccupied)
                throw new InvalidOperationException($"Handle {handle} refers to an unoccupied slot.");

            if (slot.Generation != handle.Generation)
                throw new InvalidOperationException($"Handle {handle} is stale.");

            slot.Object = null;
            slot.IsOccupied = false;
            slot.Generation = unchecked((byte)(slot.Generation + 1));

            Slots[slotId] = slot;

            if (!ActiveHandles.Remove(handle))
                throw new InvalidOperationException($"Internal error: active handle set did not contain {handle}.");
        }

        public bool TryGetSlot(ObjectHandle handle, out ObjectSlot slot)
        {
            slot = ObjectSlot.None;
            int slotId = handle.Id;
            if (slotId < 0 || slotId >= Slots.Count)
                return false;

            slot = Slots[slotId];
            return slot.IsOccupied && slot.Generation == handle.Generation;
        }
    }
}