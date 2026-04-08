using IdleDefense.GameServices.Systems.Loot;
using IdleDefense.Core.Stats;
using UnityEngine;

namespace IdleDefense.Presentation
{
    [System.Serializable]
    public sealed class LootRarityTierEntry
    {
        [Tooltip("Stable id used on generated items (e.g. common, rare).")]
        public string TierId = "common";

        [Tooltip("Shown in UI and item names.")]
        public string DisplayName = "Common";

        [Tooltip("Relative weight in the weighted rarity roll.")]
        [Min(0f)]
        public float DropWeight = 70f;

        [Tooltip("Multiplies rolled base stats for this tier.")]
        [Min(0f)]
        public float StatMultiplier = 1f;
    }

    /// <summary>
    /// Designer-facing loot config: drop rates, weighted rarity tiers, base stat bands, and auto-equip weights.
    /// </summary>
    [CreateAssetMenu(fileName = "LootTable", menuName = "IdleDefense/Loot Table", order = 2)]
    public sealed class LootTableSO : ScriptableObject
    {
        [Header("Drop Chances")]
        [Range(0f, 1f)] public float NormalEnemyDropChance = 0.35f;
        [Range(0f, 1f)] public float BossDropChance = 1f;

        [Header("Weighted Rarity Tiers")]
        [Tooltip("Leave empty to use code defaults (Common / Rare / Epic at 70 / 25 / 5).")]
        public LootRarityTierEntry[] RarityTiers =
        {
            new LootRarityTierEntry { TierId = "common", DisplayName = "Common", DropWeight = 70f, StatMultiplier = 1f },
            new LootRarityTierEntry { TierId = "rare", DisplayName = "Rare", DropWeight = 25f, StatMultiplier = 1.6f },
            new LootRarityTierEntry { TierId = "epic", DisplayName = "Epic", DropWeight = 5f, StatMultiplier = 2.4f }
        };

        [Header("Base Stat Rolls (before wave / boss / rarity multipliers)")]
        public float BaseAttackMin = 1f;
        public float BaseAttackMax = 3f;
        public float BaseHpMin = 5f;
        public float BaseHpMax = 12f;
        public float BaseAttackSpeedMin = 0.03f;
        public float BaseAttackSpeedMax = 0.12f;

        [Header("Scaling")]
        public float WaveScalingPerWave = 0.15f;
        public float BossItemScale = 1.5f;

        [Header("Auto-Equip Scoring (weighted sum of stat values)")]
        [Tooltip("Unlisted stats default to weight 0. Increase Crit weight when you add crit to loot.")]
        public StatWeightPair[] EquipScoreWeights =
        {
            new StatWeightPair { Stat = StatId.Attack, Weight = 1f },
            new StatWeightPair { Stat = StatId.MaxHp, Weight = 1f },
            new StatWeightPair { Stat = StatId.AttackSpeed, Weight = 1f },
            new StatWeightPair { Stat = StatId.CritChance, Weight = 0f },
            new StatWeightPair { Stat = StatId.LifeSteal, Weight = 0f }
        };

        public LootTableDefinition ToDefinition()
        {
            LootRarityTierDefinition[] tiers = null;
            if (RarityTiers != null && RarityTiers.Length > 0)
            {
                tiers = new LootRarityTierDefinition[RarityTiers.Length];
                for (int i = 0; i < RarityTiers.Length; i++)
                {
                    LootRarityTierEntry e = RarityTiers[i];
                    tiers[i] = new LootRarityTierDefinition
                    {
                        TierId = e.TierId,
                        DisplayName = e.DisplayName,
                        DropWeight = e.DropWeight,
                        StatMultiplier = e.StatMultiplier
                    };
                }
            }

            return new LootTableDefinition
            {
                NormalEnemyDropChance = NormalEnemyDropChance,
                BossDropChance = BossDropChance,
                RarityTiers = tiers,
                BaseAttackMin = BaseAttackMin,
                BaseAttackMax = BaseAttackMax,
                BaseHpMin = BaseHpMin,
                BaseHpMax = BaseHpMax,
                BaseAttackSpeedMin = BaseAttackSpeedMin,
                BaseAttackSpeedMax = BaseAttackSpeedMax,
                WaveScalingPerWave = WaveScalingPerWave,
                BossItemScale = BossItemScale,
                EquipScoreWeights = EquipScoreWeights != null && EquipScoreWeights.Length > 0
                    ? (StatWeightPair[])EquipScoreWeights.Clone()
                    : null
            };
        }
    }
}
