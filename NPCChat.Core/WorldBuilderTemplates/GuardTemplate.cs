using System;
using System.Collections.Generic;
using System.Drawing;
using NPCChat.Core.AIClasses;
using NPCChat.Core.BehaviorClasses;
using NPCChat.Core.BehaviorClasses.Tasks;
using NPCChat.Core.CharacterClasses;
using NPCChat.Core.DialogueClasses;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.WorldBuilderTemplates
{
    public partial class Templates
    {
        private const string GuardFaction = "townsfolk";
        private const int    GuardCloseRange    = 4;   // tiles — triggers CombatReady
        private const int    GuardAlertRadius   = 20;  // tiles — how far a guard hears alerts

        /// <summary>
        /// Adds a Guard NPC that patrols a square route and reacts to the player
        /// with Chase → CombatReady escalation and group-alert investigation.
        /// </summary>
        public Templates AddGuard(int x, int y, int patrolRadius = 4)
        {
            var npc = new WorldObjectMoveable
            {
                Kind      = WorldObjectKind.NPC,
                Handle    = ObjectHandle.None,
                Bounds    = new Bounds(x, y, CharacterSize),
                MaxSpeed  = DefaultMoveSpeed,
                Character = new Character
                {
                    Name      = "Guard",
                    Archetype = "Guard",
                    FactionId = GuardFaction
                },
                Actor = BuildGuardActor(x, y, patrolRadius)
            };

            builder.World.AddObject(npc);
            return this;
        }

        private static ActorComponent BuildGuardActor(int startX, int startY, int patrolRadius)
        {
            var actor = new ActorComponent
            {
                PerceptionRange        = 10f,
                DistantProcessInterval = 5   // guards check more frequently than civilians
            };

            // No schedule — guards are always on duty.

            // Base task: square patrol around starting position.
            var patrolPoints = new List<Point>
            {
                new(startX,               startY),
                new(startX + patrolRadius, startY),
                new(startX + patrolRadius, startY + patrolRadius),
                new(startX,               startY + patrolRadius)
            };
            actor.ActionQueue.Enqueue(new WaypointPatrolTask(patrolPoints, priority: 0));

            // ── Reactive rules (evaluated highest-priority-first each tick) ──────

            // Priority 50 — CombatReady: hold position when player is right next to us.
            actor.ReactiveRules.Add(new BehaviorRule
            {
                Priority = 50,
                Trigger = ctx =>
                {
                    if (ctx.PlayerBounds is not { } pb) return false;
                    return ChebyshevDistance(ctx.Actor.Bounds, pb) <= GuardCloseRange;
                },
                ActionFactory = _ => new CombatReadyTask(
                    closeRange:    GuardCloseRange,
                    timeoutTicks:  40,
                    priority:      50)
            });

            // Priority 40 — Chase: pursue player when perceived (and not already very close).
            actor.ReactiveRules.Add(new BehaviorRule
            {
                Priority = 40,
                Trigger = ctx =>
                {
                    if (ctx.PlayerBounds is not { } pb) return false;
                    return ctx.Actor.Actor!.CanPerceive(ctx.Actor.Bounds, pb);
                },
                ActionFactory = _ => new ChaseTask(priority: 40)
            });

            // Priority 25 — InvestigateAlert: respond to a nearby PlayerDetected alert.
            // Fires only when patrol (0) or wander is running (25 > 0).
            // Won't preempt Chase (40) or CombatReady (50).
            actor.ReactiveRules.Add(new BehaviorRule
            {
                Priority = 25,
                Trigger = ctx =>
                {
                    var center = ActorCenter(ctx.Actor.Bounds);
                    var alerts = ctx.GetNearbyAlerts?.Invoke(center, GuardAlertRadius);
                    return alerts is { Length: > 0 };
                },
                ActionFactory = ctx =>
                {
                    var center = ActorCenter(ctx.Actor.Bounds);
                    var alerts = ctx.GetNearbyAlerts?.Invoke(center, GuardAlertRadius) ?? [];
                    return new InvestigateTask(alerts[0].Position, priority: 25, lookDuration: 30);
                }
            });

            // ── Dialogue ─────────────────────────────────────────────────────────

            actor.DialogueTree = BuildGuardDialogueTree();

            actor.Interactions.Add(new InteractionEntry
            {
                Label    = "Talk",
                NodeId   = "greet",
                Priority = 10
            });

            return actor;
        }

        private static DialogueTree BuildGuardDialogueTree()
        {
            return new DialogueTreeBuilder("guard")
                .MoodAxes(MoodAxes.Friendliness, MoodAxes.Stress)

                .AddPool("greet",
                    pool => pool
                        .Add("Move along, citizen.",
                             moodHints: new() { [MoodAxes.Stress] = 1f })
                        .Add("Keep the peace and we'll have no trouble.",
                             moodHints: new() { [MoodAxes.Friendliness] = 0.5f })
                        .Add("Eyes open. Strange times of late.",
                             moodHints: new() { [MoodAxes.Stress] = 0.5f }),
                    nextNodeId: null)

                .Root("greet")
                .Build();
        }

        // ── Geometry helpers ────────────────────────────────────────────────────

        private static Point ActorCenter(Bounds b) =>
            new(b.Left + b.Width / 2, b.Top + b.Height / 2);

        private static float ChebyshevDistance(Bounds a, Bounds b)
        {
            float dx = MathF.Abs((a.Left + a.Width  * 0.5f) - (b.Left + b.Width  * 0.5f));
            float dy = MathF.Abs((a.Top  + a.Height * 0.5f) - (b.Top  + b.Height * 0.5f));
            return Math.Max(dx, dy);
        }
    }
}
