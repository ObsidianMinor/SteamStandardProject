using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Steam.KeyValues
{
    internal readonly ref struct KVDatabase
    {
        private readonly Span<DbRow> _rows;
        private readonly OwnedMemory<byte> _memory;

        private KVDatabase(Span<DbRow> span, OwnedMemory<byte> memory)
        {
            _rows = span;
            _memory = memory;
        }

        public ref DbRow Current => ref _rows[0];
        
        public int Length => Current.IsSimpleValue ? 0 : Current.Length;

        public void Dispose()
        {
            if (_memory == null)
                throw new InvalidOperationException("Cannot dispose non-root database");

            _memory.Dispose();
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public ref struct Enumerator
        {
            KVDatabase _db;
            DbRow _currentRow;
            int _index;
            int _nextIndex;

            public Enumerator(KVDatabase database)
            {
                _db = database;
                _currentRow = _db.Current;
                _index = 0;
                _nextIndex = 1;
            }

            public KVDatabase Current
            {
                get
                {
                    int newStart = _index;
                    int newEnd = _index + 1;

                    if (!_currentRow.IsSimpleValue)
                        newEnd += _currentRow.Length;

                    return new KVDatabase(_db._rows.Slice(newStart, newEnd - newStart), null);
                }
            }

            public bool MoveNext()
            {
                _index = _nextIndex;
                if (_index >= _db._rows.Length)
                    return false;

                _currentRow = _db._rows[_index];

                if (!_currentRow.IsSimpleValue)
                    _nextIndex += _currentRow.Length;

                _nextIndex++;

                return _index < _db._rows.Length;
            }
        }

        public ref struct Builder
        {
            private int _index;
            private int _length;
            private Span<DbRow> _db;
            private OwnedMemory<byte> _memory;
            private MemoryPool<byte> _pool;

            public Builder(MemoryPool<byte> pool, int initialSize)
            {
                _index = 0;
                _pool = pool;
                _memory = pool.Rent(initialSize * DbRow.Size);
                _db = _memory.Span.NonPortableCast<byte, DbRow>();
                _length = _db.Length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AppendRow(int keyPos, int keyLength, KeyValueType type, int valuePos, int valueLength)
            {
                ref DbRow row = ref AppendRow();
                row.KeyLocation = keyPos;
                row.KeyLength = keyLength;
                row.Type = type;
                row.Location = valuePos;
                row.Length = valueLength;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref DbRow AppendRow()
            {
                Resize();

                return ref _db[_index++];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Resize()
            {
                if (_index < _length)
                    return;

                var oldData = _db;
                var newMemory = _pool.Rent(_memory.Length * 2);

                var newDb = newMemory.Span.NonPortableCast<byte, DbRow>();
                _db.CopyTo(newDb);
                _db = newDb;
                _length = _db.Length;

                _memory.Dispose();
                _memory = newMemory;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public KVDatabase Build()
            {
                return new KVDatabase(_db.Slice(0, _index), _memory);
            }

            public void Dispose()
            {
                _memory.Dispose();
            }
        }
    }
}
