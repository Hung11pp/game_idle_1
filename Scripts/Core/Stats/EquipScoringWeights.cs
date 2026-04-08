using System.Collections.Generic;

namespace IdleDefense.Core.Stats
{
    /// <summary>
    /// Per-stat weights for comparing items. Unlisted stats default to 0 so new stats do not affect score until tuned.
    /// </summary>
    public sealed class EquipScoringWeights
    {
        private readonly Dictionary<StatId, float> _weights = new Dictionary<StatId, float>();

        public EquipScoringWeights()
        {
        }

        public EquipScoringWeights(IEnumerable<StatWeightPair> pairs)
        {
            if (pairs == null)
            {
                return;
            }

            foreach (StatWeightPair p in pairs)
            {
                _weights[p.Stat] = p.Weight;
            }
        }

        public float GetWeight(StatId id)
        {
            return _weights.TryGetValue(id, out float w) ? w : 0f;
        }

        /// <summary>Matches legacy sum(ATK + HP + ASPD) when those are the only non-zero bonuses.</summary>
        public static EquipScoringWeights LegacyFlatSum()
        {
            return new EquipScoringWeights(new[]
            {
                new StatWeightPair { Stat = StatId.Attack, Weight = 1f },
                new StatWeightPair { Stat = StatId.MaxHp, Weight = 1f },
                new StatWeightPair { Stat = StatId.AttackSpeed, Weight = 1f }
            });
        }
    }
}
