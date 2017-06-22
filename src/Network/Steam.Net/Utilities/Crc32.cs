using System;
using System.Security.Cryptography;

namespace Steam.Net.Utilities
{
    internal class Crc32 : HashAlgorithm
    {
        internal const uint DefaultPolynomial = 0xEDB88320;
        internal const uint DefaultSeed = uint.MaxValue;

        private uint _hash;
        private uint _seed;
        private uint[] _table;
        private static uint[] _defaultTable;

        public Crc32() : this(DefaultPolynomial, DefaultSeed) { }

        public Crc32(uint polynomial, uint seed)
        {
            _table = InitializeTable(polynomial);
            _seed = seed;
            Initialize();
        }

        public override int HashSize => 32;

        public override void Initialize()
        {
            _hash = _seed;
        }

        public static uint Compute(byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(DefaultPolynomial), DefaultSeed, buffer, 0, buffer.Length);
        }

        public static uint Compute(uint seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(DefaultPolynomial), seed, buffer, 0, buffer.Length);
        }

        public static uint Compute(uint polynomial, uint seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            _hash = CalculateHash(_table, _hash, array, ibStart, cbSize);
        }

        protected override byte[] HashFinal()
        {
            return GetBigEndianBytes(~_hash);
        }
        
        private static uint[] InitializeTable(uint polynomial)
        {
            if (polynomial == DefaultPolynomial && _defaultTable != null)
                return _defaultTable;

            uint[] createTable = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint entry = i;
                for (int j = 0; j < 8; j++)
                {
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry >>= 1;
                }
                createTable[i] = entry;
            }

            if (polynomial == DefaultPolynomial)
                _defaultTable = createTable;

            return createTable;
        }

        private static uint CalculateHash(uint[] table, uint seed, byte[] buffer, int start, int size)
        {
            uint crc = seed;
            for(int i = start; i < size; i++)
            {
                unchecked
                {
                    crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
                }
            }
            return crc;
        }

        private byte[] GetBigEndianBytes(uint x)
        {
            return new byte[] {
                (byte)((x >> 24) & 0xff),
                (byte)((x >> 16) & 0xff),
                (byte)((x >> 8) & 0xff),
                (byte)(x & 0xff)
            };
        }
    }
}
