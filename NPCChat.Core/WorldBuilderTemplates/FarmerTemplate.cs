using NPCChat.Core.BehaviorClasses;
using NPCChat.Core.BehaviorClasses.Tasks;
using NPCChat.Core.CharacterClasses;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.WorldBuilderTemplates
{
    public partial class Templates
    {
        /// <summary>
        /// Adds a Farmer NPC with a work/rest schedule. During work hours the
        /// farmer wanders within the given radius; at night they idle.
        /// </summary>
        public Templates AddFarmer(int x, int y, int wanderRadius = 6)
        {
            var npc = new WorldObjectMoveable
            {
                Kind = WorldObjectKind.NPC,
                Handle = ObjectHandle.None,
                Bounds = new Bounds(x, y, CharacterSize),
                MaxSpeed = DefaultMoveSpeed,
                Character = new Character { Name = "Farmer", Archetype = "Farmer" },
                Actor = BuildFarmerActor(wanderRadius)
            };

            builder.World.AddObject(npc);
            return this;
        }

        private static ActorComponent BuildFarmerActor(int wanderRadius)
        {
            var actor = new ActorComponent
            {
                PerceptionRange = 6f,
                DistantProcessInterval = 10
            };

            // Schedule: Work 5am–7pm, Rest 7pm–5am
            actor.Schedule
                .Add(new GameTimeRange(5, 18), "Work")
                .Add(new GameTimeRange(19, 4), "Rest");

            actor.Mode = "Work";

            // Work task: wander nearby
            actor.ActionQueue.Enqueue(new WanderTask(wanderRadius, priority: 0));

            // Reactive rule: switch to idle when mode becomes "Rest"
            actor.ReactiveRules.Add(new BehaviorRule
            {
                Priority = 5,
                Trigger = ctx => ctx.Actor.Actor?.Mode == "Rest"
                               && ctx.Actor.Actor.ActionQueue.TryPeekHighest() is not IdleTask,
                ActionFactory = _ => new IdleTask(int.MaxValue, priority: 5)
            });

            return actor;
        }
    }
}
