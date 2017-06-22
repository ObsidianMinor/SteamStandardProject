using System;

namespace Steam.Common
{
    /// <summary>
    /// A structure that represents an app, mod, shortcut, or P2P file on Steam
    /// </summary>
    public struct GameId : IEquatable<GameId>
    {
        /// <summary>
        /// The app Id of this game Id if it has one
        /// </summary>
        public uint AppId { get; }

        /// <summary>
        /// The type of this game
        /// </summary>
        public GameType Type { get; }

        /// <summary>
        /// The mod Id of this game if it's a mod
        /// </summary>
        public uint ModId { get; }

        /// <summary>
        /// Creates a new <see cref="GameId"/> with the specified app Id, type, and mod Id
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="type"></param>
        /// <param name="modId"></param>
        public GameId(uint appId, GameType type, uint modId)
        {
            if (appId > 0xFFFFFF)
                throw new ArgumentException("The provided app ID excedes the max value");

            if (type > GameType.P2P)
                throw new ArgumentNullException("The provided game type excedes the max value");

            AppId = appId;
            Type = type;
            ModId = modId;
        }

        /// <summary>
        /// Converts this game Id to its 64bit representation
        /// </summary>
        /// <returns></returns>
        public ulong ToUInt64()
        {
            return AppId | (ulong)Type << 40 | (ulong)ModId << 64;
        }

        /// <summary>
        /// Creates a game Id from its 64bit representation
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static GameId FromUInt64(ulong value)
        {
            const int AppIdMask = 0xFFFFFF;
            const int AppTypeMask = 0xFF;
            const uint ModIdMask = 0xFFFFFFFF;

            uint appId = (uint)value & AppIdMask;
            GameType type = (GameType)((value >> 24) & AppTypeMask);
            uint modId = (uint)value & ModIdMask;

            return new GameId(appId, type, modId);
        }

        /// <summary>
        /// Returns whether the provided <see cref="GameId"/> is equal to this <see cref="GameId"/>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(GameId other)
        {
            return ToUInt64() == other.ToUInt64();
        }

        /// <summary>
        /// Returns the 64bit representation of this <see cref="GameId"/> as a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
            => ToUInt64().ToString();

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
        /// Converts this game Id to its 64bit representation
        /// </summary>
        public static implicit operator ulong(GameId id)
            => id.ToUInt64();

        /// <summary>
        /// Creates a game Id from its 64bit representation
        /// </summary>
        public static implicit operator GameId(ulong value)
            => FromUInt64(value);
    }
}
