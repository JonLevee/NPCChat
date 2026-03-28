using NPCChat.Core.Validation;
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
                Require.IsGreaterThan(0, value);
                chunkSize = value;
            }
        }
    }
}