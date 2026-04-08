using IdleDefense.Core.Math;

namespace IdleDefense.GameServices.Systems.EventBus
{
    /// <summary>
    /// Published when an enemy's health reaches zero (before pool return).
    /// </summary>
    public readonly struct EnemyDiedEvent
    {
        public readonly Position3 WorldPosition;
        public readonly bool IsBoss;
        public readonly int WaveNumber;

        public EnemyDiedEvent(Position3 worldPosition, bool isBoss, int waveNumber)
        {
            WorldPosition = worldPosition;
            IsBoss = isBoss;
            WaveNumber = waveNumber;
        }
    }
}
