using IdleDefense.GameServices.CoreServices.Combat;
using IdleDefense.GameServices.CoreServices.Inventory;
using IdleDefense.Core.Stats;

namespace IdleDefense.GameServices.CoreServices.Player
{
    /// <summary>
    /// Authoritative HP and death flag; final stats = base + inventory aggregation.
    /// </summary>
    public sealed class PlayerState
    {
        private readonly CombatService _combat;
        private readonly InventoryState _inventory;

        public StatBlock BaseStats { get; }

        public float CurrentHp { get; private set; }
        public bool IsDead { get; private set; }

        /// <summary>Used to preserve HP ratio when max HP changes from gear.</summary>
        private float _lastComputedMaxHp;

        public PlayerState(CombatService combat, InventoryState inventory, StatBlock baseStats)
        {
            _combat = combat;
            _inventory = inventory;
            BaseStats = baseStats ?? new StatBlock();
        }

        public StatBlock GetFinalStats()
        {
            StatBlock final = BaseStats.Clone();
            final.AddFrom(_inventory.GetAggregatedBonuses());
            return final;
        }

        public float GetMaxHp()
        {
            return GetFinalStats().Get(StatId.MaxHp);
        }

        public float GetAttack()
        {
            return GetFinalStats().Get(StatId.Attack);
        }

        public float GetAttackSpeed()
        {
            float v = GetFinalStats().Get(StatId.AttackSpeed);
            return v < 0.1f ? 0.1f : v;
        }

        public void RecalculateHpAfterStatChange(bool fillToFull)
        {
            float max = GetMaxHp();
            if (fillToFull || CurrentHp <= 0f)
            {
                CurrentHp = max;
                _lastComputedMaxHp = max;
                return;
            }

            float previousMax = _lastComputedMaxHp > 0f ? _lastComputedMaxHp : max;
            if (previousMax <= 0f)
            {
                previousMax = 1f;
            }

            float ratio = CurrentHp / previousMax;
            CurrentHp = ratio * max;
            if (CurrentHp > max)
            {
                CurrentHp = max;
            }

            if (CurrentHp < 0f)
            {
                CurrentHp = 0f;
            }

            _lastComputedMaxHp = max;
        }

        public void InitializeAtFullHealth()
        {
            IsDead = false;
            float max = GetMaxHp();
            CurrentHp = max;
            _lastComputedMaxHp = max;
        }

        public void TakeDamage(float rawDamage)
        {
            if (IsDead)
            {
                return;
            }

            float dmg = _combat.ClampDamage(rawDamage);
            CurrentHp -= dmg;
            if (CurrentHp <= 0f)
            {
                CurrentHp = 0f;
                IsDead = true;
            }
        }
    }
}
