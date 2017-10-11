using Steam.KeyValues.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Steam.KeyValues.Linq
{
    internal class KVPropertyKeyedCollection : Collection<KVToken>
    {
        private static readonly IEqualityComparer<string> Comparer = StringComparer.Ordinal;

        private Dictionary<string, KVToken> _dictionary;

        public KVPropertyKeyedCollection() : base(new List<KVToken>())
        {
        }

        private void AddKey(string key, KVToken item)
        {
            EnsureDictionary();
            _dictionary[key] = item;
        }

        protected void ChangeItemKey(KVToken item, string newKey)
        {
            if (!ContainsItem(item))
            {
                throw new ArgumentException("The specified item does not exist in this KeyedCollection.");
            }

            string keyForItem = GetKeyForItem(item);
            if (!Comparer.Equals(keyForItem, newKey))
            {
                if (newKey != null)
                {
                    AddKey(newKey, item);
                }

                if (keyForItem != null)
                {
                    RemoveKey(keyForItem);
                }
            }
        }

        protected override void ClearItems()
        {
            base.ClearItems();

            _dictionary?.Clear();
        }

        public bool Contains(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_dictionary != null)
            {
                return _dictionary.ContainsKey(key);
            }

            return false;
        }

        private bool ContainsItem(KVToken item)
        {
            if (_dictionary == null)
            {
                return false;
            }

            string key = GetKeyForItem(item);
            return _dictionary.TryGetValue(key, out KVToken value);
        }

        private void EnsureDictionary()
        {
            if (_dictionary == null)
            {
                _dictionary = new Dictionary<string, KVToken>(Comparer);
            }
        }

        private string GetKeyForItem(KVToken item)
        {
            return ((KVProperty)item).Name;
        }

        protected override void InsertItem(int index, KVToken item)
        {
            AddKey(GetKeyForItem(item), item);
            base.InsertItem(index, item);
        }

        public bool Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_dictionary != null)
            {
                return _dictionary.ContainsKey(key) && Remove(_dictionary[key]);
            }

            return false;
        }

        protected override void RemoveItem(int index)
        {
            string keyForItem = GetKeyForItem(Items[index]);
            RemoveKey(keyForItem);
            base.RemoveItem(index);
        }

        private void RemoveKey(string key)
        {
            _dictionary?.Remove(key);
        }

        protected override void SetItem(int index, KVToken item)
        {
            string keyForItem = GetKeyForItem(item);
            string keyAtIndex = GetKeyForItem(Items[index]);

            if (Comparer.Equals(keyAtIndex, keyForItem))
            {
                if (_dictionary != null)
                {
                    _dictionary[keyForItem] = item;
                }
            }
            else
            {
                AddKey(keyForItem, item);

                if (keyAtIndex != null)
                {
                    RemoveKey(keyAtIndex);
                }
            }
            base.SetItem(index, item);
        }

        public KVToken this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (_dictionary != null)
                {
                    return _dictionary[key];
                }

                throw new KeyNotFoundException();
            }
        }

        public bool TryGetValue(string key, out KVToken value)
        {
            if (_dictionary == null)
            {
                value = null;
                return false;
            }

            return _dictionary.TryGetValue(key, out value);
        }

        public ICollection<string> Keys
        {
            get
            {
                EnsureDictionary();
                return _dictionary.Keys;
            }
        }

        public ICollection<KVToken> Values
        {
            get
            {
                EnsureDictionary();
                return _dictionary.Values;
            }
        }

        public int IndexOfReference(KVToken t)
        {
            return ((List<KVToken>)Items).IndexOfReference(t);
        }

        public bool Compare(KVPropertyKeyedCollection other)
        {
            if (this == other)
            {
                return true;
            }

            // dictionaries in JavaScript aren't ordered
            // ignore order when comparing properties
            Dictionary<string, KVToken> d1 = _dictionary;
            Dictionary<string, KVToken> d2 = other._dictionary;

            if (d1 == null && d2 == null)
            {
                return true;
            }

            if (d1 == null)
            {
                return (d2.Count == 0);
            }

            if (d2 == null)
            {
                return (d1.Count == 0);
            }

            if (d1.Count != d2.Count)
            {
                return false;
            }

            foreach (KeyValuePair<string, KVToken> keyAndProperty in d1)
            {
                if (!d2.TryGetValue(keyAndProperty.Key, out KVToken secondValue))
                {
                    return false;
                }

                KVProperty p1 = (KVProperty)keyAndProperty.Value;
                KVProperty p2 = (KVProperty)secondValue;

                if (p1.Value == null)
                {
                    return (p2.Value == null);
                }

                if (!p1.Value.DeepEquals(p2.Value))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
