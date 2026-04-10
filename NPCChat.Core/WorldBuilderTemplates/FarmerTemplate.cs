using System.Drawing;
using NPCChat.Core.BehaviorClasses;
using NPCChat.Core.BehaviorClasses.Tasks;
using NPCChat.Core.CharacterClasses;
using NPCChat.Core.DialogueClasses;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.WorldBuilderTemplates
{
    public partial class Templates
    {
        private const string FarmerFaction = "townsfolk";
        /// <summary>
        /// Adds a Farmer NPC with a work/rest schedule. During work hours the
        /// farmer wanders within the given radius; at night they idle.
        /// </summary>
        public Templates AddFarmer(int x, int y, int wanderRadius = 6)
        {
            var npc = new WorldObjectMoveable
            {
                Kind      = WorldObjectKind.NPC,
                Handle    = ObjectHandle.None,
                Bounds    = new Bounds(x, y, CharacterSize),
                MaxSpeed  = DefaultMoveSpeed,
                Character = new Character { Name = "Farmer", Archetype = "Farmer", FactionId = FarmerFaction },
                Actor     = BuildFarmerActor(wanderRadius)
            };

            builder.World.AddObject(npc);
            return this;
        }

        private static ActorComponent BuildFarmerActor(int wanderRadius)
        {
            var actor = new ActorComponent
            {
                PerceptionRange       = 6f,
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
                Trigger  = ctx => ctx.Actor.Actor?.Mode == "Rest"
                               && ctx.Actor.Actor.ActionQueue.TryPeekHighest() is not IdleTask,
                ActionFactory = _ => new IdleTask(int.MaxValue, priority: 5)
            });

            // Reactive rule: flee from threat when player enters perception range.
            actor.ReactiveRules.Add(new BehaviorRule
            {
                Priority = 60,
                Trigger = ctx =>
                {
                    if (ctx.PlayerBounds is not { } pb) return false;
                    return ctx.Actor.Actor!.CanPerceive(ctx.Actor.Bounds, pb);
                },
                ActionFactory = _ => new FleeTask(priority: 60)
            });

            // Reactive rule: investigate a nearby alert (lower priority than flee).
            actor.ReactiveRules.Add(new BehaviorRule
            {
                Priority = 20,
                Trigger = ctx =>
                {
                    var center = new Point(
                        ctx.Actor.Bounds.Left + ctx.Actor.Bounds.Width  / 2,
                        ctx.Actor.Bounds.Top  + ctx.Actor.Bounds.Height / 2);
                    var alerts = ctx.GetNearbyAlerts?.Invoke(center, 15);
                    return alerts is { Length: > 0 };
                },
                ActionFactory = ctx =>
                {
                    var center = new Point(
                        ctx.Actor.Bounds.Left + ctx.Actor.Bounds.Width  / 2,
                        ctx.Actor.Bounds.Top  + ctx.Actor.Bounds.Height / 2);
                    var alerts = ctx.GetNearbyAlerts?.Invoke(center, 15) ?? [];
                    return new InvestigateTask(alerts[0].Position, priority: 20, lookDuration: 15);
                }
            });

            // ── Dialogue ─────────────────────────────────────────────────────

            actor.DialogueTree = BuildFarmerDialogueTree();

            actor.Interactions.Add(new InteractionEntry
            {
                Label    = "Talk",
                NodeId   = "greet",
                Priority = 10
            });
            // "Quest" available only during work hours; hidden if player is Hostile
            actor.Interactions.Add(new InteractionEntry
            {
                Label     = "Quest",
                NodeId    = "quest_hint",
                Priority  = 20,
                Condition = ctx => ctx.GameHour >= 5 && ctx.GameHour < 19
                                && (ctx.GetPlayerReputation?.Invoke(FarmerFaction) ?? 0) >= -25
            });

            return actor;
        }

        private static DialogueTree BuildFarmerDialogueTree()
        {
            return new DialogueTreeBuilder("farmer")
                .MoodAxes(MoodAxes.Friendliness, MoodAxes.Stress, MoodAxes.Trust)

                .AddPool("greet",
                    pool => pool
                        .Add("Morning! Fine day for the fields.",
                             moodHints: new() { [MoodAxes.Friendliness] = 1f })
                        .Add("Ugh, another long day ahead...",
                             moodHints: new() { [MoodAxes.Stress] = 1f })
                        .Add("Oh! Didn't hear ye coming. How do.",
                             moodHints: new() { [MoodAxes.Friendliness] = 0.5f }),
                    nextNodeId: "main_menu")

                .AddChoice("main_menu",
                    "Something on your mind?",
                    choices => choices
                        .Add("What are you growing?",        nextNodeId: "crops")
                        .Add("You look like you need help.", nextNodeId: "quest_hint")
                        .Add("Never mind. Take care.",       nextNodeId: null))

                .AddNpcLine("crops",
                    "Wheat mostly, some root veg. The soil here is good if ye keep up with it. " +
                    "Pests are the real problem — always something eating the crop.",
                    nextNodeId: "main_menu")

                .AddNpcLine("quest_hint",
                    "Ha! Could always use another pair of hands. " +
                    "Those blighted rats have been at the grain store again.",
                    nextNodeId: null)   // Phase 7 will wire up a proper quest here

                .Root("greet")
                .Build();
        }
    }
}
