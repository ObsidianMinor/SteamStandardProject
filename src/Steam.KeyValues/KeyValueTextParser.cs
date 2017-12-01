﻿using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

using static System.Buffers.Binary.BinaryPrimitives;
using System.Text.Utf8;
using System.Buffers.Text;

namespace Steam.KeyValues
{
    internal ref struct KeyValueTextParser
    {
        private Memory<byte> _db;
        private ReadOnlySpan<byte> _values; // TODO: this should be ReadOnlyMemory<byte>
        private OwnedMemory<byte> _scratchManager;
        MemoryPool<byte> _pool;

        private int _valuesIndex;
        private int _dbIndex;

        private bool _evalConditionals;

        public ImmutableKeyValue Parse(ReadOnlySpan<byte> data, MemoryPool<byte> pool = null, bool useConditionals = true)
        {
            _evalConditionals = useConditionals;
            _pool = pool ?? MemoryPool<byte>.Default;
            _scratchManager = _pool.Rent(data.Length * 4);
            _db = _scratchManager.Memory;
            
            _values = data;
            _valuesIndex = 0;
            _dbIndex = 0;
            
            (bool valid, int keyPos, int keyLength, int bodyPos) = ValidateHeader();
            if (!valid)
                throw new KeyValuesException("Header is not valid");

            int dbPos = MoveDbPosition();
            int bodyLength = ReadBody();
            AppendDbRow(0, keyPos, keyLength, bodyPos, bodyLength, dbPos);

            var result = new ImmutableKeyValue(_values, _db.Slice(0, _dbIndex).Span, false, _pool, _scratchManager);
            _scratchManager = null;
            return result;
        }

        private (bool valid, int keyPos, int keyLength, int valuePos) ValidateHeader()
        {
            if (SkipAndPeekType(false) == KeyValueTokenType.PropertyName)
            {
                var (keyPos, keyLength) = ReadString();
                Utf8Span key = new Utf8Span(_values.Slice(keyPos, keyLength));
                if (key == new Utf8Span(KeyValueConstants.Base) || key == new Utf8Span(KeyValueConstants.Include))
                {
                    // skip the include statement, we can't use it
                    SkipWhitespace();
                    ReadString();

                    if (SkipAndPeekType(false) != KeyValueTokenType.PropertyName)
                        return (false, 0, 0, 0);

                    (keyPos, keyLength) = ReadString();
                }

                var type = PeekTokenType(false);
                if (type == KeyValueTokenType.Conditional)
                {
                    EvaluateConditional(); // source sdk doesn't care and neither should we
                    SkipWhitespace();
                    type = PeekTokenType(false);
                }
                
                if (type != KeyValueTokenType.StartSubkeys)
                    return (false, 0, 0, 0);
                
                return (true, keyPos, keyLength, _valuesIndex);
            }
            else
            {
                return (false, 0, 0, 0);
            }
        }

        private int ReadBody()
        {
            int numberOfRowsForMembers = 0;
            _valuesIndex++;

            while (true)
            {
                if (SkipAndPeekType(false) == KeyValueTokenType.EndSubkeys)
                {
                    _valuesIndex++;
                    break;
                }
                
                (int pos, int length) = ReadString();
                switch (SkipAndPeekType(true))
                {
                    case KeyValueTokenType.Value:
                        var value = ReadString();
                        
                        if (SkipAndPeekType(false) == KeyValueTokenType.Conditional && !EvaluateConditional())
                        {
                            continue;
                        }

                        AppendDbRow(KeyValueType.String, pos, length, value.pos, value.length);
                        numberOfRowsForMembers++;
                        break;
                    case KeyValueTokenType.StartSubkeys:
                        int dbPos = MoveDbPosition();
                        int bodyLength = ReadBody();
                        AppendDbRow(0, pos, length, _valuesIndex, bodyLength, dbPos);
                        numberOfRowsForMembers += bodyLength + 1;
                        break;
                    case KeyValueTokenType.Conditional when !EvaluateConditional():
                        SkipChildren();
                        continue;
                }
            }

            return numberOfRowsForMembers;
        }

        /// <summary>
        /// Reads from an open bracket to a closing bracket
        /// </summary>
        private void SkipChildren()
        {
            _valuesIndex++; // eat open brace

            var indexOfClosingBrace = _valuesIndex;
            do
            {
                indexOfClosingBrace = _values.Slice(indexOfClosingBrace).IndexOf(KeyValueConstants.OpenBrace);
            }
            while (AreNumOfCharAtEndOfStringOdd(_valuesIndex + indexOfClosingBrace - 2, KeyValueConstants.OpenBrace));

            _valuesIndex += indexOfClosingBrace + 1;
            SkipWhitespace();
        }

        private void ResizeDb()
        {
            throw new NotImplementedException();
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
        private (int pos, int length) ReadString()
        {
            if (_values[_valuesIndex] == KeyValueConstants.Quote)
            {
                _valuesIndex++;
                var indexOfClosingQuote = _valuesIndex;
                do
                {
                    indexOfClosingQuote = _values.Slice(indexOfClosingQuote).IndexOf((byte)'"');
                } while (AreNumOfCharAtEndOfStringOdd(_valuesIndex + indexOfClosingQuote - 2, (byte)'/'));

                int pos = _valuesIndex;

                _valuesIndex += indexOfClosingQuote + 1;
                SkipWhitespace();
                return (pos, indexOfClosingQuote);
            }
            else // straight shot, move forward until whitespace
            {
                int pos = _valuesIndex;
                int length;
                for (length = 0; length < _values.Length &&  Unicode.IsWhitespace(_values[_valuesIndex]); length++, _valuesIndex++) ;
                _valuesIndex++;
                SkipWhitespace();
                return (pos, length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SkipWhitespace()
        {
            while (Unicode.IsWhitespace(_values[_valuesIndex]))
                _valuesIndex++;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private KeyValueTokenType SkipAndPeekType(bool findingValue)
        {
            SkipWhitespace();
            return PeekTokenType(findingValue);
        }

        /// <summary>
        /// Determines the next likely token type
        /// </summary>
        /// <param name="findingValue"></param>
        /// <returns></returns>
        private KeyValueTokenType PeekTokenType(bool findingValue)
        {
            switch(_values[_valuesIndex])
            {
                case KeyValueConstants.OpenBrace:
                    return KeyValueTokenType.StartSubkeys;
                case KeyValueConstants.Quote:
                default:
                    return findingValue ? KeyValueTokenType.Value : KeyValueTokenType.PropertyName;
                case KeyValueConstants.OpenBracket:
                    return KeyValueTokenType.Conditional;
                case KeyValueConstants.CloseBrace:
                    return KeyValueTokenType.EndSubkeys;
                case (byte)'/':
                    return KeyValueTokenType.Comment;
            }
        }
        
        /// <summary>
        /// Evaluates a conditional block, advances the value index past the conditional, and returns whether we should read the next token(s)
        /// </summary>
        /// <returns></returns>
        private bool EvaluateConditional()
        {
            if (!_evalConditionals)
                return true;

            throw new NotImplementedException();
        }

        private bool AreNumOfCharAtEndOfStringOdd(int count, byte character)
        {
            var length = count - _valuesIndex;
            if (length < 0) return false;
            var nextByte = _values[count];
            if (nextByte != character) return false;
            var numOfChar = 0;
            while (nextByte == character)
            {
                numOfChar++;
                if ((length - numOfChar) < 0) return numOfChar % 2 != 0;
                nextByte = _values[count - numOfChar];
            }
            return numOfChar % 2 != 0;
        }
    }
}