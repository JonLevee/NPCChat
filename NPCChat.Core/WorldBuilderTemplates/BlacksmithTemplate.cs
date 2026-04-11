using System.Collections.Generic;
using System.Drawing;
using NPCChat.Core.BehaviorClasses;
using NPCChat.Core.BehaviorClasses.Tasks;
using NPCChat.Core.CharacterClasses;
using NPCChat.Core.DialogueClasses;
using NPCChat.Core.ShopClasses;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.WorldBuilderTemplates
{
    public partial class Templates
    {
        private const string BlacksmithFaction = "townsfolk";

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
                Character = new Character
                {
                    Name      = "Blacksmith",
                    Archetype = "Blacksmith",
                    FactionId = BlacksmithFaction
                },
                Actor = BuildBlacksmithActor(x, y),
                Shop  = BuildBlacksmithShop()
            };

            builder.World.AddObject(npc);
            AddStatic(WorldObjectKind.Building, shopX, shopY, BuildingSize.Small);
            return this;
        }

        private ActorComponent BuildBlacksmithActor(int startX, int startY)
        {
            var actor = new ActorComponent
            {
                PerceptionRange        = 8f,
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
                               && !ctx.Actor.Movement.IsMoving
                               && ctx.Actor.Actor.ActionQueue.TryPeekHighest() is not IdleTask,
                ActionFactory = _ => new IdleTask(int.MaxValue, priority: 5)
            });

            // ── Dialogue ─────────────────────────────────────────────────────

            actor.DialogueTree = BuildBlacksmithDialogueTree();

            // "Talk" — always visible
            actor.Interactions.Add(new InteractionEntry
            {
                Label    = "Talk",
                NodeId   = "greet",
                Priority = 10
            });
            // "Shop" — business hours only, blocked if player is Hostile
            actor.Interactions.Add(new InteractionEntry
            {
                Label     = "Shop",
                NodeId    = "shop_menu",
                Priority  = 20,
                Condition = ctx => ctx.GameHour >= 6 && ctx.GameHour < 22
                                && (ctx.GetPlayerReputation?.Invoke(BlacksmithFaction) ?? 0)
                                   >= -25   // above Hostile threshold
            });
            // "Quest" — business hours only, hidden after quest is complete
            actor.Interactions.Add(new InteractionEntry
            {
                Label     = "Quest",
                NodeId    = "quest_hub",
                Priority  = 30,
                Condition = ctx => ctx.GameHour >= 6 && ctx.GameHour < 22
                                && ctx.Player?.QuestLog?.HasCompletedQuest("fetch_iron_ore") != true
            });

            return actor;
        }

        private DialogueTree BuildBlacksmithDialogueTree()
        {
            var world      = builder.World;
            var staticData = builder.StaticData;

            const string questId  = "fetch_iron_ore";
            const string oreId    = "iron_ore";
            const int    oreReq   = 5;
            const int    repReward = 25;   // reputation granted on quest completion

            return new DialogueTreeBuilder("blacksmith")
                .MoodAxes(MoodAxes.Friendliness, MoodAxes.Irritability, MoodAxes.Trust)

                // ── Talk entry ────────────────────────────────────────────────
                //
                // Greeting pool now contains rep-aware entries.
                // Hostile rep (-25): curt, dismissive line.
                // Friendly/Honored (>=50): warmer welcome.
                // Default range: standard greetings.

                .AddPool("greet",
                    pool => pool
                        // Hostile — only shows at Hostile rep via requires gate (stat hack:
                        // we use the mood-axis system as a proxy; for explicit rep gating we
                        // use the node Condition on surrounding line nodes instead)
                        .Add("Aye, what can I do for ye?",
                             moodHints: new() { [MoodAxes.Friendliness] = 1f })
                        .Add("Hmph. What d'ye want.",
                             moodHints: new() { [MoodAxes.Irritability] = 1f })
                        .Add("Welcome! Browse around while I finish this.",
                             moodHints: new() { [MoodAxes.Friendliness] = 0.8f, [MoodAxes.Trust] = 0.5f }),
                    nextNodeId: "greet_rep_check")

                // After the pool line, show an extra warm line if player is Friendly+,
                // or a warning if Hostile. Both are conditional line nodes in a sequence.
                .AddSequence("greet_rep_check",
                    nodeIds: ["greet_friendly_bonus", "greet_hostile_warning", "main_menu"])

                .AddNpcLine("greet_friendly_bonus",
                    "Always good to see a friend of the town. What can I do for ye?",
                    condition: ctx => (ctx.GetPlayerReputation?.Invoke(BlacksmithFaction) ?? 0) >= 50,
                    nextNodeId: null)    // next is driven by the sequence (main_menu)

                .AddNpcLine("greet_hostile_warning",
                    "I know what the townsfolk say about ye. Don't push yer luck here.",
                    condition: ctx => (ctx.GetPlayerReputation?.Invoke(BlacksmithFaction) ?? 0) < -25,
                    nextNodeId: null)

                .AddChoice("main_menu",
                    "What can I help ye with?",
                    choices => choices
                        .Add("Tell me about yourself.",  nextNodeId: "about")
                        .Add("I need something forged.", nextNodeId: "forge_offer")
                        .Add("Just browsing. Farewell.", nextNodeId: null))

                .AddNpcLine("about",
                    "Been at the forge thirty years. Weapons and armour both. " +
                    "Trained under old Harwick up north — best in the trade, he was.",
                    nextNodeId: "main_menu")

                .AddNpcLine("forge_offer",
                    "Aye, I can make ye something fine. " +
                    "Tell me what ye need and we'll settle on a price.",
                    nextNodeId: null)

                // ── Shop entry ────────────────────────────────────────────────

                .AddChoice("shop_menu",
                    "Good to see ye. Here's what I've got in stock.",
                    choices => choices
                        .Add("Show me your weapons.",  nextNodeId: "shop_weapons")
                        .Add("Show me your armour.",   nextNodeId: "shop_armour")
                        .Add("Maybe another time.",    nextNodeId: null))

                .AddNpcLine("shop_weapons",
                    "Blades, axes, hammers — all forged right here. Take a look.",
                    nextNodeId: null)

                .AddNpcLine("shop_armour",
                    "Plate, chain, leather reinforcement. Made to last.",
                    nextNodeId: null)

                // ── Quest hub ─────────────────────────────────────────────────

                .AddSequence("quest_hub",
                    nodeIds:
                    [
                        "quest_done_ack",
                        "quest_give_reward",
                        "quest_not_enough",
                        "quest_offer"
                    ])

                .AddNpcLine("quest_done_ack",
                    "Ye've already done me a fine service. I thank ye.",
                    condition: ctx => ctx.Player?.QuestLog?.HasCompletedQuest(questId) == true)

                .AddLine("quest_give_reward",
                    speaker: "npc",
                    text: "Excellent work! Here be yer reward — ten gold coins, well earned.",
                    condition: ctx =>
                        ctx.Player?.QuestLog?.HasActiveQuest(questId) == true
                        && (ctx.GetPlayerItemCount?.Invoke(oreId) ?? 0) >= oreReq,
                    onEnter:
                    [
                        new DialogueEffect(ctx =>
                        {
                            if (ctx.Player is null) return;

                            ctx.Player.QuestLog?.TryComplete(questId);

                            // Reputation reward (UI thread — ReputationLog is UI-thread-owned)
                            ctx.Player.ReputationLog?.AddReputation(BlacksmithFaction, repReward);

                            // Consume ore and grant gold via the sim-thread command channel
                            world.EnqueueQuestReward(new QuestRewardCommand(
                                ctx.Player.Handle, oreId, ItemDef: null, oreReq, IsRemoval: true));

                            var goldDef = staticData.GetItem("gold_coin");
                            if (goldDef is not null)
                                world.EnqueueQuestReward(new QuestRewardCommand(
                                    ctx.Player.Handle, "gold_coin", goldDef, Quantity: 10, IsRemoval: false));
                        })
                    ])

                .AddNpcLine("quest_not_enough",
                    $"Ye don't have enough iron ore yet. I need {oreReq} pieces — bring them back when ye've got them.",
                    condition: ctx =>
                        ctx.Player?.QuestLog?.HasActiveQuest(questId) == true
                        && (ctx.GetPlayerItemCount?.Invoke(oreId) ?? 0) < oreReq)

                .AddChoice("quest_offer",
                    "I need five pieces of iron ore for me forge. The seam out east usually has some. Can ye fetch them for me?",
                    choices => choices
                        .Add("Aye, I'll get them for ye.",
                            nextNodeId: "quest_accepted",
                            onSelect:
                            [
                                new DialogueEffect(ctx =>
                                {
                                    var def = staticData.GetQuest(questId);
                                    if (def is not null)
                                        ctx.Player?.QuestLog?.StartQuest(def);
                                })
                            ])
                        .Add("Maybe another time.", nextNodeId: null),
                    condition: ctx => ctx.Player?.QuestLog?.CanStartQuest(questId) == true)

                .AddNpcLine("quest_accepted",
                    "Good. Five pieces of iron ore. Don't keep me waiting too long.",
                    nextNodeId: null)

                .Root("greet")
                .Build();
        }

        private ShopComponent BuildBlacksmithShop()
        {
            var shop = new ShopComponent { SellMultiplier = 0.5f };

            void AddStock(string id, float priceMult = 1.0f)
            {
                var def = builder.StaticData.GetItem(id);
                if (def is null) return;
                shop.Stock.Add(new ShopEntry
                {
                    ItemId          = id,
                    ItemDef         = def,
                    PriceMultiplier = priceMult
                });
            }

            AddStock("iron_sword");                // 40 gold — base price
            AddStock("leather_armour");            // 30 gold — base price
            AddStock("health_potion",  priceMult: 1.2f); // 18 gold — slight markup
            AddStock("iron_ore",       priceMult: 1.5f); // 8 gold  — selling raw materials premium
            AddStock("wood_plank",     priceMult: 1.0f); // 2 gold

            return shop;
        }
    }
}
