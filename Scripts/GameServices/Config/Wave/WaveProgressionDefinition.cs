namespace IdleDefense.GameServices.Config.Wave
{
    /// <summary>
    /// Spawn counts and boss cadence (from <see cref="IdleDefense.Presentation.WaveScalingConfigSO"/> or defaults).
    /// </summary>
    public sealed class WaveProgressionDefinition
    {
        /// <summary>Spawn a boss when <see cref="IsBossWave"/> is true (e.g. every Nth wave).</summary>
        public bool SpawnBossOnBossWave = true;

        /// <summary>Boss waves occur when wave number is a positive multiple of this (0 = never).</summary>
        public int BossWaveInterval = 10;

        public int BaseEnemiesPerWave = 6;
        public int ExtraEnemiesPerWave = 2;

        public int GetEnemyCountForWave(int waveNumber)
        {
            int w = waveNumber < 1 ? 1 : waveNumber;
            return BaseEnemiesPerWave + ((w - 1) * ExtraEnemiesPerWave);
        }

        public bool IsBossWave(int waveNumber)
        {
            if (BossWaveInterval <= 0 || !SpawnBossOnBossWave)
            {
                return false;
            }

            return waveNumber > 0 && waveNumber % BossWaveInterval == 0;
        }
    }
}
