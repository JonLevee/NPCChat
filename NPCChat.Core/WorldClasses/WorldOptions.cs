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
        /// Maximum number of nodes A* will explore before giving up and returning null.
        /// Prevents unbounded search on large open maps. Default: 50,000.
        /// </summary>
        int MaxPathIterations { get; }

        /// <summary>
        /// When true, the Editor draws a preview of the path that would be taken
        /// to the cursor position while a moveable object is selected.
        /// Has no effect in Unity builds (debug aid only).
        /// </summary>
        bool ShowPathPreview { get; set; }

        /// <summary>
        /// Number of sim ticks that equal one in-game hour.
        /// Default: 72 ticks → at 20 ticks/sec one game day lasts ~86 real seconds.
        /// </summary>
        int TicksPerGameHour { get; }
    }

    [Scoped(serviceType: typeof(IWorldOptions))]
    public sealed class WorldOptions : IWorldOptions
    {
        public ChunkInfo ChunkInfo { get; }
        public int SimTickMs { get; set; } = 50;
        public int MoveCommandQueueThreshold { get; set; } = 500;
        public int PathBudgetPerTick { get; set; } = 20;
        public int MaxPathIterations { get; set; } = 50_000;
        public bool ShowPathPreview { get; set; } = false;
        public int TicksPerGameHour { get; set; } = 72;

        public WorldOptions(ChunkInfo chunkInfo)
        {
            ChunkInfo = chunkInfo;
        }
    }
}
