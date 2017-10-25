using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text.Utf8;

namespace Steam.KeyValues
{
    public ref struct KeyValue
    {
        private BufferPool _pool;
        private OwnedMemory<byte> _dbMemory;
        private ReadOnlySpan<byte> _db;
        private ReadOnlySpan<byte> _values;

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

        public unsafe static KeyValue ParseBinary(string file)
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


    }
}
