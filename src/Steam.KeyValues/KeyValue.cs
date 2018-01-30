using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace Steam.KeyValues
{
    /// <summary>
    /// Represents a mutable KeyValue object
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class KeyValue : IEnumerable<KeyValue>
    {
        private string _key;
        private object _value;
        private Dictionary<string, KeyValue> _keyedValues;
        private KeyValueType _type;
        private KeyValue _parent;
        private KeyValue _next;
        private KeyValue _previous;
        private KeyValue _first;
        private KeyValue _last;
        private object _treeLock; // lock the whole tree of KeyValues

        /// <summary>
        /// Gets or sets the key of this <see cref="KeyValue"/>
        /// </summary>
        public string Key
        {
            get => _key;
            set
            {
                if (string.IsNullOrEmpty(_key))
                    throw new ArgumentException("Keys cannot be null or empty");

                lock (_treeLock)
                {
                    _parent._keyedValues.Remove(_key);
                    _parent._keyedValues.Add(value, this);
                    _key = value;
                }
            }
        }

        /// <summary>
        /// Gets the type of value in this <see cref="KeyValue"/>
        /// </summary>
        public KeyValueType Type => _type;

        private KeyValue(string key, object value, KeyValueType type, KeyValue parent, KeyValue next, KeyValue previous, object treeLock)
        {
            _key = key;
            _value = value;
            _type = type;
            _parent = parent;
            _next = next;
            _previous = previous;
            _keyedValues = new Dictionary<string, KeyValue>();
            _treeLock = treeLock;
        }

        /// <summary>
        /// Creates a root <see cref="KeyValue"/> with the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static KeyValue Create(string key) => new KeyValue(key, null, KeyValueType.None, null, null, null, new object());

        public static KeyValue FromTextReader(KeyValueTextReader stream) => throw null;

        public static KeyValue FromBinaryReader(KeyValueBinaryReader stream) => throw null;

        public static KeyValue ToTextWriter(KeyValueTextWriter writer) => throw null;

        public static KeyValue ToBinaryWriter(KeyValueBinaryWriter writer) => throw null;
        
        /// <summary>
        /// Gets the root parent of this <see cref="KeyValue"/>
        /// </summary>
        public KeyValue Root
        {
            get
            {
                if (Parent == null)
                    return this;

                var parent = Parent;
                for (; parent.Parent != null; parent = parent.Parent) ;
                return parent;
            }
        }

        /// <summary>
        /// Gets this <see cref="KeyValue"/>'s parent
        /// </summary>
        public KeyValue Parent => _parent;

        /// <summary>
        /// Gets the previous <see cref="KeyValue"/> in relation to the current <see cref="KeyValue"/>
        /// </summary>
        public KeyValue Previous => _previous;

        /// <summary>
        /// Gets the next <see cref="KeyValue"/> in relation to the current <see cref="KeyValue"/>
        /// </summary>
        public KeyValue Next => _next;

        /// <summary>
        /// Creates a new <see cref="KeyValue"/> with the specified key after the current <see cref="KeyValue"/>. If the specified key already exists, this throws an <see cref="ArgumentException"/>
        /// </summary>
        /// <param name="key"></param>
        public KeyValue InsertBefore(string key)
        {
            if (Parent == null)
                throw new InvalidOperationException("Can't insert KeyValue before root element");

            lock (_treeLock)
            {
                if (Parent.Contains(key))
                    throw new ArgumentException("The specified key already exists in this parent's KeyValue's subkeys");

                KeyValue newPrevious = new KeyValue(key, null, KeyValueType.None, _parent, this, _previous, _treeLock);

                if (_previous != null)
                    _previous._next = newPrevious;

                _previous = newPrevious;
                _parent._keyedValues.Add(key, newPrevious);

                return newPrevious;
            }
        }

        /// <summary>
        /// Creates a new <see cref="KeyValue"/> with the specified key after the current <see cref="KeyValue"/>. If the specified key already exists, this throws an <see cref="ArgumentException"/>
        /// </summary>
        /// <param name="key"></param>
        public KeyValue InsertAfter(string key)
        {
            if (Parent == null)
                throw new InvalidOperationException("Can't insert KeyValue after root element");

            lock (_treeLock)
            {
                if (Parent.Contains(key))
                    throw new ArgumentException("The specified key already exists in this parent's KeyValue's subkeys");

                KeyValue newNext = new KeyValue(key, null, KeyValueType.None, _parent, _next, this, _treeLock);

                if (_next != null)
                    _next._previous = newNext;

                _next = newNext;
                _parent._keyedValues.Add(key, newNext);

                return newNext;
            }
        }

        /// <summary>
        /// Gets the <see cref="KeyValue"/> with the specified key or adds it if it does not exist
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValue GetOrAdd(string key) => Contains(key) ? _keyedValues[key] : AddInternal(key);

        /// <summary>
        /// Adds a <see cref="KeyValue"/> with the specified key to this <see cref="KeyValue"/>'s subkeys
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private KeyValue AddInternal(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("A key cannot be null or empty");

            lock (_treeLock)
            {
                if (Contains(key))
                    throw new ArgumentException("The specified key already exists in this KeyValue's subkeys");
            
                var newLast = new KeyValue(key, null, KeyValueType.None, this, null, _last, _treeLock);
                if (Length == 0)
                {
                    SetValue(null, 0);
                    _first = newLast;
                }
                else
                    _last._next = newLast;

                _last = newLast;
                _keyedValues.Add(key, newLast);
                return newLast;
            }
        }

        public KeyValue Get(string key) => Contains(key) ? _keyedValues[key] : throw new KeyNotFoundException();

        public bool TryGet(string key, out KeyValue value)
        {
            if (Contains(key))
            {
                value = _keyedValues[key];
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Adds the specified key and value to the end of this <see cref="KeyValue"/>'s subkeys
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, string value) => GetOrAdd(key).SetString(value);

        /// <summary>
        /// Adds the specified key and value to the end of this <see cref="KeyValue"/>'s subkeys
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddWideString(string key, string value) => GetOrAdd(key).SetWideString(value);

        /// <summary>
        /// Adds the specified key and value to the end of this <see cref="KeyValue"/>'s subkeys
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, int value) => GetOrAdd(key).SetInt32(value);

        /// <summary>
        /// Adds the specified key and value to the end of this <see cref="KeyValue"/>'s subkeys
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, long value) => GetOrAdd(key).SetInt64(value);

        /// <summary>
        /// Adds the specified key and value to the end of this <see cref="KeyValue"/>'s subkeys
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, ulong value) => GetOrAdd(key).SetUInt64(value);

        /// <summary>
        /// Adds the specified key and value to the end of this <see cref="KeyValue"/>'s subkeys
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, Color value) => GetOrAdd(key).SetColor(value);

        /// <summary>
        /// Adds the specified key and value to the end of this <see cref="KeyValue"/>'s subkeys
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, IntPtr value) => GetOrAdd(key).SetIntPtr(value);

        /// <summary>
        /// Adds the specified key and value to the end of this <see cref="KeyValue"/>'s subkeys
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, float value) => GetOrAdd(key).SetFloat(value);

        /// <summary>
        /// Removes the specifed key from this <see cref="KeyValue"/>'s subkeys
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            lock (_treeLock)
            {
                if (!Contains(key))
                    return;
            
                KeyValue removed = _keyedValues[key];
                if (removed._next != null)
                {
                    removed._next._previous = removed._previous;
                }

                if (removed._previous != null)
                {
                    removed._previous._next = removed._next;
                }

                _keyedValues.Remove(key);
            }
        }
        
        /// <summary>
        /// Gets the number of subkeys in this <see cref="KeyValue"/>
        /// </summary>
        public int Length => _keyedValues.Count;

        /// <summary>
        /// Gets the subkeys in this <see cref="KeyValue"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValue> GetValues()
        {
            if (Length == 0)
                yield break;

            for (var current = _first; current != null; current = current._next)
                yield return current;
        }

        public bool Contains(string key) => _keyedValues.ContainsKey(key);

        #region Value accessors
        
        // todo: make getters more like ImmutableKeyValue's getters

        /// <summary>
        /// Tries to get the value of this <see cref="KeyValue"/> as the specified type
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetString(out string value) => TryGetValue(out value);
        
        public string GetString() => GetValue<string>();

        public void SetString(string value) => SetValue(value, KeyValueType.String);
        
        public void SetWideString(string value) => SetValue(value, KeyValueType.WideString);

        /// <summary>
        /// Tries to get the value of this <see cref="KeyValue"/> as the specified type
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetInt32(out int value) => TryGetValue(out value);

        public int GetInt32() => GetValue<int>();

        public void SetInt32(int value) => SetValue(value, KeyValueType.Int32);

        /// <summary>
        /// Tries to get the value of this <see cref="KeyValue"/> as the specified type
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetInt64(out long value) => TryGetValue(out value);

        public long GetInt64() => GetValue<long>();

        public void SetInt64(long value) => SetValue(value, KeyValueType.Int64);

        /// <summary>
        /// Tries to get the value of this <see cref="KeyValue"/> as the specified type
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetFloat(out float value) => TryGetValue(out value);

        public float GetFloat() => GetValue<float>();

        public void SetFloat(float value) => SetValue(value, KeyValueType.Float);

        /// <summary>
        /// Tries to get the value of this <see cref="KeyValue"/> as the specified type
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetUInt64(out ulong value) => TryGetValue(out value);
        
        public ulong GetUInt64() => GetValue<ulong>();
        
        public void SetUInt64(ulong value) => SetValue(value, KeyValueType.UInt64);
        
        /// <summary>
        /// Tries to get the value of this <see cref="KeyValue"/> as the specified type
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetIntPtr(out IntPtr value) => TryGetValue(out value);

        public IntPtr GetIntPtr() => GetValue<IntPtr>();
        
        public void SetIntPtr(IntPtr value) => SetValue(value, KeyValueType.Pointer);

        /// <summary>
        /// Tries to get the value of this <see cref="KeyValue"/> as the specified type
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetColor(out Color value) => TryGetValue(out value);

        public Color GetColor() => GetValue<Color>();

        public void SetColor(Color value) => SetValue(value, KeyValueType.Color);

        public object GetValue(bool escapeString)
        {
            if (_value is string stringValue && escapeString)
            {
                return stringValue.Replace("\\\\", "\\").Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t");
            }
            else
                return _value;
        }

        private void SetValue(object value, KeyValueType type)
        {
            _value = value;
            _type = type;

            if (type != 0 && Length != 0)
                _keyedValues.Clear();
        }

        private T GetValue<T>() => TryGetValue<T>(out var value) ? value : throw new InvalidCastException();

        private bool TryGetValue<T>(out T value)
        {
            switch(_type)
            {
                case KeyValueType.None:
                    value = default;
                    return false;
                case KeyValueType.UInt64 when typeof(T) == typeof(decimal) && _value is ulong ulongValue:
                    decimal decimalValue = ulongValue;
                    value = (T)(object)decimalValue;
                    return true;
                case KeyValueType.UInt64 when typeof(T) == typeof(decimal) && _value is decimal:
                default:
                    value = (T)_value;
                    return true;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"Key = \"{Key}\", {(Type == 0 ? $"Length = {Length}" : $"Value = \"{GetValue(true)}\" ({Type})")}";
        
        public IEnumerator<KeyValue> GetEnumerator() => GetValues().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetValues().GetEnumerator();

        #endregion

        public KeyValue this[string key] => _keyedValues[key];

        [DebuggerDisplay("Expanding the results view will enumerate the KeyValue", Name = "Results View", Type = "")]
        private KeyValueResultsView ResultsView => new KeyValueResultsView(this);
    }
}
