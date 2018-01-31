using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;

namespace Steam.KeyValues
{
    internal ref struct KeyValueBinaryParser
    {
        private ReadOnlySpan<byte> _values;
        private KVDatabase.Builder _db;

        private int _valuesIndex;

        public ImmutableKeyValue Parse(ReadOnlySpan<byte> data, MemoryPool<byte> pool)
        {
            _values = data;
            _valuesIndex = 0;
            _db = new KVDatabase.Builder(pool ?? MemoryPool<byte>.Default, data.Length / 4);

            if (_values[_valuesIndex] != 0) // it started with a null
                throw new InvalidDataException();

            _valuesIndex++;

            ref DbRow topRow = ref _db.AppendRow();

            ReadString(out topRow.KeyLocation, out topRow.KeyLength);
            topRow.Location = _valuesIndex;
            topRow.Length = RecursiveParse();

            return new ImmutableKeyValue(_values, _db.Build());
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

                ref DbRow row = ref _db.AppendRow();
                ReadString(out row.KeyLocation, out row.KeyLength);

                var kvType = (KeyValueType)type;
                row.Type = kvType;

                switch(kvType)
                {
                    case KeyValueType.None:
                        row.Location = _valuesIndex;
                        row.Length = RecursiveParse();
                        break;
                    case KeyValueType.String:
                        ReadString(out row.Location, out row.Length);
                        break;
                    case KeyValueType.Color:
                    case KeyValueType.Int32:
                    case KeyValueType.Pointer:
                    case KeyValueType.Float:
                        row.Location = _valuesIndex;
                        row.Length = sizeof(int);
                        _valuesIndex += sizeof(int);
                        break;
                    case KeyValueType.Int64:
                    case KeyValueType.UInt64:
                        row.Location = _valuesIndex;
                        row.Length = sizeof(long);
                        _valuesIndex += sizeof(long);
                        break;
                    default:
                        throw new InvalidDataException("The provided type is not valid");
                }

                rowCount++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReadString(out int start, out int length)
        {
            start = _valuesIndex;
            while (_values[_valuesIndex] != 0)
                _valuesIndex++;
            length = _valuesIndex - start;
            _valuesIndex++;
        }
    }
}
