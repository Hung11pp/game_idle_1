using System;
using System.Collections.Generic;
using System.Text;
using IdleDefense.Core.Items;
using IdleDefense.Core.Stats;

namespace IdleDefense.GameServices.CoreServices.Inventory
{
    /// <summary>
    /// Equipment slots + aggregation into one StatBlock for the player.
    /// </summary>
    public sealed class InventoryState
    {
        private readonly EquipScoringWeights _equipWeights;

        public ItemData Weapon;
        public ItemData Armor;
        public ItemData Trinket;

        public event Action EquipmentChanged;

        public InventoryState(EquipScoringWeights equipWeights = null)
        {
            _equipWeights = equipWeights ?? EquipScoringWeights.LegacyFlatSum();
        }

        public bool TryAutoEquip(ItemData newItem)
        {
            if (newItem == null)
            {
                return false;
            }

            switch (newItem.Slot)
            {
                case ItemSlot.Weapon:
                    return TryEquipToSlot(newItem, ref Weapon);
                case ItemSlot.Armor:
                    return TryEquipToSlot(newItem, ref Armor);
                default:
                    return TryEquipToSlot(newItem, ref Trinket);
            }
        }

        private bool TryEquipToSlot(ItemData newItem, ref ItemData equipped)
        {
            float newScore = newItem.GetEquipScore(_equipWeights);
            float oldScore = equipped == null ? float.NegativeInfinity : equipped.GetEquipScore(_equipWeights);

            if (equipped == null || newScore > oldScore)
            {
                equipped = newItem;
                EquipmentChanged?.Invoke();
                return true;
            }

            EquipmentChanged?.Invoke();
            return false;
        }

        public StatBlock GetAggregatedBonuses()
        {
            StatBlock sum = new StatBlock();
            AddItem(sum, Weapon);
            AddItem(sum, Armor);
            AddItem(sum, Trinket);
            return sum;
        }

        private static void AddItem(StatBlock target, ItemData item)
        {
            if (item == null)
            {
                return;
            }

            target.AddFrom(item.GetBonuses());
        }

        public string GetEquippedSummary()
        {
            StatBlock agg = GetAggregatedBonuses();
            StringBuilder sb = new StringBuilder();
            sb.Append("Weapon: ").AppendLine(GetItemLine(Weapon));
            sb.Append("Armor: ").AppendLine(GetItemLine(Armor));
            sb.Append("Trinket: ").AppendLine(GetItemLine(Trinket));
            sb.Append("Bonuses");

            bool any = false;
            foreach (KeyValuePair<StatId, float> pair in agg.EnumerateAllSorted())
            {
                if (System.Math.Abs(pair.Value) < 0.0001f)
                {
                    continue;
                }

                sb.Append("  ").Append(StatLabels.ToShortLabel(pair.Key)).Append(" +").Append(FormatAggValue(pair.Key, pair.Value));
                any = true;
            }

            if (!any)
            {
                sb.Append("  (none)");
            }

            return sb.ToString();
        }

        private static string FormatAggValue(StatId id, float v)
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

        private static string GetItemLine(ItemData item)
        {
            return item == null ? "None" : item.GetSummary();
        }
    }
}
