namespace IdleDefense.Core.Stats
{
    /// <summary>
    /// How a modifier combines into a <see cref="StatBlock"/>. Extend as mechanics grow (e.g. percent of base).
    /// </summary>
    public enum StatModifierKind
    {
        /// <summary>Additive flat bonus (default for loot).</summary>
        Flat = 0
    }
}
