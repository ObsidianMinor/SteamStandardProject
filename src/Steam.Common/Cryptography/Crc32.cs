using System;
using System.Security.Cryptography;

namespace Steam.Cryptography
{
    /// <summary>
    /// 
    /// </summary>
    public class Crc32 : HashAlgorithm
    {
        internal const uint DefaultPolynomial = 0xEDB88320;
        internal const uint DefaultSeed = uint.MaxValue;

        private uint _hash;
        private uint _seed;
        private uint[] _table;
        private static uint[] _defaultTable;

        /// <summary>
        /// Constructs a <see cref="Crc32"/> instance using the default polynomial and default seed
        /// </summary>
        public Crc32() : this(DefaultPolynomial, DefaultSeed) { }

        /// <summary>
        /// Constructs a <see cref="Crc32"/> instance using the provided polynomial and seed
        /// </summary>
        /// <param name="polynomial"></param>
        /// <param name="seed"></param>
        public Crc32(long polynomial, long seed)
        {
            _table = InitializeTable(polynomial);
            _seed = seed > uint.MaxValue || seed < uint.MinValue ? throw new ArgumentOutOfRangeException(nameof(seed)) : (uint)seed;
            Initialize();
        }

        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        public override int HashSize => 32;

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize()
        {
            _hash = _seed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static long Compute(byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(DefaultPolynomial), DefaultSeed, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static long Compute(long seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(DefaultPolynomial), seed, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polynomial"></param>
        /// <param name="seed"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static long Compute(long polynomial, long seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Routes data written to the object into the hash algorithm for computing the hash
        /// </summary>
        /// <param name="array">The input to compute the hash code for. </param>
        /// <param name="ibStart">The offset into the byte array from which to begin using data. </param>
        /// <param name="cbSize">The number of bytes in the byte array to use as data. </param>
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            _hash = CalculateHash(_table, _hash, array, ibStart, cbSize);
        }

        /// <summary>
        /// Finalizes the hash computation after the last data is processed by the cryptographic stream object.
        /// </summary>
        /// <returns>The computed hash code.</returns>
        protected override byte[] HashFinal()
        {
            return GetBigEndianBytes(~_hash);
        }

        private static uint[] InitializeTable(long polynomial)
        {
            if (polynomial > uint.MaxValue || polynomial < uint.MinValue)
                throw new ArgumentOutOfRangeException(nameof(polynomial));

            uint poly = (uint)polynomial;

            if (poly == DefaultPolynomial && _defaultTable != null)
                return _defaultTable;

            uint[] createTable = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint entry = i;
                for (int j = 0; j < 8; j++)
                {
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ poly;
                    else
                        entry >>= 1;
                }
                createTable[i] = entry;
            }

            if (poly == DefaultPolynomial)
                _defaultTable = createTable;

            return createTable;
        }

        private static uint CalculateHash(uint[] table, long seed, byte[] buffer, int start, int size)
        {
            if (seed > uint.MaxValue || seed < uint.MinValue)
                throw new ArgumentOutOfRangeException(nameof(seed));

            uint crc = (uint)seed;
            for (int i = start; i < size; i++)
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
