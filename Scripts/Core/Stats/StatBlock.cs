using System.Collections.Generic;

namespace IdleDefense.Core.Stats
{
    /// <summary>
    /// Key-value stat container. Merge by adding values per key.
    /// </summary>
    public sealed class StatBlock
    {
        private readonly Dictionary<StatId, float> _values = new Dictionary<StatId, float>();

        /// <summary>All stored stats (including zeros if explicitly set).</summary>
        public IEnumerable<KeyValuePair<StatId, float>> EnumerateAll()
        {
            return _values;
        }

        /// <summary>Deterministic order for UI (by enum value).</summary>
        public IEnumerable<KeyValuePair<StatId, float>> EnumerateAllSorted()
        {
            List<StatId> keys = new List<StatId>(_values.Keys);
            keys.Sort((a, b) => ((int)a).CompareTo((int)b));
            for (int i = 0; i < keys.Count; i++)
            {
                StatId id = keys[i];
                yield return new KeyValuePair<StatId, float>(id, _values[id]);
            }
        }

        public float Get(StatId id)
        {
            return _values.TryGetValue(id, out float v) ? v : 0f;
        }

        public void Set(StatId id, float value)
        {
            _values[id] = value;
        }

        public void Add(StatId id, float delta)
        {
            _values[id] = Get(id) + delta;
        }

        public void AddFrom(StatBlock other)
        {
            if (other == null)
            {
                return;
            }

            foreach (KeyValuePair<StatId, float> pair in other._values)
            {
                Add(pair.Key, pair.Value);
            }
        }

        public StatBlock Clone()
        {
            StatBlock copy = new StatBlock();
            foreach (KeyValuePair<StatId, float> pair in _values)
            {
                copy._values[pair.Key] = pair.Value;
            }

            return copy;
        }
    }
}
