#nullable enable
using System;

namespace NPCChat.Core.CombatClasses
{
    /// <summary>
    /// Health and attack statistics for a combat-capable actor.
    /// Owned by the simulation thread. Attach to <see cref="NPCChat.Core.WorldClasses.WorldObjectMoveable.Combat"/>.
    /// </summary>
    public sealed class CombatStats
    {
        private int _cooldownRemaining;

        public int MaxHp               { get; }
        public int CurrentHp           { get; private set; }
        public int Damage              { get; }

        /// <summary>Chebyshev tile range within which melee attacks land.</summary>
        public int AttackRange         { get; }

        /// <summary>Ticks that must elapse between attacks.</summary>
        public int AttackCooldownTicks { get; }

        public bool IsDead    => CurrentHp <= 0;
        public bool CanAttack => _cooldownRemaining <= 0;

        public CombatStats(
            int maxHp,
            int damage             = 10,
            int attackRange        = 1,
            int attackCooldownTicks = 8)
        {
            if (maxHp <= 0) throw new ArgumentOutOfRangeException(nameof(maxHp));
            MaxHp               = maxHp;
            CurrentHp           = maxHp;
            Damage              = damage;
            AttackRange         = attackRange;
            AttackCooldownTicks = attackCooldownTicks;
        }

        /// <summary>Reduces <see cref="CurrentHp"/> by <paramref name="amount"/>, clamped at zero.</summary>
        public void TakeDamage(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            CurrentHp = Math.Max(0, CurrentHp - amount);
        }

        /// <summary>Increases <see cref="CurrentHp"/> by <paramref name="amount"/>, clamped at <see cref="MaxHp"/>.</summary>
        public void Heal(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            CurrentHp = Math.Min(MaxHp, CurrentHp + amount);
        }

        /// <summary>Called every sim tick to decrement the attack cooldown.</summary>
        public void TickCooldown()
        {
            if (_cooldownRemaining > 0) _cooldownRemaining--;
        }

        /// <summary>Arms the cooldown after an attack fires.</summary>
        public void ResetCooldown()
        {
            _cooldownRemaining = AttackCooldownTicks;
        }

        /// <summary>Restores <see cref="CurrentHp"/> from a save record. Clamped to [0, MaxHp].</summary>
        public void RestoreCurrentHp(int hp) => CurrentHp = Math.Clamp(hp, 0, MaxHp);
    }
}
