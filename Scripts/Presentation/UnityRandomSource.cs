using IdleDefense.Core.Random;
using UnityEngine;

namespace IdleDefense.Presentation
{
    /// <summary>
    /// Bridges UnityEngine.Random to Core's IRandomSource.
    /// </summary>
    public sealed class UnityRandomSource : IRandomSource
    {
        public float NextFloat()
        {
            return Random.value;
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            return Random.Range(minInclusive, maxExclusive);
        }
    }
}
