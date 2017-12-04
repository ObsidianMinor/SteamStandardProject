using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace Steam.KeyValues
{
    /// <summary>
    /// Represents a mutable KeyValue object
    /// </summary>
    [DebuggerTypeProxy(typeof(TypeProxy))]
    public sealed class KeyValue
    {
        /// <summary>
        /// Gets the key of this <see cref="KeyValue"/>
        /// </summary>
        public string Key => throw null;

        /// <summary>
        /// Gets the type of value in this KeyValue
        /// </summary>
        public KeyValueType Type => throw null;

        public KeyValue(string key, object value, KeyValueType type) => throw null;

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

        public KeyValue(string key, IEnumerable<KeyValue> values)
        {

        }
        
        /// <summary>
        /// Gets the root parent of this <see cref="KeyValue"/>
        /// </summary>
        public KeyValue Root { get => throw null; }

        /// <summary>
        /// Gets this <see cref="KeyValue"/>'s parent
        /// </summary>
        public KeyValue Parent { get => throw null; }

        /// <summary>
        /// Gets the previous <see cref="KeyValue"/> in relation to the current <see cref="KeyValue"/>
        /// </summary>
        public KeyValue Previous => throw null;

        /// <summary>
        /// Gets the next <see cref="KeyValue"/> in relation to the current <see cref="KeyValue"/>
        /// </summary>
        public KeyValue Next => throw null;

        /// <summary>
        /// Inserts the specified <see cref="KeyValue"/> before the current <see cref="KeyValue"/>
        /// </summary>
        /// <param name="kv"></param>
        public void InsertBefore(KeyValue kv) => throw null;

        /// <summary>
        /// Inserts the specified <see cref="KeyValue"/> after the current <see cref="KeyValue"/>
        /// </summary>
        /// <param name="kv"></param>
        public void InsertAfter(KeyValue kv) => throw null;

        /// <summary>
        /// Adds the specified <see cref="KeyValue"/> to the end of this <see cref="KeyValue"/>'s subkeys
        /// </summary>
        /// <param name="kv"></param>
        public void Add(KeyValue kv) => throw null;

        /// <summary>
        /// Removes the specifed <see cref="KeyValue"/> from this <see cref="KeyValue"/>'s subkeys
        /// </summary>
        /// <param name="kv"></param>
        public void Remove(KeyValue kv) => throw null;

        public KeyValue this[string key]
        {
            get => throw null;
            set => throw null;
        }

        private class TypeProxy
        {
            public TypeProxy(KeyValue value)
            {

            }
        }
    }
}
