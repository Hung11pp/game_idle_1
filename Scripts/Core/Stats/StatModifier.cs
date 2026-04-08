using System;

namespace IdleDefense.Core.Stats
{
    /// <summary>
    /// One contribution to a stat. Items store lists of modifiers; <see cref="StatAggregator"/> folds them into a <see cref="StatBlock"/>.
    /// </summary>
    [Serializable]
    public struct StatModifier
    {
        public StatId Stat;
        public StatModifierKind Kind;
        public float Value;

        public StatModifier(StatId stat, StatModifierKind kind, float value)
        {
            Stat = stat;
            Kind = kind;
            Value = value;
        }

        public static StatModifier Flat(StatId stat, float value)
        {
            return new StatModifier(stat, StatModifierKind.Flat, value);
        }
    }
}
