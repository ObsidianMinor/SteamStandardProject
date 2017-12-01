using System;
using System.Buffers;

namespace Steam.KeyValues
{
    internal ref struct KeyValueBinaryParser
    {
        private Memory<byte> _db;
        private ReadOnlySpan<byte> _values;
        private Memory<byte> _scratchMemory;
        private OwnedMemory<byte> _scratchManager;
        private MemoryPool<byte> _pool;

        private int _valuesIndex;
        private int _dbIndex;

        private KeyValueTokenType _tokenType;

        public ImmutableKeyValue Parse(ReadOnlySpan<byte> data, MemoryPool<byte> pool)
        {
            throw new NotImplementedException();
        }
    }
}
