using NPCChat.Core.SupportClasses;
using NPCChat.Core.Validation;
using NPCChatLib.Attributes;

namespace NPCChatLib.WorldClasses
{
    public interface IWorldOptions
    {
        ChunkInfo ChunkInfo { get; }
    }

    [Scoped(serviceType: typeof(IWorldOptions))]
    public sealed class WorldOptions : IWorldOptions
    {
        public ChunkInfo ChunkInfo { get; }

        public WorldOptions(ChunkInfo chunkInfo)
        {
            ChunkInfo = chunkInfo;
        }
    }
}