using System;

namespace IdleDefense.GameServices.Config.Scaling
{
    /// <summary>
    /// Plain wave scaling parameters (from ScriptableObject or defaults).
    /// Difficulty uses compound (exponential) growth per wave: Pow(growth, max(0, wave - 1)).
    /// </summary>
    public sealed class EnemyScalingDefinition
    {
        /// <summary>Compound health multiplier per wave step (e.g. 1.12 ≈ +12% per wave).</summary>
        public float HealthGrowthPerWave = 1.12f;

        /// <summary>Compound damage multiplier per wave step.</summary>
        public float DamageGrowthPerWave = 1.06f;

        /// <summary>Compound spawn-rate multiplier per wave step.</summary>
        public float SpawnRateGrowthPerWave = 1.042f;

        public float BossHealthMultiplier = 4f;
        public float BossDamageMultiplier = 2f;
        public float BossSpeedMultiplier = 0.9f;

        /// <summary>
        /// Global multipliers applied on top of enemy base stats (exponential in wave index).
        /// </summary>
        public void GetDifficultyMultipliers(int currentWave, out float enemyHealthMultiplier, out float enemyDamageMultiplier, out float spawnRateMultiplier)
        {
            int n = WaveIndexFromOne(currentWave);

            enemyHealthMultiplier = (float)Math.Pow(HealthGrowthPerWave, n);
            enemyDamageMultiplier = (float)Math.Pow(DamageGrowthPerWave, n);
            spawnRateMultiplier = (float)Math.Pow(SpawnRateGrowthPerWave, n);
        }

        private static int WaveIndexFromOne(int currentWave)
        {
            int w = currentWave < 1 ? 1 : currentWave;
            int idx = w - 1;
            return idx < 0 ? 0 : idx;
        }
    }
}
