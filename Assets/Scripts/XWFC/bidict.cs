using System.Collections.Generic;
using System.Linq;

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
            if (Dict.Keys.Contains(key))
            {
                Dict[key] = value;
            }
            else
            {
                Dict.Add(key, value);
            }

            if (_inverse.Keys.Contains(value))
            {
                _inverse[value] = key;
            }
            else
            {
                _inverse.Add(value, key);
            }
        }

        public TKey Get(TValue value)
        {
            return _inverse[value];
        }
        public TKey GetKey(TValue value)
        {
            return _inverse[value];
        }
        
        public TValue GetValue(TKey key)
        {
            return Dict[key];
        }

        public TValue Get(TKey key)
        {
            return Dict[key];
        }

        public bool ContainsValue(TValue value)
        {
            return _inverse.Keys.Contains(value);
        }

        public int GetNEntries()
        {
            return Dict.Keys.Count;
        }

        public List<TValue> GetValues()
        {
            return _inverse.Keys.ToList();
        }
    }
}