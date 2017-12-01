using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Steam.KeyValues
{
    /// <summary>
    /// Represents a mutable KeyValue object
    /// </summary>
    public sealed class KeyValue : IList<KeyValue>, IDictionary<string, KeyValue>
    {
        private string _key;
        private object _value;
        private List<KeyValue> _children = new List<KeyValue>();
        private KeyValueType _type;

        /// <summary>
        /// Gets the key of this <see cref="KeyValue"/>
        /// </summary>
        public string Key => _key;

        /// <summary>
        /// Gets the type of value in this 
        /// </summary>
        public KeyValueType Type => _type;

        public KeyValue(string key, object value, KeyValueType type)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));

            switch(value)
            {
                case int _ when type == KeyValueType.Int32:
                case ulong _ when type == KeyValueType.UInt64:
                case long _ when type == KeyValueType.Int64:
                case IntPtr _ when type == KeyValueType.Pointer:
                case string _ when type == KeyValueType.String || type == KeyValueType.WideString:
                case float _ when type == KeyValueType.Float:
                case Color _ when type == KeyValueType.Color:
                case null when type == KeyValueType.None:
                    _value = value;
                    break;
                case decimal decimalValue when type == KeyValueType.UInt64:
                    _value = decimal.ToUInt64(decimalValue);
                    break;
                default:
                    throw new ArgumentException("The specified value is not a valid type or does not match the type specified by 'type'");
            }

            if (type < 0 || type > KeyValueType.Int64)
                throw new ArgumentOutOfRangeException(nameof(type));

            _type = type;
        }

        public KeyValue(string key, string value) : this(key, value, KeyValueType.String) { }

        public KeyValue(string key, int value) : this(key, value, KeyValueType.Int32) { }

        public KeyValue(string key, float value) : this(key, value, KeyValueType.Float) { }

        public KeyValue(string key, IntPtr value) : this(key, value, KeyValueType.Pointer) { }

        public KeyValue(string key, Color value) : this(key, value, KeyValueType.Color) { }

        [CLSCompliant(false)]
        public KeyValue(string key, ulong value) : this(key, value, KeyValueType.UInt64) { }

        public KeyValue(string key, long value) : this(key, value, KeyValueType.Int64) { }

        public KeyValue(string key, string value, bool wideString) : this(key, value, KeyValueType.WideString) { }

        public KeyValue(string key) : this(key, null, KeyValueType.None) { }

        public ICollection<string> Keys => throw new NotImplementedException();

        public ICollection<KeyValue> Values => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public KeyValue this[int index]
        {
            get => ((IList<KeyValue>)_children)[index];
            set => ((IList<KeyValue>)_children)[index] = value;
        }

        public KeyValue this[string key]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public static KeyValue Parse(byte[] data) => ImmutableKeyValue.Parse(data).ToKeyValue();

        [CLSCompliant(false)]
        public static KeyValue Parse(ReadOnlySpan<byte> utf8KeyValue) => ImmutableKeyValue.Parse(utf8KeyValue).ToKeyValue();
        
        public static KeyValue ParseBinary(byte[] data) => ImmutableKeyValue.ParseBinary(data).ToKeyValue();

        [CLSCompliant(false)]
        public static KeyValue ParseBinary(ReadOnlySpan<byte> binaryKeyValue) => ImmutableKeyValue.ParseBinary(binaryKeyValue).ToKeyValue();

        /// <summary>
        /// Creates a new <see cref="ImmutableKeyValue"/> from this <see cref="KeyValue"/>
        /// </summary>
        /// <returns></returns>
        public ImmutableKeyValue ToImmutableKeyValue() => ToImmutableKeyValue(null);

        /// <summary>
        /// Creates a new <see cref="ImmutableKeyValue"/> from this <see cref="KeyValue"/> using an optional <see cref="MemoryPool<byte>"/>
        /// </summary>
        /// <returns></returns>
        [CLSCompliant(false)]
        public ImmutableKeyValue ToImmutableKeyValue(MemoryPool<byte> pool = null)
        {
            /* 
             * to make an ImmutableKeyValue from a KeyValue,
             * all we need to do is make a span of memory
             * containing our keys and values PLUS our database
             * with the list of positions where the keys and values are.l;
             * ezpz
             */

            throw new NotImplementedException();
        }
        
        private int CalculateValueSpace() => Encoding.UTF8.GetByteCount(_key) + Type != KeyValueType.None ? CalculateValueSize() : _children.Sum(kv => kv.CalculateValueSpace());

        private int CalculateValueSize() => throw new NotImplementedException();
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValue>)_children).GetEnumerator();
        }

        public IEnumerator<KeyValue> GetEnumerator()
        {
            return ((IEnumerable<KeyValue>)_children).GetEnumerator();
        }

        /// <summary>
        /// Adds the specified <see cref="KeyValue"/> under the specified key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, KeyValue value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets whether any values 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out KeyValue value)
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<string, KeyValue> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, KeyValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, KeyValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, KeyValue> item)
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<string, KeyValue>> IEnumerable<KeyValuePair<string, KeyValue>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(KeyValue item)
        {
            return ((IList<KeyValue>)_children).IndexOf(item);
        }

        public void Insert(int index, KeyValue item)
        {
            ((IList<KeyValue>)_children).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<KeyValue>)_children).RemoveAt(index);
        }

        public void Add(KeyValue item)
        {
            ((IList<KeyValue>)_children).Add(item);
        }

        public bool Contains(KeyValue item)
        {
            return ((IList<KeyValue>)_children).Contains(item);
        }

        public void CopyTo(KeyValue[] array, int arrayIndex)
        {
            ((IList<KeyValue>)_children).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValue item)
        {
            return ((IList<KeyValue>)_children).Remove(item);
        }
    }
}
