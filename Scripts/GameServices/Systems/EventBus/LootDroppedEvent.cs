using IdleDefense.Core.Items;

namespace IdleDefense.GameServices.Systems.EventBus
{
    /// <summary>
    /// Published after an item is generated (whether or not it was equipped).
    /// </summary>
    public readonly struct LootDroppedEvent
    {
        public readonly ItemData Item;
        public readonly bool WasEquipped;

        public LootDroppedEvent(ItemData item, bool wasEquipped)
        {
            Item = item;
            WasEquipped = wasEquipped;
        }
    }
}
