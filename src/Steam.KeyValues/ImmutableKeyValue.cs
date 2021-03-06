﻿using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.Utf8;

using static System.Buffers.Binary.BinaryPrimitives;

namespace Steam.KeyValues
{
    /// <summary>
    /// Represents a KeyValue structure that is immutable, stack-only, and uses zero allocations
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly ref struct ImmutableKeyValue
    {
        private readonly KVDatabase _db;
        private readonly ReadOnlySpan<byte> _values;

        internal ImmutableKeyValue(ReadOnlySpan<byte> values, KVDatabase db)
        {
            _values = values;
            _db = db;
        }

        private ref DbRow Record => ref _db.Current;

        /// <summary>
        /// Get key of this <see cref="ImmutableKeyValue"/> as a <see cref="string"/>
        /// </summary>
        public string Key => Utf8Key.ToString();

        /// <summary>
        /// Gets the key of this <see cref="ImmutableKeyValue"/> as a <see cref="Utf8Span"/>
        /// </summary>
        public Utf8Span Utf8Key
        {
            get
            {
                return new Utf8Span(_values.Slice(Record.KeyLocation, Record.KeyLength));
            }
        }

        /// <summary>
        /// Gets the type of value this <see cref="ImmutableKeyValue"/> contains
        /// </summary>
        public KeyValueType Type => Record.Type;

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
        /// <param name="config"></param>
        /// <returns></returns>
        public static ImmutableKeyValue Parse(byte[] data, KeyValueParserConfig config = null) => Parse(new ReadOnlySpan<byte>(data), config);
        
        /// <summary>
        /// Parses the specified <see cref="ReadOnlySpan{T}"/> as a text stream
        /// </summary>
        /// <param name="utf8KeyValue"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static ImmutableKeyValue Parse(ReadOnlySpan<byte> utf8KeyValue, KeyValueParserConfig config = null)
        {
            return new KeyValueTextParser().Parse(utf8KeyValue, config);
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
        /// <param name="config"></param>
        /// <returns></returns>
        public static ImmutableKeyValue FromFile(string file, KeyValueParserConfig config = null)
        {
            var bytes = File.ReadAllBytes(file);
            if (bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return Parse(new Span<byte>(bytes).Slice(3), config);
            else
                return Parse(bytes, config);
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
        public static ImmutableKeyValue ParseBinary(byte[] data, MemoryPool<byte> pool = null) => ParseBinary(new ReadOnlySpan<byte>(data), pool);

        /// <summary>
        /// Parses the specified <see cref="ReadOnlySpan{T}"/> as a binary stream
        /// </summary>
        /// <param name="binaryKeyValue"></param>
        /// <param name="pool"></param>
        /// <returns></returns>
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
        public static ImmutableKeyValue FromBinaryFile(string file, MemoryPool<byte> pool = null) => ParseBinary(File.ReadAllBytes(file), pool);
        
        /// <summary>
        /// Gets the child <see cref="ImmutableKeyValue"/> with the specified key
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ImmutableKeyValue this[Utf8Span name] => TryGet(name, out var value) ? value : throw new KeyNotFoundException();
        
        /// <summary>
        /// Gets the child <see cref="ImmutableKeyValue"/> with the specified key
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ImmutableKeyValue this[string name] => TryGet(name, out var value) ? value : throw new KeyNotFoundException();

        /// <summary>
        /// Gets the child <see cref="ImmutableKeyValue"/> at the specified index
        /// </summary>
        /// <param name="index">The index to get the child</param>
        /// <returns>The <see cref="ImmutableKeyValue"/> at the specified index</returns>
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
                for (Enumerator enumerator = GetEnumerator(); enumerator.MoveNext(); length++) ;
                return length;
            }
        }

        /// <summary>
        /// Copies this <see cref="ImmutableKeyValue"/> to a mutable KeyValue object
        /// </summary>
        /// <returns></returns>
        public KeyValue ToKeyValue()
        {
            KeyValue root = KeyValue.Create(Key);

            foreach(ImmutableKeyValue kv in this)
                AddValue(kv, root);

            return root;
        }

        private void AddValue(ImmutableKeyValue kv, KeyValue instance)
        {
            switch (kv.Type)
            {
                case KeyValueType.None:
                    KeyValue subKey = instance.GetOrAdd(kv.Key);
                    foreach (ImmutableKeyValue immutableSubKey in kv)
                        AddValue(immutableSubKey, subKey);
                    break;
                case KeyValueType.Float:
                    instance.Add(kv.Key, kv.GetFloat());
                    break;
                case KeyValueType.Int32:
                    instance.Add(kv.Key, kv.GetInt32());
                    break;
                case KeyValueType.Int64:
                    instance.Add(kv.Key, kv.GetInt64());
                    break;
                case KeyValueType.Pointer:
                    instance.Add(kv.Key, kv.GetIntPtr());
                    break;
                case KeyValueType.String:
                    instance.Add(kv.Key, kv.GetString());
                    break;
                case KeyValueType.UInt64:
                    instance.Add(kv.Key, kv.GetUInt64());
                    break;
                case KeyValueType.WideString:
                    instance.Add(kv.Key, kv.GetString());
                    break;
                case KeyValueType.Color:
                    instance.Add(kv.Key, kv.GetColor());
                    break;
            }
        }

        /// <summary>
        /// Gets the value of this <see cref="ImmutableKeyValue"/> in the type specified by <see cref="Type"/>
        /// </summary>
        /// <returns></returns>
        public object GetValue(bool escapeString)
        {
            switch (Type)
            {
                case KeyValueType.WideString:
                case KeyValueType.String:
                    return escapeString ? GetEscapedString() : GetString();
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
        public bool TryGet(Utf8Span propertyName, out ImmutableKeyValue value)
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
        public bool TryGet(string propertyName, out ImmutableKeyValue value)
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

        /// <summary>
        /// Gets the value of this <see cref="ImmutableKeyValue"/> as an unsigned 64 bit integer
        /// </summary>
        /// <returns>The value of this <see cref="ImmutableKeyValue"/></returns>
        public ulong GetUInt64()
        {
            if (TryGetUInt64(out var value))
                return value;
            else
                throw new InvalidCastException();
        }

        /// <summary>
        /// Tries to get the value of this <see cref="ImmutableKeyValue"/> as an unsigned 64 bit integer
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Gets the value of this <see cref="ImmutableKeyValue"/> as a 64 bit integer
        /// </summary>
        /// <returns></returns>
        public long GetInt64()
        {
            if (TryGetInt64(out var value))
                return value;
            else
                throw new InvalidCastException();
        }

        /// <summary>
        /// Tries to get the value of this <see cref="ImmutableKeyValue"/> as a 64 bit integer
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetInt64(out long value)
        {
            value = default;

            var record = Record;
            if (!record.IsSimpleValue)
                return false;

            switch (record.Type)
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
                    value = ReadMachineEndian<long>(_values.Slice(record.Location, record.Length));
                    return true;
            }
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
        
        public Utf8String GetUtf8String()
        {
            if (TryGetUtf8String(out var value))
                return value;
            else
                throw new InvalidCastException();
        }
        
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

        public bool TryGetEscapedString(out string value)
        {
            value = default;

            if (!TryGetUtf8String(out var unescaped))
                return false;
            else
            {
                var unescapedString = unescaped.ToString();
                value = unescapedString.Replace("\\\\", "\\").Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t");
                return true;
            }
        }

        public string GetEscapedString()
        {
            if (TryGetEscapedString(out var value))
                return value;
            else
                throw new InvalidCastException();
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
        public static explicit operator Utf8String(ImmutableKeyValue kv) => kv.GetUtf8String();
        public static explicit operator string(ImmutableKeyValue kv) => kv.GetString();
        public static explicit operator float(ImmutableKeyValue kv) => kv.GetFloat();
        public static explicit operator long(ImmutableKeyValue kv) => kv.GetInt64();
        public static explicit operator ulong(ImmutableKeyValue kv) => kv.GetUInt64();
        public static explicit operator int(ImmutableKeyValue kv) => kv.GetInt32();
        public static explicit operator IntPtr(ImmutableKeyValue kv) => kv.GetIntPtr();

        public Enumerator GetEnumerator() => new Enumerator(this);

        /// <summary>
        /// Disposes of the database memory for keeping track of values, returning it to its memory pool
        /// </summary>
        public void Dispose()
        {
            _db.Dispose();
        }

        [DebuggerDisplay("Expanding the results view will enumerate the KeyValue", Name = "Results View", Type = "")]
        private KeyValueResultsView ResultsView => new KeyValueResultsView(this);
        
        /// <summary>
        /// Provides an enumerator for enumerating through an <see cref="ImmutableKeyValue"/>'s subkeys
        /// </summary>
        public ref struct Enumerator
        {
            private ImmutableKeyValue _keyValues;
            private KVDatabase.Enumerator _db;

            internal Enumerator(ImmutableKeyValue keyValue)
            {
                _keyValues = keyValue;
                _db = keyValue._db.GetEnumerator();
            }

            /// <summary>
            /// Returns the <see cref="ImmutableKeyValue"/> at the current position
            /// </summary>
            public ImmutableKeyValue Current => new ImmutableKeyValue(_keyValues._values, _db.Current);

            /// <summary>
            /// Moves the enumerator to the position of the next <see cref="ImmutableKeyValue"/>
            /// </summary>
            /// <returns></returns>
            public bool MoveNext() => _db.MoveNext();
        }
    }
}
