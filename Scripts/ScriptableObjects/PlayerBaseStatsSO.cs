using IdleDefense.Core.Stats;
using UnityEngine;

namespace IdleDefense.Presentation
{
    [CreateAssetMenu(fileName = "PlayerBaseStats", menuName = "IdleDefense/Player Base Stats", order = 0)]
    public sealed class PlayerBaseStatsSO : ScriptableObject
    {
        [Header("Combat")]
        public float MaxHp = 100f;
        public float Attack = 10f;
        public float AttackSpeed = 1f;
        public float AttackRange = 4f;

        public StatBlock ToStatBlock()
        {
            StatBlock block = new StatBlock();
            block.Set(StatId.MaxHp, MaxHp);
            block.Set(StatId.Attack, Attack);
            block.Set(StatId.AttackSpeed, AttackSpeed);
            block.Set(StatId.AttackRange, AttackRange);
            return block;
        }
    }
}
