using System;

namespace Steam
{
    /// <summary>
    /// A structure that represents an app, mod, shortcut, or P2P file on Steam
    /// </summary>
    public struct GameId : IEquatable<GameId>
    {
        private readonly uint _modId;

        /// <summary>
        /// Represents a shortcut
        /// </summary>
        public static GameId Shortcut => 0x8000000002000000;

        /// <summary>
        /// The app Id of this game Id if it has one
        /// </summary>
        public int AppId { get; }

        /// <summary>
        /// The type of this game
        /// </summary>
        public GameType Type { get; }

        /// <summary>
        /// The mod Id of this game if it's a mod
        /// </summary>
        public long ModId => _modId;

        /// <summary>
        /// Creates a new <see cref="GameId"/> with the specified app Id, type, and mod Id. This API is not CLS compliant
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="type"></param>
        /// <param name="modId"></param>
        [CLSCompliant(false)]
        public GameId(int appId, GameType type, uint modId)
        {
            if (appId > 0xFFFFFF | appId < 0)
                throw new ArgumentOutOfRangeException(nameof(appId));

            if (type > GameType.P2P)
                throw new ArgumentOutOfRangeException(nameof(type));

            AppId = appId;
            Type = type;
            _modId = modId;
        }

        /// <summary>
        /// Creates a new <see cref="GameId"/> with the specified app Id, type, and mod Id
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="type"></param>
        /// <param name="modId"></param>
        public GameId(int appId, GameType type, long modId)
            : this(appId, type, modId > uint.MaxValue || modId < 0 ? throw new ArgumentOutOfRangeException(nameof(modId)) : (uint)modId)
        {
        }

        /// <summary>
        /// Converts this <see cref="GameId"/> to its 64 bit representation
        /// </summary>
        /// <returns></returns>
        [CLSCompliant(false)]
        public ulong ToUInt64()
        {
            return (uint)AppId | (ulong)Type << 40 | (ulong)ModId << 64;
        }

        /// <summary>
        /// Converts this <see cref="GameId"/> to its 64 bit representation
        /// </summary>
        /// <returns></returns>
        public decimal ToDecimal() => ToUInt64();

        /// <summary>
        /// Creates a <see cref="GameId"/> from its 64 bit representation
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public static GameId FromUInt64(ulong value)
        {
            const int AppIdMask = 0xFFFFFF;
            const int AppTypeMask = 0xFF;
            const uint ModIdMask = 0xFFFFFFFF;

            int appId = (int)value & AppIdMask;
            GameType type = (GameType)((value >> 24) & AppTypeMask);
            uint modId = (uint)value & ModIdMask;

            return new GameId(appId, type, modId);
        }

        /// <summary>
        /// Creates a <see cref="GameId"/> from its 64 bit representation
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static GameId FromDecimal(decimal value) => FromUInt64(decimal.ToUInt64(value));

        /// <summary>
        /// Returns whether the provided <see cref="GameId"/> is equal to this <see cref="GameId"/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(GameId other)
        {
            return AppId == other.AppId
                && ModId == other.ModId
                && Type == other.Type;
        }

        /// <summary>
        /// Returns the 64 bit representation of this <see cref="GameId"/> as a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToUInt64().ToString();
        }

        /// <summary>
        /// Returns whether the provided object is equal to this <see cref="GameId"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is GameId gameId))
                return false;

            return Equals(gameId);
        }

        /// <summary>
        /// Gets the hash code of this <see cref="GameId"/>
        /// </summary>
        public override int GetHashCode()
            => ToUInt64().GetHashCode();

        /// <summary>
        /// Converts this <see cref="GameId"/> to a <see cref="UInt64"/>
        /// </summary>
        [CLSCompliant(false)]
        public static implicit operator ulong(GameId id)
            => id.ToUInt64();

        /// <summary>
        /// Converts this <see cref="UInt64"/> to a <see cref="GameId"/>
        /// </summary>
        [CLSCompliant(false)]
        public static implicit operator GameId(ulong value)
            => FromUInt64(value);

        /// <summary>
        /// Converts this <see cref="GameId"/> to a <see cref="decimal"/>
        /// </summary>
        /// <param name="id"></param>
        public static explicit operator decimal(GameId id)
            => id.ToDecimal();


        /// <summary>
        /// Converts this <see cref="decimal"/> to a <see cref="GameId"/>
        /// </summary>
        /// <param name="value"></param>
        public static explicit operator GameId(decimal value)
            => FromDecimal(value);
    }
}