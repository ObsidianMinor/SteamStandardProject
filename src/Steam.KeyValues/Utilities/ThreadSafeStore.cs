using System;
using System.Collections.Generic;

namespace Steam.KeyValues.Utilities
{
    internal class ThreadSafeStore<TKey, TValue>
    {
        private readonly object _lock = new object();
        private Dictionary<TKey, TValue> _store;
        private readonly Func<TKey, TValue> _creator;

        public ThreadSafeStore(Func<TKey, TValue> creator)
        {
            _creator = creator ?? throw new ArgumentNullException(nameof(creator));
            _store = new Dictionary<TKey, TValue>();
        }

        public TValue Get(TKey key)
        {
            if (!_store.TryGetValue(key, out TValue value))
            {
                return AddValue(key);
            }

            return value;
        }

        private TValue AddValue(TKey key)
        {
            TValue value = _creator(key);

            lock (_lock)
            {
                if (_store == null)
                {
                    _store = new Dictionary<TKey, TValue>
                    {
                        [key] = value
                    };
                }
                else
                {
                    // double check locking
                    if (_store.TryGetValue(key, out TValue checkValue))
                    {
                        return checkValue;
                    }

                    Dictionary<TKey, TValue> newStore = new Dictionary<TKey, TValue>(_store)
                    {
                        [key] = value
                    };
                    _store = newStore;
                }

                return value;
            }
        }
    }
}
