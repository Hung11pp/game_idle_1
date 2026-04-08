using IdleDefense.Core.Stats;

namespace IdleDefense.GameServices.Systems.Loot
{
    /// <summary>
    /// Plain data copied from ScriptableObject at runtime (no Unity in Core logic path).
    /// </summary>
    public sealed class LootTableDefinition
    {
        public float NormalEnemyDropChance = 0.35f;
        public float BossDropChance = 1f;

        /// <summary>Weighted tiers; if null or empty, <see cref="LootDropService"/> uses built-in defaults.</summary>
        public LootRarityTierDefinition[] RarityTiers;

        public float BaseAttackMin = 1f;
        public float BaseAttackMax = 3f;
        public float BaseHpMin = 5f;
        public float BaseHpMax = 12f;
        public float BaseAttackSpeedMin = 0.03f;
        public float BaseAttackSpeedMax = 0.12f;

        public float WaveScalingPerWave = 0.15f;
        public float BossItemScale = 1.5f;

        /// <summary>Weights for auto-equip comparison (items are scored via <see cref="EquipScoringWeights"/>).</summary>
        public StatWeightPair[] EquipScoreWeights = DefaultEquipWeights();

        public EquipScoringWeights ToEquipScoringWeights()
        {
            if (EquipScoreWeights == null || EquipScoreWeights.Length == 0)
            {
                return EquipScoringWeights.LegacyFlatSum();
            }

            return new EquipScoringWeights(EquipScoreWeights);
        }

        private static StatWeightPair[] DefaultEquipWeights()
        {
            return new[]
            {
                new StatWeightPair { Stat = StatId.Attack, Weight = 1f },
                new StatWeightPair { Stat = StatId.MaxHp, Weight = 1f },
                new StatWeightPair { Stat = StatId.AttackSpeed, Weight = 1f }
            };
        }

        public LootRarityTierDefinition[] GetResolvedRarityTiers()
        {
            if (RarityTiers != null && RarityTiers.Length > 0)
            {
                return RarityTiers;
            }

            return CreateDefaultRarityTiers();
        }

        public static LootRarityTierDefinition[] CreateDefaultRarityTiers()
        {
            return new[]
            {
                new LootRarityTierDefinition
                {
                    TierId = "common",
                    DisplayName = "Common",
                    DropWeight = 70f,
                    StatMultiplier = 1f
                },
                new LootRarityTierDefinition
                {
                    TierId = "rare",
                    DisplayName = "Rare",
                    DropWeight = 25f,
                    StatMultiplier = 1.6f
                },
                new LootRarityTierDefinition
                {
                    TierId = "epic",
                    DisplayName = "Epic",
                    DropWeight = 5f,
                    StatMultiplier = 2.4f
                }
            };
        }
    }
}
