namespace IdleDefense.Core.Enemy
{
    /// <summary>
    /// Runtime combat/movement stats for one enemy instance (pool-friendly).
    /// </summary>
    public sealed class EnemyState
    {
        public bool IsBoss;
        public int WaveNumber;

        public float MaxHealth;
        public float CurrentHealth;
        public float ContactDamage;
        public float MoveSpeed;
        public float ContactRange;
        public float DamageTickSeconds;

        public bool IsAlive => CurrentHealth > 0f;

        public void ResetFromSpawn(float maxHp, float contactDamage, float moveSpeed, float contactRange, float damageTickSeconds, bool isBoss, int waveNumber)
        {
            IsBoss = isBoss;
            WaveNumber = waveNumber;
            MaxHealth = maxHp;
            CurrentHealth = maxHp;
            ContactDamage = contactDamage;
            MoveSpeed = moveSpeed;
            ContactRange = contactRange;
            DamageTickSeconds = damageTickSeconds;
        }

        public void ApplyDamage(float amount)
        {
            if (!IsAlive)
            {
                return;
            }

            CurrentHealth -= amount;
            if (CurrentHealth < 0f)
            {
                CurrentHealth = 0f;
            }
        }
    }
}
