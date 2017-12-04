using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Utf8;

using static System.Buffers.Binary.BinaryPrimitives;

namespace Steam.KeyValues
{
    /// <summary>
    /// Represents a KeyValue structure that is immutable, stack-only, and uses zero allocations
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    // [DebuggerTypeProxy(typeof(DebugView))] one day
    public readonly ref struct ImmutableKeyValue
    {
        private readonly MemoryPool<byte> _pool;
        private readonly OwnedMemory<byte> _dbMemory;
        private readonly ReadOnlySpan<byte> _db;
        private readonly ReadOnlySpan<byte> _values;
        private readonly bool _binarySpan;
        
        internal ImmutableKeyValue(ReadOnlySpan<byte> values, ReadOnlySpan<byte> db, bool binary, MemoryPool<byte> pool = null, OwnedMemory<byte> dbMemory = null)
        {
            _values = values;
            _db = db;
            _pool = pool;
            _dbMemory = dbMemory;
            _binarySpan = binary;
        }
        
        internal DbRow Record => ReadMachineEndian<DbRow>(_db);

        /// <summary>
        /// Get key of this <see cref="ImmutableKeyValue"/> as a <see cref="string"/>
        /// </summary>
        public string Key => Utf8Key.ToString();

        /// <summary>
        /// Gets the key of this <see cref="ImmutableKeyValue"/> as a <see cref="Utf8Span"/>
        /// </summary>
        [CLSCompliant(false)]
        public Utf8Span Utf8Key
        {
            get
            {
                var record = Record;
                return new Utf8Span(_values.Slice(record.KeyLocation, record.KeyLength));
            }
        }

        /// <summary>
        /// Gets the type of value this <see cref="ImmutableKeyValue"/> contains
        /// </summary>
        public KeyValueType Type => ReadMachineEndian<KeyValueType>(_db.Slice(16)); // 16 is the offset of the type

        /// <summary>
        /// Parses the specified byte array as a text stream
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ImmutableKeyValue Parse(byte[] data) => Parse(new ReadOnlySpan<byte>(data));

        /// <summary>
        /// Parses the specified byte array as a text stream
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pool"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public static ImmutableKeyValue Parse(byte[] data, MemoryPool<byte> pool = null) => Parse(new ReadOnlySpan<byte>(data), pool);
        
        /// <summary>
        /// Parses the specified <see cref="ReadOnlySpan{T}"/> as a text stream
        /// </summary>
        /// <param name="utf8KeyValue"></param>
        /// <param name="pool"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public static ImmutableKeyValue Parse(ReadOnlySpan<byte> utf8KeyValue, MemoryPool<byte> pool = null)
        {
            return new KeyValueTextParser().Parse(utf8KeyValue, pool);
        }

        /// <summary>
        /// Loads a UTF8 file at the specified path and parses it as a text stream
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static ImmutableKeyValue FromFile(string file) => FromFile(file, null);

        /// <summary>
        /// Loads a UTF8 file at the specified path and parses it as a text stream
        /// </summary>
        /// <param name="file"></param>
        /// <param name="pool"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public static ImmutableKeyValue FromFile(string file, MemoryPool<byte> pool = null)
        {
            var bytes = File.ReadAllBytes(file);
            if (bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return Parse(new Span<byte>(bytes).Slice(3), pool);
            else
                return Parse(bytes, pool);
        }

        /// <summary>
        /// Parses the specified byte array as a binary stream
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ImmutableKeyValue ParseBinary(byte[] data) => Parse(new ReadOnlySpan<byte>(data));

        /// <summary>
        /// Parses the specified byte array as a binary stream
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pool"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public static ImmutableKeyValue ParseBinary(byte[] data, MemoryPool<byte> pool = null) => ParseBinary(new ReadOnlySpan<byte>(data), pool);

        /// <summary>
        /// Parses the specified <see cref="ReadOnlySpan{T}"/> as a binary stream
        /// </summary>
        /// <param name="binaryKeyValue"></param>
        /// <param name="pool"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public static ImmutableKeyValue ParseBinary(ReadOnlySpan<byte> binaryKeyValue, MemoryPool<byte> pool = null)
        {
            return new KeyValueBinaryParser().Parse(binaryKeyValue, pool);
        }

        /// <summary>
        /// Loads a file and parses it in binary format
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static ImmutableKeyValue FromBinaryFile(string file) => FromBinaryFile(file, null);

        /// <summary>
        /// Loads a file and parses it in binary format
        /// </summary>
        /// <param name="file"></param>
        /// <param name="pool"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public static ImmutableKeyValue FromBinaryFile(string file, MemoryPool<byte> pool = null) => ParseBinary(File.ReadAllBytes(file), pool);
        
        /// <summary>
        /// Gets the child <see cref="ImmutableKeyValue"/> with the specified key
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public ImmutableKeyValue this[Utf8Span name] => TryGetValue(name, out var value) ? value : throw new KeyNotFoundException();
        
        /// <summary>
        /// Gets the child <see cref="ImmutableKeyValue"/> with the specified key
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ImmutableKeyValue this[string name] => TryGetValue(name, out var value) ? value : throw new KeyNotFoundException();

        /// <summary>
        /// Gets the child <see cref="ImmutableKeyValue"/> at the specified index
        /// </summary>
        /// <param name="index">The index to get the child</param>
        /// <returns>The <see cref="ImmutableKeyValue"/> at the specified index</returns>
        /// <remarks>This method performs a linear search; therefore, this method is an O(n) operation, where n is Length</remarks>
        public ImmutableKeyValue this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                int pos = 0;
                Enumerator enumerator = GetEnumerator();
                while (enumerator.MoveNext() && pos < index)
                {
                    pos++;
                }
                return enumerator.Current;
            }
        }

