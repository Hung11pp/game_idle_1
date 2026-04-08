using System;
using System.Collections.Generic;
using System.Text;
using IdleDefense.Core.Stats;

namespace IdleDefense.Core.Items
{
    public enum ItemSlot
    {
        Weapon,
        Armor,
        Trinket
    }

    /// <summary>
    /// One loot item instance. Pure data; no Unity types. Bonuses are defined by <see cref="Modifiers"/> and aggregated on demand.
    /// </summary>
    [Serializable]
    public sealed class ItemData
    {
        public ItemSlot Slot;

        public string ItemName;

        /// <summary>Stable id from loot tier (e.g. common, rare).</summary>
        public string RarityTierId = string.Empty;

        /// <summary>Display label for UI (from loot table tier).</summary>
        public string RarityDisplayName = string.Empty;

        /// <summary>Authoritative stat contributions; aggregate with <see cref="StatAggregator"/>.</summary>
        public List<StatModifier> Modifiers = new List<StatModifier>();

        public StatBlock GetBonuses()
        {
            return StatAggregator.ToStatBlock(Modifiers);
        }

        public float GetEquipScore(EquipScoringWeights weights)
        {
            return EquipScoreEvaluator.ComputeScore(GetBonuses(), weights);
        }

        public string GetSummary()
        {
            StatBlock b = GetBonuses();
            StringBuilder sb = new StringBuilder();
            sb.Append(ItemName);
            if (!string.IsNullOrEmpty(RarityDisplayName))
            {
                sb.Append(" [").Append(RarityDisplayName).Append(']');
            }

            bool any = false;
            foreach (KeyValuePair<StatId, float> pair in b.EnumerateAllSorted())
            {
                if (System.Math.Abs(pair.Value) < 0.0001f)
                {
                    continue;
                }

                if (any)
                {
                    sb.Append("  ");
                }

                any = true;
                sb.Append(StatLabels.ToShortLabel(pair.Key)).Append(" +").Append(FormatStatValue(pair.Key, pair.Value));
            }

            if (!any)
            {
                sb.Append("  (no stats)");
            }

            return sb.ToString();
        }

        private static string FormatStatValue(StatId id, float v)
        {
            switch (id)
            {
                case StatId.AttackSpeed:
                case StatId.CritChance:
                case StatId.LifeSteal:
                    return v.ToString("0.00");
                default:
                    return v.ToString("0.0");
            }
        }
    }
}
