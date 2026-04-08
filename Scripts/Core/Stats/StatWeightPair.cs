using System;

namespace IdleDefense.Core.Stats
{
    /// <summary>Inspector-friendly stat → weight for equip scoring.</summary>
    [Serializable]
    public struct StatWeightPair
    {
        public StatId Stat;
        public float Weight;
    }
}
