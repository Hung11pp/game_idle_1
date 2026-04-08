using System.Collections.Generic;

namespace IdleDefense.Core.Stats
{
    /// <summary>
    /// Folds item modifiers into a <see cref="StatBlock"/>. Central place to add percent-of-base, caps, etc.
    /// </summary>
    public static class StatAggregator
    {
        public static StatBlock ToStatBlock(IReadOnlyList<StatModifier> modifiers)
        {
            StatBlock block = new StatBlock();
            if (modifiers == null || modifiers.Count == 0)
            {
                return block;
            }

            for (int i = 0; i < modifiers.Count; i++)
            {
                StatModifier m = modifiers[i];
                if (m.Kind == StatModifierKind.Flat)
                {
                    block.Add(m.Stat, m.Value);
                }
            }

            return block;
        }
    }
}
