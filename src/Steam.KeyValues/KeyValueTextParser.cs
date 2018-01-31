using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text.Utf8;

namespace Steam.KeyValues
{
    internal ref struct KeyValueTextParser // the time to beat is 21ms.
    {
        private ReadOnlySpan<byte> _values;
        private KVDatabase.Builder _db;
        private string[] _conditions;

        private int _valuesIndex;

        public ImmutableKeyValue Parse(ReadOnlySpan<byte> data, KeyValueParserConfig config)
        {
            _conditions = config?.Conditions ?? KeyValueParserConfig.GetDefaultConditions();

            _values = data;
            _valuesIndex = 0;
            _db = new KVDatabase.Builder(config?.Pool ?? MemoryPool<byte>.Default, data.Length / 8);
            ref DbRow topRow = ref _db.AppendRow();

            if (!ValidateHeader(ref topRow))
            {
                _db.Dispose();
                throw new KeyValuesException("Header is not valid");
            }

            topRow.Length = ReadBody();
            
            return new ImmutableKeyValue(_values, _db.Build());
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

                        _db.AppendRow(pos, length, KeyValueType.String, valuePos, valueLength);
                        numberOfRowsForMembers++;
                        break;
                    case KeyValueToken.StartSubkeys:
                        ref DbRow row = ref _db.AppendRow();
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
