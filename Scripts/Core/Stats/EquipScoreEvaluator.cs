namespace IdleDefense.Core.Stats
{
    /// <summary>
    /// Equip comparison score: DPS slice matches linearized marginal of combat DPS = Attack × AttackSpeed
    /// (see <see cref="CalculateDpsContribution"/>); other stats use per-stat normalization × weights.
    /// </summary>
    public static class EquipScoreEvaluator
    {
        /// <summary>Typical mid-range attack from loot bases (~1–3).</summary>
        public const float ReferenceAttack = 2f;

        /// <summary>Typical mid-range attacks/sec from loot bases (~0.03–0.12).</summary>
        public const float ReferenceAttackSpeed = 0.075f;

        /// <summary>Typical mid-range max HP from loot bases (~5–12).</summary>
        public const float ReferenceMaxHp = 8.5f;

        /// <summary>Default player attack range scale (see <c>PlayerBaseStatsSO</c>).</summary>
        public const float ReferenceAttackRange = 4f;

        /// <summary>Normalization for crit chance on 0–1 scale (tune with combat caps).</summary>
        public const float ReferenceCritChance = 0.25f;

        /// <summary>Normalization for additive crit multiplier bonus (e.g. +50% → 0.5).</summary>
        public const float ReferenceCritMultiplier = 0.5f;

        /// <summary>Normalization for lifesteal on 0–1 scale.</summary>
        public const float ReferenceLifeSteal = 0.1f;

        /// <summary>
        /// Linearized marginal DPS from item bonuses around <see cref="ReferenceAttack"/> / <see cref="ReferenceAttackSpeed"/>.
        /// Matches d(Attack × AttackSpeed) = S₀·ΔA + A₀·ΔS with the same APS floor as <c>CombatService</c> (0.1).
        /// When <paramref name="weights"/> is set, applies per-stat Attack / AttackSpeed weights to each term.
        /// Extend here for crit / damage multipliers without rewriting <see cref="ComputeScore"/>.
        /// </summary>
        public static float CalculateDpsContribution(StatBlock bonuses, EquipScoringWeights weights = null)
        {
            if (bonuses == null)
            {
                return 0f;
            }

            float dA = bonuses.Get(StatId.Attack);
            float dS = bonuses.Get(StatId.AttackSpeed);
            float a0 = ReferenceAttack > 1e-6f ? ReferenceAttack : 1f;
            float s0 = ReferenceAttackSpeed < 0.1f ? 0.1f : ReferenceAttackSpeed;

            float linear;
            if (weights != null)
            {
                linear = s0 * dA * weights.GetWeight(StatId.Attack) + a0 * dS * weights.GetWeight(StatId.AttackSpeed);
            }
            else
            {
                linear = s0 * dA + a0 * dS;
            }

            // Future: crit / global damage multipliers (multiply or add terms using bonuses + refs).
            return linear;
        }

        /// <summary>Weighted sum: DPS slice + normalized non-DPS stats × weights.</summary>
        public static float ComputeScore(StatBlock bonuses, EquipScoringWeights weights)
        {
            if (bonuses == null || weights == null)
            {
                return 0f;
            }

            float score = CalculateDpsContribution(bonuses, weights);

            foreach (System.Collections.Generic.KeyValuePair<StatId, float> pair in bonuses.EnumerateAllSorted())
            {
                if (pair.Key == StatId.Attack || pair.Key == StatId.AttackSpeed)
                {
                    continue;
                }

                float normalized = NormalizeStat(pair.Key, pair.Value);
                score += normalized * weights.GetWeight(pair.Key);
            }

            return score;
        }

        private static float NormalizeStat(StatId id, float raw)
        {
            switch (id)
            {
                case StatId.MaxHp:
                    return ReferenceMaxHp > 1e-6f ? raw / ReferenceMaxHp : raw;
                case StatId.AttackRange:
                    return ReferenceAttackRange > 1e-6f ? raw / ReferenceAttackRange : raw;
                case StatId.CritChance:
                    return ReferenceCritChance > 1e-6f ? raw / ReferenceCritChance : raw;
                case StatId.CritMultiplier:
                    return ReferenceCritMultiplier > 1e-6f ? raw / ReferenceCritMultiplier : raw;
                case StatId.LifeSteal:
                    return ReferenceLifeSteal > 1e-6f ? raw / ReferenceLifeSteal : raw;
                default:
                    return raw;
            }
        }
    }
}
