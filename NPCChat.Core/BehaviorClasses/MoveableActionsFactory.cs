#nullable enable
using System.Drawing;
using NPCChat.Core.Attributes;
using NPCChat.Core.BehaviorClasses.Tasks;

namespace NPCChat.Core.BehaviorClasses
{
    public interface IMoveableActionsFactory
    {
        /// <summary>
        /// Reconstructs an IActorTask from a serialized token.
        /// The caller is responsible for calling Begin() on the returned task
        /// before enqueueing it.
        /// </summary>
        IActorTask Create(ITaskToken token);
    }

    /// <summary>
    /// Maps serialized task tokens back to live IActorTask instances.
    /// Register all task types here; used by the save/load system.
    /// </summary>
    [Singleton(serviceType: typeof(IMoveableActionsFactory))]
    public sealed class MoveableActionsFactory : IMoveableActionsFactory
    {
        public IActorTask Create(ITaskToken token) => token switch
        {
            WanderTaskToken t =>
                new WanderTask(t.WanderRadius, t.Priority),

            WaypointPatrolTaskToken t =>
                new WaypointPatrolTask(t.Waypoints, t.Priority),

            IdleTaskToken t =>
                new IdleTask(t.DurationTicks, t.Priority),

            AttackTaskToken t =>
                new AttackTask(t.Priority),

            ChaseTaskToken t =>
                new ChaseTask(t.Priority),

            FleeTaskToken t =>
                new FleeTask(t.Priority),

            CombatReadyTaskToken t =>
                new CombatReadyTask(t.CloseRange, t.TimeoutTicks, t.Priority),

            InvestigateTaskToken t =>
                new InvestigateTask(new Point(t.TargetX, t.TargetY), t.Priority, t.LookDuration),

            _ => throw new InvalidOperationException(
                $"Unknown task token type: {token.GetType().Name}. " +
                $"Register it in MoveableActionsFactory.")
        };
    }
}
