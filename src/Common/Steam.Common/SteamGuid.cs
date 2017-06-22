using System;

namespace Steam.Common
{
    /// <summary>
    /// Represents a Steam global unique identifier
    /// </summary>
    public struct SteamGuid : IEquatable<SteamGuid>
    {
        /// <summary>
        /// Represents the minimum start time for a process (January 1st, 2005)
        /// </summary>
        public static readonly DateTime ValveEpoch = new DateTime(2005, 1, 1);

        /// <summary>
        /// Gets the sequential count for this <see cref="SteamGuid"/>
        /// </summary>
        public uint SequentialCount { get; }
        /// <summary>
        /// Gets the start time of the server that generated this <see cref="SteamGuid"/>
        /// </summary>
        public DateTime StartTime { get; }
        /// <summary>
        /// Gets the process Id of the server that generated this <see cref="SteamGuid"/>
        /// </summary>
        public uint ProcessId { get; }
        /// <summary>
        /// Gets the box ID of the server that generated this <see cref="SteamGuid"/>
        /// </summary>
        public ushort BoxId { get; }

        /// <summary>
        /// Creates a new SteamGuid with the specified sequential count, start time, process Id, and box Id
        /// </summary>
        /// <param name="sequentialCount"></param>
        /// <param name="startTime"></param>
        /// <param name="processId"></param>
        /// <param name="boxId"></param>
        public SteamGuid(uint sequentialCount, DateTime startTime, byte processId, ushort boxId)
        {
            TimeSpan span = startTime.Subtract(ValveEpoch);
            if (span.Ticks < 0)
                throw new ArgumentOutOfRangeException("The provided start time is outside the valve epoch range");

            SequentialCount = sequentialCount;
            StartTime = startTime;
            ProcessId = processId;
            BoxId = boxId;
        }

        /// <summary>
        /// Converts this Steam guid to its 64 bit form
        /// </summary>
        /// <returns></returns>
        public ulong ToUInt64()
        {
            return SequentialCount | ((ulong)StartTime.Subtract(ValveEpoch).TotalSeconds << 20) | ((ulong)ProcessId << 50) | ((ulong)BoxId << 54);
        }

        /// <summary>
        /// Converts the provided 64 bit value into a SteamGuid
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SteamGuid FromUInt64(ulong value)
        {
            const int SequentialCountMask = 0xFFFFF;
            const int StartTimeSecondsMask = 0x3FFFFFFF;
            const int ProcessIdMask = 0xF;
            const ushort BoxIdMask = 0x3FF;

            uint sequentialCount = (uint)value & SequentialCountMask;
            uint startTimeSeconds = (uint)(value >> 20) & StartTimeSecondsMask;
            uint processId = (uint)(value >> 50) & ProcessIdMask;
            uint boxId = (uint)(value >> 54) & BoxIdMask;
            return new SteamGuid(sequentialCount, ValveEpoch + TimeSpan.FromSeconds(startTimeSeconds), (byte)processId, (ushort)boxId);
        }

        /// <summary>
        /// Converts this <see cref="SteamGuid"/> to a <see cref="UInt64"/>
        /// </summary>
        /// <param name="guid"></param>
        public static implicit operator UInt64(SteamGuid guid) => guid.ToUInt64();

        /// <summary>
        /// Converts the provided <see cref="UInt64"/> to a <see cref="SteamGuid"/>
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator SteamGuid(UInt64 value) => FromUInt64(value);

        /// <summary>
        /// Returns whether the provided <see cref="SteamGuid"/> is equal to this <see cref="SteamGuid"/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(SteamGuid other) => ToUInt64() == other.ToUInt64();

        /// <summary>
        /// Returns whether the provided object is equal to this <see cref="SteamGuid"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is SteamGuid guid))
                return false;

            return Equals(guid);
        }

        /// <summary>
        /// Gets the hash code of this <see cref="SteamGuid"/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ToUInt64().GetHashCode();
        }

        /// <summary>
        /// Returns the string representation of this <see cref="SteamGuid"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToUInt64().ToString();
        }

        /// <summary>
        /// Returns whether the two <see cref="SteamGuid"/>s are equal
        /// </summary>
        /// <param name="guid1"></param>
        /// <param name="guid2"></param>
        /// <returns></returns>
        public static bool operator ==(SteamGuid guid1, SteamGuid guid2)
        {
            return guid1.Equals(guid2);
        }
        /// <summary>
        /// Returns whether the two <see cref="SteamGuid"/>s are not equal
        /// </summary>
        /// <param name="guid1"></param>
        /// <param name="guid2"></param>
        /// <returns></returns>
        public static bool operator !=(SteamGuid guid1, SteamGuid guid2)
        {
            return !guid1.Equals(guid2);
        }
    }
}
