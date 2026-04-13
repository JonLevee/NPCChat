#nullable enable
using System.Text.Json.Serialization;

namespace NPCChat.Core.BehaviorClasses
{
    /// <summary>
    /// Serializable snapshot of an IActorTask's constructor parameters.
    /// Used to persist the ActionQueue and reconstruct tasks on load.
    /// Tasks restart from Begin() on load; mid-execution state restoration is deferred.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(Tasks.WanderTaskToken),        "wander")]
    [JsonDerivedType(typeof(Tasks.WaypointPatrolTaskToken), "waypoint_patrol")]
    [JsonDerivedType(typeof(Tasks.IdleTaskToken),           "idle")]
    [JsonDerivedType(typeof(Tasks.AttackTaskToken),         "attack")]
    [JsonDerivedType(typeof(Tasks.ChaseTaskToken),          "chase")]
    [JsonDerivedType(typeof(Tasks.FleeTaskToken),           "flee")]
    [JsonDerivedType(typeof(Tasks.CombatReadyTaskToken),    "combat_ready")]
    [JsonDerivedType(typeof(Tasks.InvestigateTaskToken),    "investigate")]
    public interface ITaskToken { }
}
