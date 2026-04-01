using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPCChatLib.Attributes;

namespace NPCChat.Core.SupportClasses
{
    [Singleton]
    public class ChunkInfo
    {
        private List<int> _chunkSizes = [4, 8, 16, 32, 64, 128];
        public IReadOnlyList<int> ChunkSizes => _chunkSizes.AsReadOnly();
        private int _chunkSizeIndex = 0;
        public int MaxChunkSize => ChunkSizes[_chunkSizeIndex];
        public int ChunkSize
        {
            get => ChunkSizes[_chunkSizeIndex];
            set
            {
                var index = _chunkSizes.IndexOf(value);
                if (index == -1)
                {
                    throw new ArgumentException($"Invalid chunk size {value}. Valid sizes are: {string.Join(", ", ChunkSizes)}");
                }
                _chunkSizeIndex = index;
            }
        }
        public ChunkInfo()
        {
            ChunkSize = 8;
        }
    }
}
