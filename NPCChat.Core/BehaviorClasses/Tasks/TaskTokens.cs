#nullable enable
using System.Drawing;

namespace NPCChat.Core.BehaviorClasses.Tasks
{
    // ── Token records — one per concrete IActorTask subclass ────────────────────
    // Each record captures only the constructor parameters needed to recreate the
    // task. On load, MoveableActionsFactory creates the task and calls Begin().

    public sealed record WanderTaskToken(
        int Priority,
        int WanderRadius) : ITaskToken;

    public sealed record WaypointPatrolTaskToken(
        int Priority,
        Point[] Waypoints) : ITaskToken;

    public sealed record IdleTaskToken(
        int Priority,
        int DurationTicks) : ITaskToken;

    public sealed record AttackTaskToken(
        int Priority) : ITaskToken;

    public sealed record ChaseTaskToken(
        int Priority) : ITaskToken;

    public sealed record FleeTaskToken(
        int Priority) : ITaskToken;

    public sealed record CombatReadyTaskToken(
        int Priority,
        int CloseRange,
        int TimeoutTicks) : ITaskToken;

    public sealed record InvestigateTaskToken(
        int Priority,
        int TargetX,
        int TargetY,
        int LookDuration) : ITaskToken;
}
