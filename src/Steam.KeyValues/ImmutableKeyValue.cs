using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Utf8;

namespace Steam.KeyValues
{
    /// <summary>
    /// Represents a stack-only, zero-allocation, immutable KeyValue structure
    /// </summary>
    public readonly ref struct ImmutableKeyValue
    {
        private readonly BufferPool _pool;
        private readonly OwnedMemory<byte> _dbMemory;
        private readonly ReadOnlySpan<byte> _db;
        private readonly ReadOnlySpan<byte> _values;

        public string Name { get => throw new NotImplementedException(); }

        public static ImmutableKeyValue Parse(string file)
        {
            using (FileStream stream = File.Open(file, FileMode.Open))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return Parse(memoryStream.ToArray());
            }
        }

        public static ImmutableKeyValue Parse(byte[] data) => Parse(new ReadOnlySpan<byte>(data));
        
        [CLSCompliant(false)]
        public static ImmutableKeyValue Parse(ReadOnlySpan<byte> utf8ImmutableKeyValue)
        {
            throw new NotImplementedException();
        }

        public static ImmutableKeyValue ParseBinary(string file)
        {
            using (FileStream stream = File.Open(file, FileMode.Open))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return ParseBinary(memoryStream.ToArray());
            }
        }

        public static ImmutableKeyValue ParseBinary(byte[] data) => Parse(new ReadOnlySpan<byte>(data));

        [CLSCompliant(false)]
        public static ImmutableKeyValue ParseBinary(ReadOnlySpan<byte> binaryImmutableKeyValue)
        {
            throw new NotImplementedException();
        }

        internal ImmutableKeyValue(ReadOnlySpan<byte> values, ReadOnlySpan<byte> db, BufferPool pool = null, OwnedMemory<byte> dbMemory = null)
        {
            _values = values;
            _db = db;
            _pool = pool;
            _dbMemory = dbMemory;
        }

        [CLSCompliant(false)]
        public ImmutableKeyValue this[Utf8Span name] => TryGetValue(name, out var value) ? value : throw new KeyNotFoundException();

        public ImmutableKeyValue this[string name] => TryGetValue(name, out var value) ? value : throw new KeyNotFoundException();

        [CLSCompliant(false)]
        public bool TryGetValue(Utf8Span propertyName, out ImmutableKeyValue value)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string propertyName, out ImmutableKeyValue value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value of this <see cref="ImmutableKeyValue"/> as a 32 bit integer
        /// </summary>
        /// <returns>The value of this <see cref="ImmutableKeyValue"/></returns>
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

        public static explicit operator Color(ImmutableKeyValue kv) => kv.GetColor();
        public static explicit operator bool(ImmutableKeyValue kv) => kv.GetBool();
        public static explicit operator string(ImmutableKeyValue kv) => kv.GetString();
        public static explicit operator float(ImmutableKeyValue kv) => kv.GetFloat();
        public static explicit operator long(ImmutableKeyValue kv) => kv.GetInt64();
        public static explicit operator ulong(ImmutableKeyValue kv) => kv.GetUInt64();
        public static explicit operator int(ImmutableKeyValue kv) => kv.GetInt();
        public static explicit operator IntPtr(ImmutableKeyValue kv) => kv.GetPtr();
        public static explicit operator decimal(ImmutableKeyValue kv) => kv.GetDecimal();
    }
}
