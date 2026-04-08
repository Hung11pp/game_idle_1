using IdleDefense.GameServices.Config.Scaling;
using UnityEngine;

namespace IdleDefense.Presentation
{
    [CreateAssetMenu(fileName = "EnemyScaling", menuName = "IdleDefense/Enemy Scaling", order = 1)]
    public sealed class EnemyScalingConfigSO : ScriptableObject
    {
        [Header("Exponential wave curve")]
        [Tooltip("Per-wave compound factor for enemy HP scaling: HP mult = Pow(this, wave-1).")]
        public float HealthGrowthPerWave = 1.12f;

        [Tooltip("Per-wave compound factor for enemy damage scaling.")]
        public float DamageGrowthPerWave = 1.06f;

        [Tooltip("Per-wave compound factor for spawn rate scaling.")]
        public float SpawnRateGrowthPerWave = 1.042f;

        [Header("Boss multipliers")]
        public float BossHealthMultiplier = 4f;
        public float BossDamageMultiplier = 2f;
        public float BossSpeedMultiplier = 0.9f;

        public EnemyScalingDefinition ToDefinition()
        {
            return new EnemyScalingDefinition
            {
                HealthGrowthPerWave = HealthGrowthPerWave,
                DamageGrowthPerWave = DamageGrowthPerWave,
                SpawnRateGrowthPerWave = SpawnRateGrowthPerWave,
                BossHealthMultiplier = BossHealthMultiplier,
                BossDamageMultiplier = BossDamageMultiplier,
                BossSpeedMultiplier = BossSpeedMultiplier
            };
        }
    }
}
