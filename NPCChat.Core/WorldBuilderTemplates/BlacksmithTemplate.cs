using System.Collections.Generic;
using System.Drawing;
using NPCChat.Core.BehaviorClasses;
using NPCChat.Core.BehaviorClasses.Tasks;
using NPCChat.Core.CharacterClasses;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.WorldBuilderTemplates
{
    public partial class Templates
    {
        /// <summary>
        /// Adds a Blacksmith NPC with a work/sleep schedule and a patrol route
        /// around their forge area. Adds a matching shop building at (shopX, shopY).
        /// </summary>
        public Templates AddBlacksmith(int x, int y, int shopX, int shopY)
        {
            var npc = new WorldObjectMoveable
            {
                Kind = WorldObjectKind.NPC,
                Handle = ObjectHandle.None,
                Bounds = new Bounds(x, y, CharacterSize),
                MaxSpeed = DefaultMoveSpeed,
                Character = new Character { Name = "Blacksmith", Archetype = "Blacksmith" },
                Actor = BuildBlacksmithActor(x, y)
            };

            builder.World.AddObject(npc);
            AddStatic(WorldObjectKind.Building, shopX, shopY, BuildingSize.Small);
            return this;
        }

        private static ActorComponent BuildBlacksmithActor(int startX, int startY)
        {
            var actor = new ActorComponent
            {
                PerceptionRange = 8f,
                DistantProcessInterval = 10
            };

            // Schedule: Work 6am–10pm, Sleep 10pm–6am
            actor.Schedule
                .Add(new GameTimeRange(6, 21), "Work")
                .Add(new GameTimeRange(22, 5), "Sleep");

            actor.Mode = "Work";

            // Work task: patrol a small loop around the forge
            var patrolPoints = new List<Point>
            {
                new(startX,     startY),
                new(startX + 2, startY),
                new(startX + 2, startY - 2),
                new(startX,     startY - 2)
            };
            actor.ActionQueue.Enqueue(new WaypointPatrolTask(patrolPoints, priority: 0));

            // Reactive rule: switch to idle when mode becomes "Sleep"
            actor.ReactiveRules.Add(new BehaviorRule
            {
                Priority = 5,
                Trigger = ctx => ctx.Actor.Actor?.Mode == "Sleep"
                               && ctx.Actor.Movement.IsMoving == false
                               && ctx.Actor.Actor.ActionQueue.TryPeekHighest() is not IdleTask,
                ActionFactory = _ => new IdleTask(int.MaxValue, priority: 5)
            });

            return actor;
        }
    }
}
