using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Utf8;

using static System.Buffers.Binary.BinaryPrimitives;

namespace Steam.KeyValues
{
    internal ref struct KeyValueTextParser // the time to beat is 21ms.
    {
        private Span<byte> _db;
        private ReadOnlySpan<byte> _values;
        private OwnedMemory<byte> _scratchManager;
        MemoryPool<byte> _pool;
        private string[] _conditions;

        private int _valuesIndex;
        private int _dbIndex;

        public ImmutableKeyValue Parse(ReadOnlySpan<byte> data, KeyValueParserConfig config)
        {
            _pool = config?.Pool ?? MemoryPool<byte>.Default;
            _conditions = config?.Conditions ?? KeyValueParserConfig.GetDefaultConditions();
            _scratchManager = _pool.Rent(data.Length * 2);
            _db = _scratchManager.Span;
            
            _values = data;
            _valuesIndex = 0;
            _dbIndex = 0;

            ref DbRow topRow = ref CreateDbRow();

            if (!ValidateHeader(ref topRow))
                throw new KeyValuesException("Header is not valid");

            topRow.Length = ReadBody();

            var result = new ImmutableKeyValue(_values, _db.Slice(0, _dbIndex), false, _pool, _scratchManager);
            _scratchManager = null;
            return result;
        }

        private bool ValidateHeader(ref DbRow row)
        {
            if (SkipAndPeekType(false) == KeyValueToken.Key)
            {
                ReadString(out var keyPos, out var keyLength);
                Utf8Span key = new Utf8Span(_values.Slice(keyPos, keyLength));
                if (key == new Utf8Span(KeyValueConstants.Base) || key == new Utf8Span(KeyValueConstants.Include))
                {
                    // skip the include statement, we can't use it
                    SkipWhitespace();
                    ReadString(out var _, out var _);

                    if (SkipAndPeekType(false) != KeyValueToken.Key)
                        return false;

                    ReadString(out keyPos, out keyLength);
                }

                var type = PeekTokenType(false);
                if (type == KeyValueToken.Conditional)
                {
                    EvaluateConditional(); // source sdk doesn't care and neither should we
                    SkipWhitespace();
                    type = PeekTokenType(false);
                }
                
                if (type != KeyValueToken.StartSubkeys)
                    return false;

                row.KeyLocation = keyPos;
                row.KeyLength = keyLength;
                row.Location = _valuesIndex;
                return true;
            }
            else
            {
                return false;
            }
        }

        private int ReadBody()
        {
            int numberOfRowsForMembers = 0;
            _valuesIndex++;

            while (true)
            {
                KeyValueToken token = SkipAndPeekType(false);
                if (token == KeyValueToken.EndSubkeys)
                {
                    _valuesIndex++;
                    break;
                }

                ReadString(out int pos, out int length);
                switch (SkipAndPeekType(true))
                {
                    case KeyValueToken.Value:
                        ReadString(out int valuePos, out int valueLength);

                        if (SkipAndPeekType(false) == KeyValueToken.Conditional && !EvaluateConditional())
                        {
                            continue;
                        }

                        AppendDbRow(KeyValueType.String, pos, length, valuePos, valueLength);
                        numberOfRowsForMembers++;
                        break;
                    case KeyValueToken.StartSubkeys:
                        ref DbRow row = ref CreateDbRow();
                        row.KeyLength = length;
                        row.KeyLocation = pos;
                        row.Type = 0;
                        row.Location = _valuesIndex;
                        row.Length = ReadBody();
                        numberOfRowsForMembers += row.Length + 1;
                        break;
                    case KeyValueToken.Conditional when !EvaluateConditional():
                        SkipChildren();
                        continue;
                }
            }

            return numberOfRowsForMembers;
        }

        /// <summary>
        /// Reads from an open bracket to a closing bracket
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SkipChildren()
        {
            _valuesIndex++; // eat open brace

            var newPosition = GetFinalCharPosition(KeyValueConstants.CloseBrace, KeyValueConstants.OpenBrace);

            _valuesIndex += newPosition;
            SkipWhitespace();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SkipLine()
        {
            do _valuesIndex++;
            while (_values[_valuesIndex] != KeyValueConstants.LineFeed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResizeDb()
        {
            var oldData = _scratchManager.Span;
            var newScratch = _pool.Rent(_scratchManager.Length * 2);
            int dbLength = newScratch.Length / 2;

            var newDb = newScratch.Span.Slice(0, dbLength);
            _db.Slice(0, _valuesIndex).CopyTo(newDb);
            _db = newDb;

            _scratchManager.Dispose();
            _scratchManager = newScratch;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Read()
        {
            var canRead = _valuesIndex < _values.Length;
            if (canRead)
            {
                SkipWhitespace();
            }
            return canRead;
        }

        /// <summary>
        /// Reads a string and returns its starting position and length in bytes
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReadString(out int pos, out int length)
        {
            if (_values[_valuesIndex] == KeyValueConstants.Quote)
            {
                _valuesIndex++;
                pos = _valuesIndex;
                length = GetFinalCharIfNotEscaped(KeyValueConstants.Quote, KeyValueConstants.BackSlash);
                _valuesIndex += length + 1;
                SkipWhitespace();
            }
            else // straight shot, move forward until whitespace
            {
                pos = _valuesIndex;
                length = 0;
                while (length < _values.Length && !KeyValueConstants.IsSpace(_values[_valuesIndex]))
                {
                    length++;
                    _valuesIndex++;
                }
                _valuesIndex++;
                SkipWhitespace();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetFinalCharPosition(byte charToFind, byte ignoreController)
        {
            int start = _valuesIndex;
            int realStart = _valuesIndex;
            
            int level = 0;
            while (true)
            {
                int indexOfIgnore = _values.Slice(start).IndexOf(ignoreController);
                int indexOfFind = _values.Slice(start).IndexOf(charToFind);

                if (indexOfIgnore < indexOfFind)
                    level++;
                else
                    level--;

                if (level == 0)
                    return (start + indexOfFind + 1) - realStart;

                start += indexOfIgnore < indexOfFind ? indexOfIgnore + 1 : indexOfFind + 1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetFinalCharIfNotEscaped(byte charToFind, byte ignoreController)
        {
            int start = _valuesIndex;
            int realStart = _valuesIndex;

            while (true)
            {
                int nextPosition = _values.Slice(start).IndexOf(charToFind);
                start += nextPosition;
                if (_values[start - 1] == ignoreController)
                    start++;
                else
                    return start - realStart;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SkipWhitespace()
        {
            while (KeyValueConstants.IsSpace(_values[_valuesIndex]))
                _valuesIndex++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AppendDbRow(KeyValueType type, int keyIndex, int keyLength, int valuesIndex, int length)
        {
            int dbPosition = CheckDbSize();

            var dbRow = new DbRow(keyIndex, keyLength, type, valuesIndex, length);
            WriteMachineEndian(_db.Slice(dbPosition), ref dbRow);
            _dbIndex = dbPosition + DbRow.Size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref DbRow CreateDbRow()
        {
            int dbPosition = CheckDbSize();

            var dbRow = new DbRow();
            WriteMachineEndian(_db.Slice(dbPosition), ref dbRow);
            _dbIndex = dbPosition + DbRow.Size;

            return ref Unsafe.As<byte, DbRow>(ref MemoryMarshal.GetReference(_db.Slice(dbPosition)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CheckDbSize()
        {
            int dbPosition = _dbIndex;
            var newIndex = _dbIndex + DbRow.Size;
            if (newIndex >= _db.Length)
                ResizeDb();

            return dbPosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private KeyValueToken SkipAndPeekType(bool findingValue)
        {
            SkipWhitespace();
            KeyValueToken type;
            for (type = PeekTokenType(findingValue); type == KeyValueToken.Comment; type = PeekTokenType(findingValue))
            {
                SkipLine();
                SkipWhitespace();
            }
            return type;
        }

        /// <summary>
        /// Determines the next likely token type
        /// </summary>
        /// <param name="findingValue"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private KeyValueToken PeekTokenType(bool findingValue)
        {
            switch(_values[_valuesIndex])
            {
                case KeyValueConstants.OpenBrace:
                    return KeyValueToken.StartSubkeys;
                case KeyValueConstants.Quote:
                default:
                    return findingValue ? KeyValueToken.Value : KeyValueToken.Key;
                case KeyValueConstants.OpenBracket:
                    return KeyValueToken.Conditional;
                case KeyValueConstants.CloseBrace:
                    return KeyValueToken.EndSubkeys;
                case (byte)'/' when _values[_valuesIndex + 1] == '/':
                    return KeyValueToken.Comment;
            }
        }

        /// <summary>
        /// Evaluates a conditional block, advances the value index past the conditional, and returns whether we should read the next token(s)
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EvaluateConditional()
        {
            _valuesIndex++;

            bool result = false;
            while(_values[_valuesIndex] != KeyValueConstants.CloseBracket)
            {
                if (result || _values[_valuesIndex] == '|')
                {
                    _valuesIndex++;
                    continue;
                }
                
                bool isNegated = _values[_valuesIndex] == KeyValueConstants.Bang;
                if (isNegated)
                    _valuesIndex++;
                
                for (int i = 0; i < _conditions.Length; i++)
                {
                    if (EqualsCondition(new Utf8Span(_conditions[i]).Bytes))
                    {
                        result = !isNegated;
                        break;
                    }
                }

                do _valuesIndex++;
                while (_values[_valuesIndex] != '|' && _values[_valuesIndex] != KeyValueConstants.CloseBracket);
            }

            _valuesIndex++;

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EqualsCondition(ReadOnlySpan<byte> condition)
        {
            return _values.Slice(_valuesIndex, condition.Length).SequenceEqual(condition);
        }
    }
}
