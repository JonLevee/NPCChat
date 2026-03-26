using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPCChatLib.Attributes;

namespace NPCChatLib.WorldClasses
{
    public interface IWorldOptions
    {
        int ChunkSize { get; }
    }

    [Scoped(serviceType: typeof(IWorldOptions))]
    public sealed class WorldOptions : IWorldOptions
    {
        private int chunkSize = 16;
        public int ChunkSize
        {
            get => chunkSize;
            set
            {
                Assert.IsGreaterThan(value, 0);
                chunkSize = value;
            }
        }
    }
}