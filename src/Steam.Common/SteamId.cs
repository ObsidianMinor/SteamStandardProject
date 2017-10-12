using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Steam
{
    /// <summary>
    /// A unique identifier to identify a Steam account
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
        /// A generic invalid Steam ID with the value 0
        /// </summary>
        public static readonly SteamId Zero = 0;

        /// <summary>
        /// Sent from user game connection to an out of date game server that hasn't 
        /// implemented the protocol to provide its Steam ID
        /// </summary>
        public static readonly SteamId OutOfDateGameServer = new SteamId(0, 0, 0, 0);

        /// <summary>
        /// Sent from a user game connection to an sv_lan game server
        /// </summary>
        public static readonly SteamId LanModeGameServer = new SteamId(0, Universe.Public, 0, 0);

        /// <summary>
        /// Sent from a user game connection to a game server that has just booted but hasn't 
        /// started its Steam3 component and started logging on
        /// </summary>
        public static readonly SteamId NotYetInitializedGameServer = new SteamId(1, 0, 0, 0);

        /// <summary>
        /// Sent from a user game connection to a game server that isn't using the Steam 
        /// authentication system but still wants to support the "Join Game" option in the friends list
        /// </summary>
        public static readonly SteamId NoSteamGameServer = new SteamId(2, 0, 0, 0);

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

        private readonly uint _accountId;
        private readonly uint _accountInstance;

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
        public long AccountId => _accountId;
        /// <summary>
        /// Gets the 20 bit instance value
        /// </summary>
        public long AccountInstance => _accountInstance;
        
        private SteamId(bool pending, bool unknown) : this(0, 0, AccountType.Pending, 0)
        {
            if (pending && unknown)
                throw new ArgumentException("A SteamId can't be pending and unknown at the same time!");
            
            IsPending = pending;
            IsUnknown = unknown;
        }

        /// <summary>
        /// Creates a new SteamId with the specified account Id, universe, account type, and instance Id. This API is not CLS compliant
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="universe"></param>
        /// <param name="type"></param>
        /// <param name="instance"></param>
        [CLSCompliant(false)]
        public SteamId(uint accountId, Universe universe = Universe.Public, AccountType type = AccountType.Individual, uint instance = 1)
        {
            if (instance > 0xFFFFF)
                throw new ArgumentOutOfRangeException(nameof(instance));

            if (universe > Universe.Dev)
                throw new ArgumentOutOfRangeException(nameof(universe));

            if (type > AccountType.AnonUser)
                throw new ArgumentOutOfRangeException(nameof(type));

            _accountId = accountId;
            AccountUniverse = universe;
            AccountType = type;
            _accountInstance = instance;

            IsPending = false;
            IsUnknown = false;
        }

        /// <summary>
        /// Creates a new SteamId with the specified account Id, universe, account type, and instance Id
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="universe"></param>
        /// <param name="type"></param>
        /// <param name="instance"></param>
        public SteamId(long accountId, Universe universe = Universe.Public, AccountType type = AccountType.Individual, long instance = 1)
            : this(accountId > uint.MaxValue || accountId < uint.MinValue ? throw new ArgumentOutOfRangeException(nameof(accountId)) : (uint)accountId, universe, type, instance < uint.MinValue || instance > uint.MaxValue ? throw new ArgumentOutOfRangeException(nameof(instance)) : (uint)instance)
        {
        }

        /// <summary>
        /// Creates an anonymous user in the specified universe
        /// </summary>
        /// <param name="universe">The universe</param>
        /// <returns>A new Steam ID</returns>
        public static SteamId CreateAnonymousUser(Universe universe) => new SteamId(0, universe, AccountType.AnonUser, 0);

        /// <summary>
        /// Creates an anonymous game server in the specified universe
        /// </summary>
        /// <param name="universe">The universe</param>
        /// <returns>A new Steam ID</returns>
        public static SteamId CreateAnonymousGameServer(Universe universe) => new SteamId(0, universe, AccountType.AnonGameServer, 0);
        
        /// <summary>
        /// Converts a community Id into a Steam ID. This API is not CLS compliant
        /// </summary>
        /// <param name="communityId"></param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public static SteamId FromCommunityId(ulong communityId)
        {
            if (communityId == 0)
                return Zero;

            uint id = (uint)communityId & IdMask;
            uint instance = (uint)((communityId >> 32) & InstanceMask);
            AccountType type = (AccountType)((communityId >> 52) & 0xF); // add 20 to the shift
            Universe universe = (Universe)(communityId >> 56); // add 4 to the shift
            return new SteamId(id, universe, type, instance);
        }

        /// <summary>
        /// Converts a community Id into a Steam ID
        /// </summary>
        /// <param name="communityId"></param>
        /// <returns></returns>
        public static SteamId FromCommunityId(decimal communityId) => FromCommunityId(decimal.ToUInt64(communityId));

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
            uint magic = uint.Parse(match.Groups[2].Value); // I don't get it
            uint id = uint.Parse(match.Groups[3].Value);
            return new SteamId((id * 2) + magic, universe, AccountType.Individual, 1);
        }

        /// <summary>
        /// Processes a Steam3 ID in the format [y:x:w] or [y:x:w:z]. If the string doesn't match, it returns <see cref="Zero"/>
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
        /// Gets whether this Steam ID is an account to be filled in
        /// </summary>
        public bool IsBlankAnonymousAccount => AccountId == 0 && IsAnonymousAccount && AccountInstance == 0;

        /// <summary>
        /// Gets whether this Steam ID is for a persistent game server
        /// </summary>
        public bool IsPersistentGameServer => AccountType == AccountType.GameServer;

        /// <summary>
        /// Gets whether this Steam ID is for an anonymous or persistent game server
        /// </summary>
        public bool IsGameServer => IsAnonymousGameServer || IsPersistentGameServer;

        /// <summary>
        /// Gets whether this Steam ID is for a content server
        /// </summary>
        public bool IsContentServer => AccountType == AccountType.ContentServer;

        /// <summary>
        /// Gets whether this Steam ID is for a Steam group
        /// </summary>
        public bool IsClan => AccountType == AccountType.Clan;

        /// <summary>
        /// Gets whether this Steam ID is for a chat
        /// </summary>
        public bool IsChat => AccountType == AccountType.Chat;

        /// <summary>
        /// Gets whether this Steam ID is for a console user or individual user
        /// </summary>
        public bool IsIndividualAccount => AccountType == AccountType.Individual || IsConsoleUser;

        /// <summary>
        /// Gets whether this Steam ID is a fake for a PlayStation Network friend account
        /// </summary>
        public bool IsConsoleUser => AccountType == AccountType.ConsoleUser;

        /// <summary>
        /// Gets whether this Steam ID is for an anonymous game server
        /// </summary>
        public bool IsAnonymousGameServer => AccountType == AccountType.AnonGameServer;

        /// <summary>
        /// Gets whether this Steam ID is for an anonymous user
        /// </summary>
        public bool IsAnonymousUser => AccountType == AccountType.AnonUser;

        /// <summary>
        /// Gets whether this Steam ID is for an anonymous account
        /// </summary>
        public bool IsAnonymousAccount => IsAnonymousUser || IsAnonymousGameServer;

        /// <summary>
        /// Gets whether this Steam ID belongs to a lobby chat
        /// </summary>
        /// <returns></returns>
        public bool IsLobby => (AccountInstance & LobbyChatInstance) != 0 && AccountType == AccountType.Chat;

        /// <summary>
        /// Gets whether this Steam ID belongs to a group chat
        /// </summary>
        /// <returns></returns>
        public bool IsGroupChat => (AccountInstance & ClanChatInstance) != 0 && AccountType == AccountType.Chat;

        /// <summary>
        /// Gets the user account instance if it exists
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
        /// Renders this Steam ID in Steam3 text format
        /// </summary>
        /// <returns></returns>
        public string ToSteam3Id()
        {
            char typeCharacter = GetCharFromAccountType(AccountType);

            if (IsGroupChat)
                typeCharacter = 'c';
            else if (IsLobby)
                typeCharacter = 'L';

            bool renderInstance = AccountType == AccountType.AnonGameServer || AccountType == AccountType.Multiseat || (AccountType == AccountType.Individual && Instance != Instance.Desktop);
            return $"[{typeCharacter}:{(int)AccountUniverse}:{AccountId}{(renderInstance ? ":" + AccountInstance : "")}]";
        }

        /// <summary>
        /// Returns the matching chat Steam ID with the default instance of 0. If this Steam ID is already a chat ID, this method returns this Steam ID
        /// </summary>
        /// <returns></returns>
        public SteamId ToChat()
        {
            return IsChat ? this : new SteamId(AccountId, AccountUniverse, AccountType.Chat, 0);
        }

        /// <summary>
        /// Returns the matching clan Steam ID with the default instance of 0. If this Steam ID is already a clan ID, this method returns this Steam ID
        /// </summary>
        /// <returns></returns>
        public SteamId ToClan()
        {
            return IsClan ? this : new SteamId(AccountId, AccountUniverse, AccountType.Clan, 0);
        }

        /// <summary>
        /// Converts this chat ID to a matching clan ID
        /// </summary>
        /// <returns></returns>
        public SteamId ChatToClan()
        {
            if (AccountType != AccountType.Chat)
                throw new InvalidOperationException($"Cannot convert a {AccountType} Steam ID to a clan Steam ID");

            return ToClan();
        }

        /// <summary>
        /// Converts this clan ID to a matching chat ID
        /// </summary>
        /// <returns></returns>
        public SteamId ClanToChat()
        {
            if (AccountType != AccountType.Clan)
                throw new InvalidOperationException($"Cannot convert a {AccountType} Steam ID to a clan Steam ID");
            
            return ToChat();
        }
        
        /// <summary>
        /// Renders this Steam ID as an unsigned 64 bit integer. This API is not CLS compliant
        /// </summary>
        /// <returns></returns>
        [CLSCompliant(false)]
        public ulong ToCommunityId()
        {
            return _accountId | ((ulong)AccountInstance << 32) | ((ulong)AccountType << 52) | ((ulong)AccountUniverse << 56);
        }

        /// <summary>
        /// Renders this Steam ID as a decimal value
        /// </summary>
        /// <returns></returns>
        public decimal ToDecimalCommunityId() => ToCommunityId();

        /// <summary>
        /// Converts the static parts of a steam ID to a 64-bit representation.
        /// </summary>
        /// <returns></returns>
        [CLSCompliant(false)]
        public ulong ToStaticAccountKey()
        {
            return _accountId | ((ulong)AccountType << 52) | ((ulong)AccountUniverse << 56);
        }

        /// <summary>
        /// Converts the static parts of a steam ID to a 64-bit representation.
        /// </summary>
        /// <returns></returns>
        public decimal ToDecimalStaticAccountKey() => ToStaticAccountKey();

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

            return AccountId == id.AccountId
                && AccountInstance == id.AccountInstance
                && AccountType == id.AccountType
                && AccountUniverse == id.AccountUniverse;
        }

        /// <summary>
        /// Returns whether the provided object is equal to this <see cref="SteamId"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return (obj is SteamId id) ? Equals(id) : false;
        }

        /// <summary>
        /// Returns the hash code for this instance
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => ToCommunityId().GetHashCode();

        /// <summary>
        /// Returns this <see cref="SteamId"/> in Steam3 text format
        /// </summary>
        /// <returns></returns>
        public override string ToString() => ToSteam3Id();

        /// <summary>
        /// Performs an implicit conversion from a Steam ID to a ulong. This API is not CLS compliant
        /// </summary>
        /// <param name="id"></param>
        [CLSCompliant(false)]
        public static implicit operator ulong(SteamId id) => id.ToCommunityId();

        /// <summary>
        /// Performs an implict conversion from a ulong to a Steam ID. This API is not CLS compliant
        /// </summary>
        /// <param name="id"></param>
        [CLSCompliant(false)]
        public static implicit operator SteamId(ulong id) => FromCommunityId(id);

        /// <summary>
        /// Performs an implicit conversion from a Steam ID to a decimal
        /// </summary>
        /// <param name="id"></param>
        public static explicit operator decimal(SteamId id) => id.ToDecimalCommunityId();

        /// <summary>
        /// Performs an implict conversion from a decimal to a Steam ID
        /// </summary>
        /// <param name="id"></param>
        public static explicit operator SteamId(decimal id) => FromCommunityId(id);
    }
}
