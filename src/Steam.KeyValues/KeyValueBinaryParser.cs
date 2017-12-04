using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;

using static System.Buffers.Binary.BinaryPrimitives;

namespace Steam.KeyValues
{
    internal ref struct KeyValueBinaryParser
    {
        private Memory<byte> _db;
        private ReadOnlySpan<byte> _values;
        private OwnedMemory<byte> _scratchManager;
        private MemoryPool<byte> _pool;

        private int _valuesIndex;
        private int _dbIndex;

        public ImmutableKeyValue Parse(ReadOnlySpan<byte> data, MemoryPool<byte> pool)
        {
            _pool = pool ?? MemoryPool<byte>.Default;
            _scratchManager = _pool.Rent(data.Length * 4);
            _db = _scratchManager.Memory;

            _values = data;
            _valuesIndex = 0;
            _dbIndex = 0;

            if (_values[_valuesIndex] != 0)
                throw new InvalidDataException();

            _valuesIndex++;

            (int keyPos, int keyLength) = ReadString();
            int bodyPos = _valuesIndex;

            int dbPos = MoveDbPosition();
            int bodyLength = RecursiveParse();
            AppendDbRow(0, keyPos, keyLength, bodyPos, bodyLength, dbPos);

            var result = new ImmutableKeyValue(_values, _db.Slice(0, _dbIndex).Span, false, _pool, _scratchManager);
            _scratchManager = null;
            return result;
        }

        private int RecursiveParse()
        {
            int rowCount = 0;

            while (true)
            {
                var type = _values[_valuesIndex];
                _valuesIndex++;

                if (type == 8)
                    return rowCount;

                (int pos, int length) = ReadString();

                var kvType = (KeyValueType)type;

                switch((KeyValueType)type)
                {
                    case KeyValueType.None:
                        int oldValuePosition = _valuesIndex;
                        int oldPosition = MoveDbPosition();
                        int count = RecursiveParse();
                        AppendDbRow(0, pos, length, oldValuePosition, count, oldPosition);
                        break;
                    case KeyValueType.String:
                        (int valPos, int valLength) = ReadString();
                        AppendDbRow(KeyValueType.String, pos, length, valPos, valLength);
                        break;
                    case KeyValueType.Color:
                    case KeyValueType.Int32:
                    case KeyValueType.Pointer:
                    case KeyValueType.Float:
                        AppendDbRow(kvType, pos, length, _valuesIndex, sizeof(int));
                        _valuesIndex += sizeof(int);
                        break;
                    case KeyValueType.Int64:
                    case KeyValueType.UInt64:
                        AppendDbRow(kvType, pos, length, _valuesIndex, sizeof(long));
                        _valuesIndex += sizeof(long);
                        break;
                    default:
                        throw new InvalidDataException("The provided type is not valid");
                }

                rowCount++;
            }
        }

        private (int pos, int length) ReadString()
        {
            int start = _valuesIndex;
            while (_values[_valuesIndex] != 0)
                _valuesIndex++;
            int end = _valuesIndex - start;
            _valuesIndex++;
            return (start, end);
        }

        private void ResizeDb()
        {
            var oldData = _scratchManager.Span;
            var newScratch = _pool.Rent(_scratchManager.Length * 2);
            int dbLength = newScratch.Length / 2;

            var newDb = newScratch.Memory.Slice(0, dbLength);
            _db.Slice(0, _valuesIndex).Span.CopyTo(newDb.Span);
            _db = newDb;

            _scratchManager.Dispose();
            _scratchManager = newScratch;
        }
        
        private bool AppendDbRow(KeyValueType type, int keyIndex, int keyLength, int valuesIndex, int length, int dbPosition = -1)
        {
            if (dbPosition != -1)
            {
                var dbRow = new DbRow(keyIndex, keyLength, type, valuesIndex, length);
                WriteMachineEndian(_db.Span.Slice(dbPosition), ref dbRow);
                return true;
            }
            else
            {
                dbPosition = _dbIndex;
                var newIndex = _dbIndex + DbRow.Size;
                if (newIndex >= _db.Length)
                    ResizeDb();

                var dbRow = new DbRow(keyIndex, keyLength, type, valuesIndex, length);
                WriteMachineEndian(_db.Span.Slice(dbPosition), ref dbRow);
                _dbIndex = newIndex;
                return true;
            }
        }

        /// <summary>
        /// Moves the database index up
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int MoveDbPosition()
        {
            int old = _dbIndex;
            _dbIndex += DbRow.Size;
            return old;
        }

    }
}
