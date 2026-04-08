namespace IdleDefense.Core.Enemy
{
    /// <summary>
    /// Final enemy stats: base values × GameManager difficulty multipliers × boss multipliers.
    /// Per-wave scaling is fully expressed in GameManager multipliers (see EnemyScalingDefinition).
    /// </summary>
    public static class EnemyCombatMath
    {
        public static void ComputeFinalStats(
            float baseMaxHealth,
            float baseDamage,
            float baseMoveSpeed,
            float gameManagerHealthMultiplier,
            float gameManagerDamageMultiplier,
            bool isBoss,
            float bossHealthMultiplier,
            float bossDamageMultiplier,
            float bossSpeedMultiplier,
            out float maxHealth,
            out float contactDamage,
            out float moveSpeed)
        {
            maxHealth = baseMaxHealth * gameManagerHealthMultiplier;
            contactDamage = baseDamage * gameManagerDamageMultiplier;
            moveSpeed = baseMoveSpeed;

            if (isBoss)
            {
                maxHealth *= bossHealthMultiplier;
                contactDamage *= bossDamageMultiplier;
                moveSpeed *= bossSpeedMultiplier;
            }
        }
    }
}
