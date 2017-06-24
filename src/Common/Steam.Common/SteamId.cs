using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Steam.Common
{
    /// <summary>
    /// A Steam ID representation that can be converted to and from any string or integer format
    /// </summary>
    [DebuggerDisplay("{ToSteam3Id()}")]
    public struct SteamId : IEquatable<SteamId>
    {
        // ono regex
        private static readonly Regex steamIdReg = new Regex("STEAM_([0-4]{1}):([01]{1}):([0-9]+)");
        private static readonly Regex steamId3Reg = new Regex("^\\[([IUMGAPCgTLca]):([0-4]):([0-9]+)(?::([0-9]+))?\\]");

        /// <summary>
        /// Represents an unknown account
        /// </summary>
        public static readonly SteamId Unknown = new SteamId(false, true);

        /// <summary>
        /// Represents a pending Steam account that hasn't been verified by the authentication servers
        /// </summary>
        public static readonly SteamId Pending = new SteamId(true, false);

        /// <summary>
        /// Represents a Steam ID with the value 0
        /// </summary>
        public static SteamId Zero => new SteamId(0, 0, 0, 0);

        const string PendingString = "STEAM_ID_PENDING";
        const string UnknownString = "UNKNOWN";

        // ah fuck, masks and bitwise operations
        const uint IdMask = 0xFFFFFFFF;
        const int InstanceMask = 0x000FFFFF;
        
        private const int ChatAccountInstanceMask = 0x00000FFF; // hm. why is this not used
        /// <summary>
        /// Represents the clan chat instance value
        /// </summary>
        public const int ClanChatInstance = (InstanceMask + 1) >> 1;
        /// <summary>
        /// Represents the lobby chat instance value
        /// </summary>
        public const int LobbyChatInstance = (InstanceMask + 1) >> 2;
        /// <summary>
        /// Represents the mms lobby chat instance value
        /// </summary>
        public const int MMSLobbyChatInstance = (InstanceMask + 1) >> 3; // this isn't even used in the source sdk

        /// <summary>
        /// Gets whether this account is pending
        /// </summary>
        public bool IsPending { get; }

        /// <summary>
        /// Gets whether this account is unknown
        /// </summary>
        public bool IsUnknown { get; }

        /// <summary>
        /// The universe of this Steam ID
        /// </summary>
        public Universe AccountUniverse { get; }
        /// <summary>
        /// The account type of this Steam ID
        /// </summary>
        public AccountType AccountType { get; }
        /// <summary>
        /// The account ID of this Steam ID
        /// </summary>
        public uint AccountId { get; }
        /// <summary>
        /// Gets or sets the 20 bit instance value
        /// </summary>
        public uint AccountInstance { get; }

        private SteamId(bool pending, bool unknown) : this(0, 0, 0, 0)
        {
            if (pending && unknown)
                throw new ArgumentException("A SteamId can't be pending and unknown at the same time!");
            
            IsPending = pending;
            IsUnknown = unknown;
        }

        /// <summary>
        /// Creates a new SteamId with the specified account Id, universe, account type, and instance Id
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="universe"></param>
        /// <param name="type"></param>
        /// <param name="instance"></param>
        public SteamId(uint accountId, Universe universe = Universe.Public, AccountType type = AccountType.Individual, uint instance = 1)
        {
            if (instance > 0xFFFFF)
                throw new ArgumentOutOfRangeException(nameof(instance));

            if (universe > Universe.Dev)
                throw new ArgumentOutOfRangeException(nameof(universe));

            if (type > AccountType.AnonUser)
                throw new ArgumentOutOfRangeException(nameof(type));

            AccountId = accountId;
            AccountUniverse = universe;
            AccountType = type;
            AccountInstance = instance;

            IsPending = false;
            IsUnknown = false;
        }
        
        /// <summary>
        /// Converts a community Id into a Steam ID
        /// </summary>
        /// <param name="communityId"></param>
        /// <returns></returns>
        public static SteamId FromCommunityId(ulong communityId)
        {
            uint id = (uint)communityId & IdMask;
            uint instance = (uint)((communityId >> 32) & InstanceMask);
            AccountType type = (AccountType)((communityId >> 52) & 0xF); // add 20 to the shift
            Universe universe = (Universe)(communityId >> 56); // add 4 to the shift
            return new SteamId(id, universe, type, instance);
        }

        /// <summary>
        /// Processes a SteamID in the format STEAM_X:Y:Z
        /// </summary>
        /// <param name="steam2"></param>
        /// <returns></returns>
        public static SteamId FromSteamId(string steam2)
        {
            if (string.IsNullOrWhiteSpace(steam2))
                return Zero;

            if (steam2 == PendingString)
                return Pending;

            if (steam2 == UnknownString)
                return Unknown;

            Match match = steamIdReg.Match(steam2);
            if (!match.Success)
                return Zero;

            Universe universe = (Universe)int.Parse(match.Groups[1].Value);
            if (universe == Universe.Invalid)
                universe = Universe.Public;
            uint magic = uint.Parse(match.Groups[2].Value); // I don't get it
            uint id = uint.Parse(match.Groups[3].Value);
            return new SteamId((id * 2) + magic, universe, AccountType.Individual, 1);
        }

        /// <summary>
        /// Processes a Steam3 ID in the format [y:x:w] or [y:x:w:z]
        /// </summary>
        /// <param name="steam3"></param>
        /// <returns></returns>
        public static SteamId FromSteam3Id(string steam3)
        {
            if (string.IsNullOrWhiteSpace(steam3))
                return Zero;

            Match match = steamId3Reg.Match(steam3);
            if (!match.Success)
                return Zero;

            char typeChar = match.Groups[1].Value[0];
            AccountType type = GetAccountTypeFromChar(typeChar);
            Universe universe = (Universe)int.Parse(match.Groups[2].Value);
            uint id = uint.Parse(match.Groups[3].Value);
            string endGroupValue = match.Groups[4].Value;
            uint instance = 0;

            if (!string.IsNullOrWhiteSpace(endGroupValue))
                instance = uint.Parse(endGroupValue);
            else if (type == AccountType.Individual)
                instance = 1;
            
            if (typeChar == 'c')
                instance |= ClanChatInstance;
            else if (typeChar == 'L')
                instance |= LobbyChatInstance;

            return new SteamId(id, universe, type, instance);
        }

        /// <summary>
        /// Returns whether this Steam ID belongs to a lobby chat
        /// </summary>
        /// <returns></returns>
        public bool IsLobby => ((AccountInstance & LobbyChatInstance) != 0 || (AccountInstance & MMSLobbyChatInstance) != 0) && AccountType == AccountType.Chat;

        /// <summary>
        /// Sets whether this Steam ID belongs to a group chat
        /// </summary>
        /// <returns></returns>
        public bool IsGroupChat => (AccountInstance & ClanChatInstance) != 0 && AccountType == AccountType.Chat;

        /// <summary>
        /// Gets the instance user account instance if it exists
        /// </summary>
        public Instance Instance
        {
            get 
            {
                if (AccountType == AccountType.Clan || AccountType == AccountType.GameServer)
                    return 0;

                return (Instance)(AccountInstance & 0b111);
            }
        }

        /// <summary>
        /// Renders this Steam ID in Steam2 text format
        /// </summary>
        /// <param name="newFormat">If true, this will use the public universe value 1, otherwise it will use the old universe value 0</param>
        /// <returns></returns>
        public string ToSteam2Id(bool newFormat = true)
        {
            if (IsPending)
                return PendingString;
            else if (IsUnknown)
                return UnknownString;

            if (AccountType != AccountType.Individual)
                throw new InvalidOperationException("Cannot make Steam2 ID with non-individual ID");
            
            int universe = (int)AccountUniverse;
            if (!newFormat && universe == 1)
                universe = 0;

            return $"STEAM_{universe}:{AccountId & 1}:{Math.Floor(AccountId / 2d)}"; // don't question the AccountId & 1, it's just a short for if divisible by 2 use 1 else use 0
        }

        /// <summary>
        /// Renders this Steam ID in Steam3 text format. If it is pending or unknown this returns an empty string.
        /// </summary>
        /// <returns></returns>
        public string ToSteam3Id()
        {
            if (IsPending || IsUnknown)
                return string.Empty;

            char typeCharacter = GetCharFromAccountType(AccountType);

            if (IsGroupChat)
                typeCharacter = 'c';
            else if (IsLobby)
                typeCharacter = 'L';

            bool renderInstance = AccountType == AccountType.AnonGameServer || AccountType == AccountType.Multiseat || (AccountType == AccountType.Individual && Instance != Instance.Desktop);
            return $"[{typeCharacter}:{(int)AccountUniverse}:{AccountId}{(renderInstance ? ":" + AccountInstance : "")}]";
        }

        /// <summary>
        /// Renders this Steam ID as an unsigned 64 bit integer
        /// </summary>
        /// <returns></returns>
        public ulong ToCommunityId()
        {
            return AccountId | ((ulong)AccountInstance << 32) | ((ulong)AccountType << 52) | ((ulong)AccountUniverse << 56);
        }

        private char GetCharFromAccountType(AccountType type)
        {
            switch(type)
            {
                case AccountType.Individual:
                    return 'U';
                case AccountType.AnonGameServer:
                    return 'A';
                case AccountType.Multiseat:
                    return 'M';
                case AccountType.GameServer:
                    return 'G';
                case AccountType.Pending:
                    return 'P';
                case AccountType.ContentServer:
                    return 'C';
                case AccountType.Clan:
                    return 'g';
                case AccountType.Chat:
                    {
                        if (IsLobby)
                            return 'L';
                        else if (IsGroupChat)
                            return 'c';
                        else
                            return 'T';
                    }
                case AccountType.AnonUser:
                    return 'a';
                default:
                    return 'I';
            }
        }
        
        private static AccountType GetAccountTypeFromChar(char value)
        {
            switch(value)
            {
                case 'I':
                    return AccountType.Invalid;
                case 'U':
                    return AccountType.Individual;
                case 'A':
                    return AccountType.AnonGameServer;
                case 'M':
                    return AccountType.Multiseat;
                case 'G':
                    return AccountType.GameServer;
                case 'P':
                    return AccountType.Pending;
                case 'C':
                    return AccountType.ContentServer;
                case 'g':
                    return AccountType.Clan;
                case 'T':
                case 'L':
                case 'c':
                    return AccountType.Chat;
                case 'a':
                    return AccountType.AnonUser;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), "The provided character does not have a corresponding account type");
            }
        }

        /// <summary>
        /// Returns whether two SteamId instances are equal
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Equals(SteamId id)
        {
            if ((id.IsPending && IsPending) || (id.IsUnknown && IsUnknown))
                return true;

            return ToCommunityId() == id.ToCommunityId();
        }

        /// <summary>
        /// Performs an implicit conversion from a Steam ID to a ulong
        /// </summary>
        /// <param name="id"></param>
        public static implicit operator ulong(SteamId id) => id.ToCommunityId();

        /// <summary>
        /// Performs an implict conversion from a ulong to a Steam ID
        /// </summary>
        /// <param name="id"></param>
        public static implicit operator SteamId(ulong id) => FromCommunityId(id);
    }
}
