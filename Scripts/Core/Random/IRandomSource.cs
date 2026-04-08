namespace IdleDefense.Core.Random
{
    /// <summary>
    /// Abstraction over System.Random / UnityEngine.Random for core logic tests.
    /// </summary>
    public interface IRandomSource
    {
        /// <summary>Returns value in [0, 1).</summary>
        float NextFloat();

        int Range(int minInclusive, int maxExclusive);
    }
}
