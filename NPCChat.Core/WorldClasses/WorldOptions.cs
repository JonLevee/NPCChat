using NPCChat.Core.SupportClasses;
using NPCChat.Core.Validation;
using NPCChat.Core.Attributes;

namespace NPCChat.Core.WorldClasses
{
    public interface IWorldOptions
    {
        ChunkInfo ChunkInfo { get; }

        /// <summary>Simulation loop tick interval in milliseconds. Default: 50 ms (20 ticks/sec).</summary>
        int SimTickMs { get; }

        /// <summary>
        /// Maximum number of pending MoveCommands before the simulation raises a
        /// SimulationOverflowException. Default: 500.
        /// </summary>
        int MoveCommandQueueThreshold { get; }

        /// <summary>
        /// Maximum number of A* path queries processed per simulation tick.
        /// Excess queries carry over to the next tick. Default: 20.
        /// </summary>
        int PathBudgetPerTick { get; }

        /// <summary>
        /// When true, the Editor draws a preview of the path that would be taken
        /// to the cursor position while a moveable object is selected.
        /// Has no effect in Unity builds (debug aid only).
        /// </summary>
        bool ShowPathPreview { get; set; }
    }

    [Scoped(serviceType: typeof(IWorldOptions))]
    public sealed class WorldOptions : IWorldOptions
    {
        public ChunkInfo ChunkInfo { get; }
        public int SimTickMs { get; set; } = 50;
        public int MoveCommandQueueThreshold { get; set; } = 500;
        public int PathBudgetPerTick { get; set; } = 20;
        public bool ShowPathPreview { get; set; } = false;

        public WorldOptions(ChunkInfo chunkInfo)
        {
            ChunkInfo = chunkInfo;
        }
    }
}
