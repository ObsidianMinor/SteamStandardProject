using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Utf8;

namespace Steam.KeyValues
{
    /// <summary>
    /// Represents an immutable KeyValue structure
    /// </summary>
    public ref struct KeyValue
    {
        private BufferPool _pool;
        private OwnedMemory<byte> _dbMemory;
        private ReadOnlySpan<byte> _db;
        private ReadOnlySpan<byte> _values;

        public string Name { get => throw new NotImplementedException(); }

        public static KeyValue Parse(string file)
        {
            using (FileStream stream = File.Open(file, FileMode.Open))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return Parse(memoryStream.ToArray());
            }
        }

        public static KeyValue Parse(byte[] data) => Parse(new ReadOnlySpan<byte>(data));
        
        [CLSCompliant(false)]
        public static KeyValue Parse(ReadOnlySpan<byte> utf8KeyValue)
        {
            throw new NotImplementedException();
        }

        public static KeyValue ParseBinary(string file)
        {
            using (FileStream stream = File.Open(file, FileMode.Open))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return ParseBinary(memoryStream.ToArray());
            }
        }

        public static KeyValue ParseBinary(byte[] data) => Parse(new ReadOnlySpan<byte>(data));

        [CLSCompliant(false)]
        public static KeyValue ParseBinary(ReadOnlySpan<byte> binaryKeyValue)
        {
            throw new NotImplementedException();
        }

        internal KeyValue(ReadOnlySpan<byte> values, ReadOnlySpan<byte> db, BufferPool pool = null, OwnedMemory<byte> dbMemory = null)
        {
            _values = values;
            _db = db;
            _pool = pool;
            _dbMemory = dbMemory;
        }

        [CLSCompliant(false)]
        public KeyValue this[Utf8Span name] => TryGetValue(name, out var value) ? value : throw new KeyNotFoundException();

        public KeyValue this[string name] => TryGetValue(name, out var value) ? value : throw new KeyNotFoundException();

        [CLSCompliant(false)]
        public bool TryGetValue(Utf8Span propertyName, out KeyValue value)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string propertyName, out KeyValue value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value of this <see cref="KeyValue"/> as a 32 bit integer
        /// </summary>
        /// <returns>The value of this <see cref="KeyValue"/></returns>
        public int GetInt()
        {
            if (TryGetInt(out var value))
                return value;
            else
                throw new InvalidOperationException();
        }
        
        public bool TryGetInt(out int value) => throw new NotImplementedException();

        public ulong GetUInt64()
        {
            if (TryGetUInt64(out var value))
                return value;
            else
                throw new InvalidOperationException();
        }

        public bool TryGetUInt64(out ulong value) => throw new NotImplementedException();

        public long GetInt64()
        {
            if (TryGetInt64(out var value))
                return value;
            else
                throw new InvalidOperationException();
        }

        public bool TryGetInt64(out long value) => throw new NotImplementedException();

        /// <summary>
        /// Gets the value of this <see cref="KeyValue"/> as a UInt64
        /// </summary>
        /// <returns>A decimal container for an UInt64</returns>
        public decimal GetDecimal() => GetUInt64();

        public bool TryGetDecimal(out decimal value)
        {
            bool success = TryGetUInt64(out var ulongValue);
            value = ulongValue;
            return success;
        }

        public IntPtr GetPtr()
        {
            if (TryGetPtr(out var value))
                return value;
            else
                throw new InvalidOperationException();
        }

        public bool TryGetPtr(out IntPtr value) => throw new NotImplementedException();

        public float GetFloat()
        {
            if (TryGetFloat(out var value))
                return value;
            else
                throw new InvalidOperationException();
        }

        public bool TryGetFloat(out float value) => throw new NotImplementedException();

        public string GetString()
        {
            if (TryGetString(out var value))
                return value;
            else
                throw new InvalidOperationException();
        }

        public bool TryGetString(out string value) => throw new NotImplementedException();

        public bool GetBool()
        {
            if (TryGetBool(out var value))
                return value;
            else
                throw new InvalidOperationException();
        }

        public bool TryGetBool(out bool value) => throw new NotImplementedException();

        public Color GetColor()
        {
            if (TryGetColor(out var value))
                return value;
            else
                throw new InvalidOperationException();
        }

        public bool TryGetColor(out Color value) => throw new NotImplementedException();

        public static explicit operator Color(KeyValue kv) => kv.GetColor();
        public static explicit operator bool(KeyValue kv) => kv.GetBool();
        public static explicit operator string(KeyValue kv) => kv.GetString();
        public static explicit operator float(KeyValue kv) => kv.GetFloat();
        public static explicit operator long(KeyValue kv) => kv.GetInt64();
        public static explicit operator ulong(KeyValue kv) => kv.GetUInt64();
        public static explicit operator int(KeyValue kv) => kv.GetInt();
        public static explicit operator IntPtr(KeyValue kv) => kv.GetPtr();
        public static explicit operator decimal(KeyValue kv) => kv.GetDecimal();
    }
}
