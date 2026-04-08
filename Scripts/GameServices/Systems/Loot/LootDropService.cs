using System.Collections.Generic;
using IdleDefense.GameServices.CoreServices.Inventory;
using IdleDefense.GameServices.Systems.EventBus;
using IdleDefense.Core.Items;
using IdleDefense.Core.Random;
using IdleDefense.Core.Stats;

namespace IdleDefense.GameServices.Systems.Loot
{
    /// <summary>
    /// Orchestrates loot rolls and notifies inventory + events.
    /// </summary>
    public sealed class LootDropService
    {
        private readonly IRandomSource _random;
        private readonly LootTableDefinition _table;

        public LootDropService(IRandomSource random, LootTableDefinition table)
        {
            _random = random;
            _table = table ?? new LootTableDefinition();
        }

        /// <summary>
        /// Returns true if a loot drop was generated (even if not equipped).
        /// </summary>
        public bool TryDropLoot(bool isBoss, int waveNumber, InventoryState inventory, GameEventBus bus)
        {
            if (inventory == null)
            {
                return false;
            }

            float dropChance = isBoss ? _table.BossDropChance : _table.NormalEnemyDropChance;
            if (_random.NextFloat() > dropChance)
            {
                return false;
            }

            ItemData item = GenerateItem(waveNumber, isBoss);
            bool equipped = inventory.TryAutoEquip(item);
            if (bus != null)
            {
                bus.LootDropped.Publish(new LootDroppedEvent(item, equipped));
            }

            return true;
        }

        private ItemData GenerateItem(int waveNumber, bool isBoss)
        {
            LootRarityTierDefinition tier = RollTier(_table.GetResolvedRarityTiers());
            float waveScale = 1f + ((System.Math.Max(1, waveNumber) - 1) * _table.WaveScalingPerWave);
            float bossScale = isBoss ? _table.BossItemScale : 1f;
            float rarityScale = tier.StatMultiplier;
            float finalScale = waveScale * bossScale * rarityScale;

            float atk = Lerp(_table.BaseAttackMin, _table.BaseAttackMax) * finalScale;
            float hp = Lerp(_table.BaseHpMin, _table.BaseHpMax) * finalScale;
            float asp = Lerp(_table.BaseAttackSpeedMin, _table.BaseAttackSpeedMax) * finalScale;

            ItemSlot slot = RollItemSlot();
            ItemData item = new ItemData
            {
                Slot = slot,
                ItemName = BuildItemName(tier, slot),
                RarityTierId = tier.TierId,
                RarityDisplayName = tier.DisplayName,
                Modifiers = new List<StatModifier>
                {
                    StatModifier.Flat(StatId.Attack, atk),
                    StatModifier.Flat(StatId.MaxHp, hp),
                    StatModifier.Flat(StatId.AttackSpeed, asp)
                }
            };

            return item;
        }

        private LootRarityTierDefinition RollTier(LootRarityTierDefinition[] tiers)
        {
            if (tiers == null || tiers.Length == 0)
            {
                tiers = LootTableDefinition.CreateDefaultRarityTiers();
            }

            float total = 0f;
            for (int i = 0; i < tiers.Length; i++)
            {
                float w = tiers[i].DropWeight;
                if (w > 0f)
                {
                    total += w;
                }
            }

            if (total <= 0f)
            {
                return tiers[0];
            }

            float r = _random.NextFloat() * total;
            float acc = 0f;
            for (int i = 0; i < tiers.Length; i++)
            {
                float w = tiers[i].DropWeight;
                if (w <= 0f)
                {
                    continue;
                }

                acc += w;
                if (r < acc)
                {
                    return tiers[i];
                }
            }

            return tiers[tiers.Length - 1];
        }

        private float Lerp(float a, float b)
        {
            float t = _random.NextFloat();
            return a + (b - a) * t;
        }

        private ItemSlot RollItemSlot()
        {
            int roll = _random.Range(0, 3);
            if (roll == 0)
            {
                return ItemSlot.Weapon;
            }

            if (roll == 1)
            {
                return ItemSlot.Armor;
            }

            return ItemSlot.Trinket;
        }

        private static string BuildItemName(LootRarityTierDefinition tier, ItemSlot slot)
        {
            string suffix = slot == ItemSlot.Weapon ? "Blade" : slot == ItemSlot.Armor ? "Core" : "Charm";
            return tier.DisplayName + " " + suffix;
        }
    }
}
