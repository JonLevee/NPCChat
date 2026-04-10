using System.Collections.Generic;
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
        /// <summary>
        /// Adds a Blacksmith NPC with a work/sleep schedule and a patrol route
        /// around their forge area. Adds a matching shop building at (shopX, shopY).
        /// </summary>
        public Templates AddBlacksmith(int x, int y, int shopX, int shopY)
        {
            var npc = new WorldObjectMoveable
            {
                Kind      = WorldObjectKind.NPC,
                Handle    = ObjectHandle.None,
                Bounds    = new Bounds(x, y, CharacterSize),
                MaxSpeed  = DefaultMoveSpeed,
                Character = new Character { Name = "Blacksmith", Archetype = "Blacksmith" },
                Actor     = BuildBlacksmithActor(x, y)
            };

            builder.World.AddObject(npc);
            AddStatic(WorldObjectKind.Building, shopX, shopY, BuildingSize.Small);
            return this;
        }

        private static ActorComponent BuildBlacksmithActor(int startX, int startY)
        {
            var actor = new ActorComponent
            {
                PerceptionRange       = 8f,
                DistantProcessInterval = 10
            };

            // Schedule: Work 6am–9pm, Sleep 10pm–5am
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
                Trigger  = ctx => ctx.Actor.Actor?.Mode == "Sleep"
                               && ctx.Actor.Movement.IsMoving == false
                               && ctx.Actor.Actor.ActionQueue.TryPeekHighest() is not IdleTask,
                ActionFactory = _ => new IdleTask(int.MaxValue, priority: 5)
            });

            // ── Dialogue ─────────────────────────────────────────────────────

            actor.DialogueTree = BuildBlacksmithDialogueTree();

            // Interaction verbs: "Talk" always visible; "Shop" only during business hours
            actor.Interactions.Add(new InteractionEntry
            {
                Label    = "Talk",
                NodeId   = "greet",
                Priority = 10
            });
            actor.Interactions.Add(new InteractionEntry
            {
                Label     = "Shop",
                NodeId    = "shop_menu",
                Priority  = 20,
                Condition = ctx => ctx.GameHour >= 6 && ctx.GameHour < 22
            });

            return actor;
        }

        private static DialogueTree BuildBlacksmithDialogueTree()
        {
            return new DialogueTreeBuilder("blacksmith")
                .MoodAxes(MoodAxes.Friendliness, MoodAxes.Irritability, MoodAxes.Trust)

                // Entry nodes (reached via InteractionEntry verbs)
                .AddPool("greet",
                    pool => pool
                        .Add("Aye, what can I do for ye?",
                             moodHints: new() { [MoodAxes.Friendliness] = 1f })
                        .Add("Hmph. What d'ye want.",
                             moodHints: new() { [MoodAxes.Irritability] = 1f })
                        .Add("Welcome! Browse around while I finish this.",
                             moodHints: new() { [MoodAxes.Friendliness] = 0.8f, [MoodAxes.Trust] = 0.5f }),
                    nextNodeId: "main_menu")

                .AddChoice("main_menu",
                    "What can I help ye with?",
                    choices => choices
                        .Add("Tell me about yourself.",   nextNodeId: "about")
                        .Add("I need something forged.",  nextNodeId: "forge_offer")
                        .Add("Just browsing. Farewell.",  nextNodeId: null))

                .AddNpcLine("about",
                    "Been at the forge thirty years. Weapons and armour both. " +
                    "Trained under old Harwick up north — best in the trade, he was.",
                    nextNodeId: "main_menu")

                .AddNpcLine("forge_offer",
                    "Aye, I can make ye something fine. " +
                    "Tell me what ye need and we'll settle on a price.",
                    nextNodeId: null)   // Phase 7 (Quests) will wire this further

                // Shop entry (reached via "Shop" verb)
                .AddChoice("shop_menu",
                    "Good to see ye. Here's what I've got in stock.",
                    choices => choices
                        .Add("Show me your weapons.",   nextNodeId: "shop_weapons")
                        .Add("Show me your armour.",    nextNodeId: "shop_armour")
                        .Add("Maybe another time.",     nextNodeId: null))

                .AddNpcLine("shop_weapons",
                    "Blades, axes, hammers — all forged right here. Take a look.",
                    nextNodeId: null)   // Phase 10 (Economy) will wire up item lists

                .AddNpcLine("shop_armour",
                    "Plate, chain, leather reinforcement. Made to last.",
                    nextNodeId: null)

                .Root("greet")
                .Build();
        }
    }
}
