namespace IdleDefense.GameServices.Systems.Loot
{
    /// <summary>Runtime copy of one rarity row from a loot table (weights + scaling).</summary>
    public sealed class LootRarityTierDefinition
    {
        public string TierId = "common";
        public string DisplayName = "Common";

        /// <summary>Relative chance in weighted roll (need not sum to 100).</summary>
        public float DropWeight = 1f;

        public float StatMultiplier = 1f;
    }
}