        /// <summary>
        /// Gets the number of children in this <see cref="ImmutableKeyValue"/>
        /// </summary>
        public int Length
        {
            get
            {
                int length = 0;
                Enumerator enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    length++;
                }
                return length;
            }
        }

        /// <summary>
        /// Converts this <see cref="ImmutableKeyValue"/> to a mutable KeyValue object
        /// </summary>
        /// <returns></returns>
        public KeyValue ToKeyValue()
        {
            if (Type == 0)
            {
                List<KeyValue> subKeys = new List<KeyValue>();
                foreach (ImmutableKeyValue kv in this)
                    subKeys.Add(kv.ToKeyValue());

                return new KeyValue(Key, subKeys);
            }
            else
            {
                return new KeyValue(Key, GetValue(), Type);
            }
        }

        /// <summary>
        /// Gets the value of this <see cref="ImmutableKeyValue"/> in the type specified by <see cref="Type"/>
        /// </summary>
        /// <returns></returns>
        public object GetValue()
        {
            switch (Type)
            {
                case KeyValueType.WideString:
                case KeyValueType.String:
                    return GetString();
                case KeyValueType.Int32:
                    return GetInt32();
                case KeyValueType.Float:
                    return GetFloat();
                case KeyValueType.Pointer:
                    return GetIntPtr();
                case KeyValueType.Color:
                    return GetColor();
                case KeyValueType.UInt64:
                    return GetUInt64();
                case KeyValueType.Int64:
                    return GetInt64();
                case KeyValueType.None:
                default:
                    throw new InvalidOperationException("Invalid data type");
            }
        }

        /// <summary>
        /// Tries to get the <see cref="ImmutableKeyValue"/> with the specified key
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public bool TryGetValue(Utf8Span propertyName, out ImmutableKeyValue value)
        {
            var record = Record;
            
            if (record.Type != KeyValueType.None)
                throw new InvalidOperationException();

            foreach(ImmutableKeyValue keyValue in this)
            {
                if (keyValue.Utf8Key == propertyName)
                {
                    value = keyValue;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Tries to get the <see cref="ImmutableKeyValue"/> with the specified key
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string propertyName, out ImmutableKeyValue value)
        {
            var record = Record;

            if (record.Length == 0)
                throw new KeyNotFoundException();

            if (record.Type != KeyValueType.None)
                throw new InvalidOperationException();

            foreach (ImmutableKeyValue keyValue in this)
            {
                if (keyValue.Utf8Key == propertyName)
                {
                    value = keyValue;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Gets the value of this <see cref="ImmutableKeyValue"/> as a 32 bit integer
        /// </summary>
        /// <returns>The value of this <see cref="ImmutableKeyValue"/></returns>
        public int GetInt32()
        {
            if (TryGetInt32(out var value))
                return value;
            else
                throw new InvalidCastException();
        }
        
        /// <summary>
        /// Tries to get the value of this <see cref="ImmutableKeyValue"/> as a 32 bit integer
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetInt32(out int value)
        {
            value = default;

            var record = Record;
            if (!record.IsSimpleValue)
                return false;

            switch(record.Type)
            {
                case KeyValueType.String:
                case KeyValueType.WideString:
                    return Utf8Parser.TryParse(_values.Slice(record.Location, record.Length), out value, out var _);
                case KeyValueType.UInt64:
                case KeyValueType.Int64:
                    return false;
                case KeyValueType.Pointer:
                case KeyValueType.Int32:
                case KeyValueType.Float:
                default:
                    value = ReadMachineEndian<int>(_values.Slice(record.Location, record.Length));
                    return true;
            }
        }

        [CLSCompliant(false)]
        public ulong GetUInt64()
        {
            if (TryGetUInt64(out var value))
                return value;
            else
                throw new InvalidCastException();
        }

        [CLSCompliant(false)]
        public bool TryGetUInt64(out ulong value)
        {
            value = default;

            var record = Record;
            if (!record.IsSimpleValue)
                return false;

            switch(record.Type)
            {
                case KeyValueType.String:
                case KeyValueType.WideString:
                    return Utf8Parser.TryParse(_values.Slice(record.Location, record.Length), out value, out var _);
                case KeyValueType.Float:
                case KeyValueType.Int32:
                case KeyValueType.Pointer:
                default:
                    value = ReadMachineEndian<uint>(_values.Slice(record.Location, record.Length));
                    return true;
                case KeyValueType.Int64:
                case KeyValueType.UInt64:
                    value = ReadMachineEndian<ulong>(_values.Slice(record.Location, record.Length));
                    return true;
            }
        }
        
        public long GetInt64()
        {
            if (TryGetInt64(out var value))
                return value;
            else
                throw new InvalidCastException();
        }

        public bool TryGetInt64(out long value) => throw new NotImplementedException();

        /// <summary>
        /// Gets the value of this <see cref="ImmutableKeyValue"/> as a UInt64
        /// </summary>
        /// <returns>A decimal container for an UInt64</returns>
        public decimal GetDecimal() => GetUInt64();

        public bool TryGetDecimal(out decimal value)
        {
            bool success = TryGetUInt64(out var ulongValue);
            value = ulongValue;
            return success;
        }

        public IntPtr GetIntPtr()
        {
            if (TryGetIntPtr(out var value))
                return value;
            else
                throw new InvalidCastException();
        }

        public bool TryGetIntPtr(out IntPtr value) => throw new NotImplementedException();

        public float GetFloat()
        {
            if (TryGetFloat(out var value))
                return value;
            else
                throw new InvalidCastException();
        }

        public bool TryGetFloat(out float value) => throw new NotImplementedException();

        [CLSCompliant(false)]
        public Utf8String GetUtf8String()
        {
            if (TryGetUtf8String(out var value))
                return value;
            else
                throw new InvalidCastException();
        }

        [CLSCompliant(false)]
        public bool TryGetUtf8String(out Utf8String value)
        {
            value = default;

            var record = Record;
            if (!record.IsSimpleValue)
                return false;

            value = new Utf8String(_values.Slice(record.Location, record.Length));
            return true;
        }

        public string GetString()
        {
            if (TryGetString(out var value))
                return value;
            else
                throw new InvalidCastException();
        }

        public bool TryGetString(out string value)
        {
            value = default;

            if (!TryGetUtf8String(out var utf8))
                return false;
            else
            {
                value = utf8.ToString();
                return true;
            }
        }

        public bool GetBool()
        {
            if (TryGetBool(out var value))
                return value;
            else
                throw new InvalidCastException();
        }

        public bool TryGetBool(out bool value)
        {
            value = default;

            if (!TryGetInt32(out int val))
                return false;
            else
            {
                value = val != 0;
                return true;
            }
        }

        public Color GetColor()
        {
            if (TryGetColor(out var value))
                return value;
            else
                throw new InvalidCastException();
        }

        public bool TryGetColor(out Color value)
        {
            value = default;

            var record = Record;
            if (!record.IsSimpleValue)
                throw new InvalidCastException();

            var slice = _values.Slice(record.Location);

            switch(Type)
            {
                case KeyValueType.Color:
                case KeyValueType.Int32: // um
                case KeyValueType.Float: // ok valve...
                    if (slice.Length < 4)
                        return false;

                    byte r = slice[0];
                    byte g = slice[1];
                    byte b = slice[2];
                    byte a = slice[3];

                    value = Color.FromArgb(a, r, g, b);
                    return true;
                case KeyValueType.String:
                    // todo: implement color parsing
                    throw new NotImplementedException();
            }

            return false;
        }
        
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"Key = \"{Key}\", {(Type == 0 ? $"Length = {Length}" : $"Value = \"{GetString()}\" ({Type})")}";

        public static explicit operator Color(ImmutableKeyValue kv) => kv.GetColor();
        public static explicit operator bool(ImmutableKeyValue kv) => kv.GetBool();
        [CLSCompliant(false)]
        public static explicit operator Utf8String(ImmutableKeyValue kv) => kv.GetUtf8String();
        public static explicit operator string(ImmutableKeyValue kv) => kv.GetString();
        public static explicit operator float(ImmutableKeyValue kv) => kv.GetFloat();
        public static explicit operator long(ImmutableKeyValue kv) => kv.GetInt64();
        [CLSCompliant(false)]
        public static explicit operator ulong(ImmutableKeyValue kv) => kv.GetUInt64();
        public static explicit operator int(ImmutableKeyValue kv) => kv.GetInt32();
        public static explicit operator IntPtr(ImmutableKeyValue kv) => kv.GetIntPtr();
        public static explicit operator decimal(ImmutableKeyValue kv) => kv.GetDecimal();

        public Enumerator GetEnumerator() => new Enumerator(this);

        /// <summary>
        /// Disposes of the database memory for keeping track of values, returning it to its memory pool
        /// </summary>
        public void Dispose()
        {
            if (_pool == null)
                throw new InvalidOperationException("Only the root object can be disposed");

            _dbMemory.Dispose();
        }

        private DebugView DebuggerView => new DebugView(this);
        
        /// <summary>
        /// Provides an enumerator for enumerating through an <see cref="ImmutableKeyValue"/>'s subkeys
        /// </summary>
        public ref struct Enumerator
        {
            private readonly ImmutableKeyValue _keyValue;
            private DbRow _currentRecord;
            private int _dbIndex;
            private int _nextDbIndex;

            internal Enumerator(ImmutableKeyValue keyValue)
            {
                _keyValue = keyValue;
                _currentRecord = keyValue.Record;
                _dbIndex = 0;
                _nextDbIndex = DbRow.Size;
            }

            /// <summary>
            /// Returns the <see cref="ImmutableKeyValue"/> at the current position
            /// </summary>
            public ImmutableKeyValue Current
            {
                get
                {
                    int newStart = _dbIndex;
                    int newEnd = _dbIndex + DbRow.Size;

                    if (!_currentRecord.IsSimpleValue)
                    {
                        newEnd += DbRow.Size * _currentRecord.Length;
                    }
                    return new ImmutableKeyValue(_keyValue._values, _keyValue._db.Slice(newStart, newEnd - newStart), _keyValue._binarySpan);
                }
            }

            /// <summary>
            /// Moves the enumerator to the position of the next <see cref="ImmutableKeyValue"/>
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                _dbIndex = _nextDbIndex;
                if (_dbIndex >= _keyValue._db.Length)
                    return false;

                _currentRecord = ReadMachineEndian<DbRow>(_keyValue._db.Slice(_dbIndex));

                if (!_currentRecord.IsSimpleValue)
                    _nextDbIndex += _currentRecord.Length * DbRow.Size;

                _nextDbIndex += DbRow.Size;
                return _dbIndex < _keyValue._db.Length;
            }
        }

        [DebuggerDisplay("{DebuggerDisplay,nq}")]
        internal sealed class DebugView
        {
            public DebugView(ImmutableKeyValue value)
            {
                object[] items = new object[value.Length];
                int i = 0;
                foreach(var subKey in value)
                {
                    items[i] = subKey.Type == 0 ? (object)new ValuesTypeProxy(subKey) : new ValueTypeProxy(subKey);
                    i++;
                }

                Items = items;
            }
            
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public object[] Items { get; }
            
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private string DebuggerDisplay => "";
            
            [DebuggerDisplay("{_value}", Name = "{_key,nq}")]
            private class ValueTypeProxy
            {
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private readonly string _key;
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private readonly object _value;
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private readonly KeyValueType _type;

                public ValueTypeProxy(ImmutableKeyValue kv)
                {
                    _key = kv.Key;
                    _type = kv.Type;
                    _value = kv.GetValue();
                }
            }

            [DebuggerDisplay("Length = {Items.Length}", Name = "{_key,nq}")]
            private class ValuesTypeProxy
            {
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private readonly string _key;

                public ValuesTypeProxy(ImmutableKeyValue kv)
                {
                    _key = kv.Key;

                    object[] items = new object[kv.Length];
                    int i = 0;
                    foreach (var subKey in kv)
                    {
                        items[i] = subKey.Type == 0 ? (object)new ValuesTypeProxy(subKey) : new ValueTypeProxy(subKey);
                        i++;
                    }

                    Items = items;
                }

                [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
                public object[] Items { get; }
            }
        }
    }
}
