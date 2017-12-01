using System;
using System.Collections.Generic;
using System.Text;

using static System.Buffers.Binary.BinaryPrimitives;

namespace Steam.KeyValues
{
    internal struct KvStack
    {
        private Memory<byte> _memory;
        private int _topOfStackSubkey;
        private int _capacity;
        private int _subkeyStackCount;

        public int SubkeyStackCount => _subkeyStackCount;

        public bool IsFull => _subkeyStackCount >= _capacity;

        public KvStack(Memory<byte> db)
        {
            _memory = db;
            _topOfStackSubkey = _memory.Length;
            _subkeyStackCount = 0;
            _capacity = _memory.Length / 4;
        }

        public bool TryPush(int membersCount)
        {
            if (!IsFull)
            {
                WriteMachineEndian(_memory.Slice(_topOfStackSubkey - 4).Span, ref membersCount);
                _topOfStackSubkey -= 4;
                _subkeyStackCount++;
                return true;
            }
            else
                return false;
        }

        public int Pop()
        {
            _subkeyStackCount--;
            var value = ReadMachineEndian<int>(_memory.Slice(_topOfStackSubkey).Span);
            _topOfStackSubkey += 4;
            return value;
        }

        internal void Resize(Memory<byte> newStackMemory)
        {
            _memory.Slice(0, _subkeyStackCount * 4).Span.CopyTo(newStackMemory.Span);
            _memory = newStackMemory;
        }
    }
}
