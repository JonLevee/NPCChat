#nullable enable
using System;
using System.Drawing;
using NPCChat.Core.AIClasses;
using NPCChat.Core.WorldClasses;

namespace NPCChat.Core.BehaviorClasses.Tasks
{
    /// <summary>
    /// Halts the actor and strikes the player on cooldown while within melee range.
    /// Completes when the player leaves attack range, allowing a lower-priority task
    /// (e.g. <see cref="CombatReadyTask"/> or <see cref="ChaseTask"/>) to resume.
    /// Requires <see cref="NPCChat.Core.WorldClasses.WorldObjectMoveable.Combat"/> to be set on the actor.
    /// </summary>
    public sealed class AttackTask : ActorTaskBase
    {
        public AttackTask(int priority = 55) : base(priority) { }

        public override void Begin(in SimContext ctx)
        {
            base.Begin(ctx);
            ctx.Actor.Movement.ClearMovement();
        }

        protected override bool TickNoSubTasks(in SimContext ctx)
        {
            if (ctx.PlayerBounds is not { } playerBounds) return true;
            if (ctx.Actor.Combat is not { } myCombat)     return true;

            if (!IsWithin(ctx.Actor.Bounds, playerBounds, myCombat.AttackRange))
                return true;   // player backed away — yield to CombatReadyTask / ChaseTask

            myCombat.TickCooldown();

            if (!myCombat.CanAttack) return false;

            // Apply damage to the player's combat component.
            if (ctx.PlayerHandle is { } playerHandle)
            {
                var player = ctx.GetMoveable?.Invoke(playerHandle);
                player?.Combat?.TakeDamage(myCombat.Damage);
            }

            // Broadcast so nearby allies can converge on the fight.
            ctx.PostAlert?.Invoke(new AlertEvent
            {
                Position = new Point(
                    playerBounds.Left + playerBounds.Width  / 2,
                    playerBounds.Top  + playerBounds.Height / 2),
                Kind = AlertKind.CombatNoise,
                Tick = ctx.GameTick
            });

            myCombat.ResetCooldown();
            return false;
        }

        public override void Interrupt(in SimContext ctx)
            => ctx.Actor.Movement.ClearMovement();

        private static bool IsWithin(Bounds a, Bounds b, float range)
        {
            float dx = (a.Left + a.Width  * 0.5f) - (b.Left + b.Width  * 0.5f);
            float dy = (a.Top  + a.Height * 0.5f) - (b.Top  + b.Height * 0.5f);
            return Math.Max(Math.Abs(dx), Math.Abs(dy)) <= range;
        }
    }
}
