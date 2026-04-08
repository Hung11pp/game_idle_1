using IdleDefense.GameServices.Config.Wave;
using UnityEngine;

namespace IdleDefense.Presentation
{
    /// <summary>
    /// Wave spawn counts, boss cadence, and optional timing overrides for <see cref="WaveSpawner"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveScaling", menuName = "IdleDefense/Wave Progression", order = 2)]
    public sealed class WaveScalingConfigSO : ScriptableObject
    {
        [Header("Boss")]
        [Tooltip("Every N waves spawns a boss (if enabled on spawner). 0 = no boss waves.")]
        public int BossWaveInterval = 10;

        public bool SpawnBossOnBossWave = true;

        [Header("Enemy count (linear per wave)")]
        public int BaseEnemiesPerWave = 6;

        [Tooltip("Added to count for each wave after the first.")]
        public int ExtraEnemiesPerWave = 2;

        [Header("Optional WaveSpawner timing")]
        [Tooltip("If > 0, overrides WaveSpawner.baseTimeBetweenSpawns when config is assigned.")]
        public float BaseTimeBetweenSpawns = 0f;

        [Tooltip("If > 0, overrides WaveSpawner.timeBetweenWaves when config is assigned.")]
        public float TimeBetweenWaves = 0f;

        public WaveProgressionDefinition ToDefinition()
        {
            return new WaveProgressionDefinition
            {
                BossWaveInterval = BossWaveInterval,
                SpawnBossOnBossWave = SpawnBossOnBossWave,
                BaseEnemiesPerWave = BaseEnemiesPerWave,
                ExtraEnemiesPerWave = ExtraEnemiesPerWave
            };
        }
    }
}
