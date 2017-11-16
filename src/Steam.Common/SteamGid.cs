using System;

namespace Steam
{
    /// <summary>
    /// Represents a Steam global unique identifier
    /// </summary>
    public readonly struct SteamGid : IEquatable<SteamGid>
    {
        private readonly uint _sequentialCount;
        private readonly byte _processId;

        /// <summary>
        /// Represents the invalid value for a GID
        /// </summary>
        public static readonly SteamGid Invalid = ulong.MaxValue;

        /// <summary>
        /// Represents an unknown transaction
        /// </summary>
        public static readonly SteamGid TransactionUnknown = 0;

        /// <summary>
        /// Represents the minimum start time for a process (January 1st, 2005)
        /// </summary>
        public static readonly DateTime ValveEpoch = new DateTime(2005, 1, 1);

        /// <summary>
        /// Gets the sequential count for this <see cref="SteamGid"/>
        /// </summary>
        public long SequentialCount => _sequentialCount;
        /// <summary>
        /// Gets the start time of the server that generated this <see cref="SteamGid"/>
        /// </summary>
        public DateTime StartTime { get; }
        /// <summary>
        /// Gets the process Id of the server that generated this <see cref="SteamGid"/>
        /// </summary>
        public byte ProcessId => _processId;
        /// <summary>
        /// Gets the box ID of the server that generated this <see cref="SteamGid"/>
        /// </summary>
        public short BoxId { get; }

        /// <summary>
        /// Creates a new <see cref="SteamGid"/> with the specified sequential count, start time, process Id, and box Id. This API is not CLS compliant
        /// </summary>
        /// <param name="sequentialCount"></param>
        /// <param name="startTime"></param>
        /// <param name="processId"></param>
        /// <param name="boxId"></param>
        [CLSCompliant(false)]
        public SteamGid(uint sequentialCount, DateTime startTime, byte processId, short boxId)
        {
            TimeSpan span = startTime.Subtract(ValveEpoch);
            if (span.Ticks < 0 || span.TotalSeconds > 0x3FFFFFFF)
                throw new ArgumentOutOfRangeException(nameof(startTime), "The provided start time is outside the valve epoch range");

            _sequentialCount = sequentialCount > 0xFFFFF ? throw new ArgumentOutOfRangeException(nameof(sequentialCount)) : sequentialCount;
            StartTime = startTime;
            _processId = processId > 0xF ? throw new ArgumentOutOfRangeException(nameof(processId)) : processId;
            BoxId = boxId > 0x3FF || boxId < 0 ? throw new ArgumentOutOfRangeException(nameof(boxId)) : boxId;
        }

        /// <summary>
        /// Creates a new <see cref="SteamGid"/> with the specified sequential count, start time, process Id, and box Id
        /// </summary>
        /// <param name="sequentialCount"></param>
        /// <param name="startTime"></param>
        /// <param name="processId"></param>
        /// <param name="boxId"></param>
        public SteamGid(long sequentialCount, DateTime startTime, byte processId, short boxId) 
            : this(sequentialCount > 0xFFFFF || sequentialCount < 0 ? throw new ArgumentOutOfRangeException(nameof(sequentialCount)) : (uint)sequentialCount, startTime, processId, boxId)
        {
        }

        /// <summary>
        /// Converts this <see cref="SteamGid"/> to its 64 bit form. This API is not CLS compliant
        /// </summary>
        /// <returns></returns>
        [CLSCompliant(false)]
        public ulong ToUInt64()
        {
            return (ulong)SequentialCount | ((ulong)StartTime.Subtract(ValveEpoch).TotalSeconds << 20) | ((ulong)ProcessId << 50) | ((ulong)BoxId << 54);
        }

        /// <summary>
        /// Converts this <see cref="SteamGid"/> to its 64 bit form as a decimal
        /// </summary>
        /// <returns></returns>
        public decimal ToDecimal() => ToUInt64();

        /// <summary>
        /// Converts the provided 64 bit value into a <see cref="SteamGid"/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public static SteamGid FromUInt64(ulong value)
        {
            const int SequentialCountMask = 0xFFFFF;
            const int StartTimeSecondsMask = 0x3FFFFFFF;
            const int ProcessIdMask = 0xF;
            const ushort BoxIdMask = 0x3FF;

            uint sequentialCount = (uint)value & SequentialCountMask;
            uint startTimeSeconds = (uint)(value >> 20) & StartTimeSecondsMask;
            uint processId = (uint)(value >> 50) & ProcessIdMask;
            uint boxId = (uint)(value >> 54) & BoxIdMask;
            return new SteamGid(sequentialCount, ValveEpoch + TimeSpan.FromSeconds(startTimeSeconds), (byte)processId, (short)boxId);
        }

        /// <summary>
        /// Converts the provided decimal into a <see cref="SteamGid"/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SteamGid FromDecimal(decimal value) => FromUInt64(decimal.ToUInt64(value));

        /// <summary>
        /// Converts this <see cref="SteamGid"/> to a <see cref="ulong"/>
        /// </summary>
        /// <param name="gid"></param>
        [CLSCompliant(false)]
        public static implicit operator ulong(SteamGid gid) => gid.ToUInt64();

        /// <summary>
        /// Converts the provided <see cref="ulong"/> to a <see cref="SteamGid"/>
        /// </summary>
        /// <param name="value"></param>
        [CLSCompliant(false)]
        public static implicit operator SteamGid(ulong value) => FromUInt64(value);

        /// <summary>
        /// Converts this <see cref="SteamGid"/> to a <see cref="decimal"/>
        /// </summary>
        /// <param name="gid"></param>
        public static explicit operator decimal(SteamGid gid) => gid.ToDecimal();

        /// <summary>
        /// Converts the provided <see cref="ulong"/> to a <see cref="SteamGid"/>
        /// </summary>
        /// <param name="value"></param>
        public static explicit operator SteamGid(decimal value) => FromDecimal(value);

        /// <summary>
        /// Returns whether the provided <see cref="SteamGid"/> is equal to this <see cref="SteamGid"/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(SteamGid other) => ToUInt64() == other.ToUInt64();

        /// <summary>
        /// Returns whether the provided object is equal to this <see cref="SteamGid"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is SteamGid guid))
                return false;

            return Equals(guid);
        }

        /// <summary>
        /// Gets the hash code of this <see cref="SteamGid"/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ToUInt64().GetHashCode();
        }

        /// <summary>
        /// Returns the string representation of this <see cref="SteamGid"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToUInt64().ToString();
        }

        /// <summary>
        /// Returns whether the two <see cref="SteamGid"/>s are equal
        /// </summary>
        /// <param name="guid1"></param>
        /// <param name="guid2"></param>
        /// <returns></returns>
        public static bool operator ==(SteamGid guid1, SteamGid guid2)
        {
            return guid1.Equals(guid2);
        }
        /// <summary>
        /// Returns whether the two <see cref="SteamGid"/>s are not equal
        /// </summary>
        /// <param name="guid1"></param>
        /// <param name="guid2"></param>
        /// <returns></returns>
        public static bool operator !=(SteamGid guid1, SteamGid guid2)
        {
            return !guid1.Equals(guid2);
        }
    }
}
