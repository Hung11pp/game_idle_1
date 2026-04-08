namespace IdleDefense.GameServices.CoreServices.Combat
{
    /// <summary>
    /// Pure combat math and timers (no Unity). Pass <see cref="deltaTime"/> from the presentation layer.
    /// Shared by player auto-attack and enemy contact damage ticks.
    /// </summary>
    public sealed class CombatService
    {
        // --- Damage ---

        public float ClampDamage(float raw)
        {
            return raw < 0f ? 0f : raw;
        }

        /// <summary>Converts attacks per second into seconds between attacks (minimum 0.1 APS).</summary>
        public float SecondsPerAttack(float attacksPerSecond)
        {
            if (attacksPerSecond < 0.1f)
            {
                attacksPerSecond = 0.1f;
            }

            return 1f / attacksPerSecond;
        }

        /// <summary>Applies damage to health; clamps at zero; returns true if dead.</summary>
        public bool ApplyDamageToHealth(ref float currentHealth, float damageDealt)
        {
            currentHealth -= ClampDamage(damageDealt);
            if (currentHealth < 0f)
            {
                currentHealth = 0f;
            }

            return currentHealth <= 0f;
        }

        // --- Player-style auto-attack cooldown ---

        /// <summary>Subtracts elapsed time from remaining cooldown (call once per tick).</summary>
        public void TickCooldown(ref float cooldownRemaining, float deltaTime)
        {
            cooldownRemaining -= deltaTime;
        }

        /// <summary>True when the next swing is allowed (cooldown depleted).</summary>
        public bool IsAttackReady(float cooldownRemaining)
        {
            return cooldownRemaining <= 0f;
        }

        /// <summary>Starts the delay after a successful hit.</summary>
        public void BeginAttackCooldown(ref float cooldownRemaining, float attackIntervalSeconds)
        {
            cooldownRemaining = attackIntervalSeconds;
        }

        // --- Enemy-style periodic damage while in range ---

        /// <summary>
        /// While <paramref name="inContact"/> is false, resets the timer (ready to tick immediately on re-entry).
        /// While in contact, counts down and returns true once per period when a damage tick should occur.
        /// </summary>
        public bool TickPeriodicDamage(ref float timer, float deltaTime, bool inContact, float periodSeconds)
        {
            if (!inContact)
            {
                timer = 0f;
                return false;
            }

            timer -= deltaTime;
            if (timer > 0f)
            {
                return false;
            }

            timer = periodSeconds;
            return true;
        }
    }
}
