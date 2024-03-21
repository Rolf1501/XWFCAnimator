using System.Collections.Generic;

namespace XWFC
{
    public class Bidict<TKey, TValue> where TKey : notnull where TValue : notnull
    {
        public readonly Dictionary<TKey, TValue> Dict;
        private readonly Dictionary<TValue, TKey> _inverse;

        public Bidict()
        {
            Dict = new Dictionary<TKey, TValue>();
            _inverse = new Dictionary<TValue, TKey>();
        }

        public void AddPair(TKey key, TValue value)
        {
            Dict.Add(key, value);
            _inverse.Add(value, key);
        }

        public TKey Get(TValue value)
        {
            return _inverse[value];
        }

        public TValue Get(TKey key)
        {
            return Dict[key];
        }

        public int GetNEntries()
        {
            return Dict.Keys.Count;
        }
    }
}