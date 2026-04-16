// Polyfill for System.Collections.Generic.PriorityQueue<TElement, TPriority>
// which was added in .NET 6 and is absent from netstandard2.1.
// Implements a binary min-heap. Only the members used by AStarPathfinder are provided.
#if NETSTANDARD2_1
namespace System.Collections.Generic
{
    internal class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
    {
        private readonly List<(TElement Element, TPriority Priority)> _heap =
            new List<(TElement, TPriority)>();

        public int Count => _heap.Count;

        public void Enqueue(TElement element, TPriority priority)
        {
            _heap.Add((element, priority));
            SiftUp(_heap.Count - 1);
        }

        public TElement Dequeue()
        {
            if (_heap.Count == 0) throw new InvalidOperationException("Queue is empty.");
            var result = _heap[0].Element;
            int last = _heap.Count - 1;
            _heap[0] = _heap[last];
            _heap.RemoveAt(last);
            if (_heap.Count > 0) SiftDown(0);
            return result;
        }

        private void SiftUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (_heap[parent].Priority.CompareTo(_heap[index].Priority) <= 0) break;
                var tmp = _heap[parent]; _heap[parent] = _heap[index]; _heap[index] = tmp;
                index = parent;
            }
        }

        private void SiftDown(int index)
        {
            int count = _heap.Count;
            while (true)
            {
                int smallest = index;
                int left  = 2 * index + 1;
                int right = 2 * index + 2;
                if (left  < count && _heap[left ].Priority.CompareTo(_heap[smallest].Priority) < 0) smallest = left;
                if (right < count && _heap[right].Priority.CompareTo(_heap[smallest].Priority) < 0) smallest = right;
                if (smallest == index) break;
                var tmp = _heap[smallest]; _heap[smallest] = _heap[index]; _heap[index] = tmp;
                index = smallest;
            }
        }
    }
}
#endif
