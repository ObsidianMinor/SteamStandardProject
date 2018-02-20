[<AutoOpen>]
module FSharp.Steam.Core

open System
open System.Globalization

type App =
    private | Id of uint32 with

    static member Create id =
        if id < 0u || id > 0xFFFFFFu then
            Id id
        else
            raise (ArgumentOutOfRangeException "id")

    static member TryCreate id =
        if id < 0u || id > 0xFFFFFFu then
            Some (Id id)
        else
            None

    static member Create id =
        if id < 0 || id > 0xFFFFFF then
            Id (uint32 id)
        else
            raise (ArgumentOutOfRangeException "id")

    static member TryCreate id =
        if id < 0 || id > 0xFFFFFF then
            Some (Id (uint32 id))
        else
            None

    static member op_Explicit (id : App) = id.ToUInt32 ()

    member this.ToUInt32 () =
        let (Id id) = this in id

type Package =
    private | Id of uint32 with

    static member Create id =
        if id < 0u || id > 0xFFFFFFu then
            Id id
        else
            raise (ArgumentOutOfRangeException "id")

    static member TryCreate id =
        if id < 0u || id > 0xFFFFFFu then
            Some (Id id)
        else
            None

    static member op_Explicit (id : Package) = id.ToUInt32 ()

    member this.ToUInt32 () =
        let (Id id) = this in id

type Depot =
    private | Id of uint32 with

    static member Create id = Id id

    static member TryCreate id = Some (Id id)

    static member op_Explicit (id : Depot) = id.ToUInt32 ()

    member this.ToUInt32 () =
        let (Id id) = this in id

let app (id : int) = App.Create id
let depot = Depot.Create
let package = Package.Create

type NotificationPosition = 
    | TopLeft = 0
    | TopRight = 1
    | BottomLeft = 2
    | BottomRight = 3

type Region = 
    | USEast = 0
    | USWest = 1
    | SouthAmerica = 2
    | Europe = 3
    | Asia = 4
    | Australia = 5
    | MiddleEast = 6
    | Africa = 7
    | World = 255

type Result = 
    | OK = 1
    | Fail = 2
    | NoConnection = 3
    | [<Obsolete "This result has been removed">] NoConnectionRetry = 4
    | InvalidPassword = 5
    | LoggedInElsewhere = 6
    | InvalidProtocolVersion = 7
    | InvalidParameter = 8
    | FileNotFound = 9
    | Busy = 10
    | InvalidState = 11
    | InvalidName = 12
    | InvalidEmail = 13
    | DuplicateName = 14
    | AccessDenied = 15
    | Timeout = 16
    | Banned = 17
    | AccountNotFound = 18
    | InvalidSteamID = 19
    | ServiceUnavailable = 20
    | NotLoggedOn = 21
    | Pending = 22
    | EncryptionFailure = 23
    | InsufficientPrivilege = 24
    | LimitExceeded = 25
    | Revoked = 26
    | Expired = 27
    | AlreadyRedeemed = 28
    | DuplicateRequest = 29
    | AlreadyOwned = 30
    | IPNotFound = 31
    | PersistFailed = 32
    | LockingFailed = 33
    | LogonSessionReplaced = 34
    | ConnectFailed = 35
    | HandshakeFailed = 36
    | IOFailure = 37
    | RemoteDisconnect = 38
    | ShoppingCartNotFound = 39
    | Blocked = 40
    | Ignored = 41
    | NoMatch = 42
    | AccountDisabled = 43
    | ServiceReadOnly = 44
    | AccountNotFeatured = 45
    | AdministratorOK = 46
    | ContentVersion = 47
    | TryAnotherCM = 48
    | PasswordRequiredToKickSession = 49
    | AlreadyLoggedInElsewhere = 50
    | Suspended = 51
    | Cancelled = 52
    | DataCorruption = 53
    | DiskFull = 54
    | RemoteCallFailed = 55
    | PasswordUnset = 56
    | ExternalAccountUnlinked = 57
    | PSNTicketInvalid = 58
    | ExternalAccountAlreadyLinked = 59
    | RemoteFileConflict = 60
    | IllegalPassword = 61
    | SameAsPreviousValue = 62
    | AccountLogonDenied = 63
    | CannotUseOldPassword = 64
    | InvalidLoginAuthCode = 65
    | AccountLogonDeniedNoMail = 66
    | HardwareNotCapableOfIPT = 67
    | IPTInitError = 68
    | ParentalControlRestricted = 69
    | FacebookQueryError = 70
    | ExpiredLoginAuthCode = 71
    | IPLoginRestrictionFailed = 72
    | AccountLockedDown = 73
    | AccountLogonDeniedVerifiedEmailRequired = 74
    | NoMatchingURL = 75
    | BadResponse = 76
    | RequirePasswordReEntry = 77
    | ValueOutOfRange = 78
    | UnexpectedError = 79
    | Disabled = 80
    | InvalidCEGSubmission = 81
    | RestrictedDevice = 82
    | RegionLocked = 83
    | RateLimitExceeded = 84
    | AccountLoginDeniedNeedTwoFactor = 85
    | ItemDeleted = 86
    | AccountLoginDeniedThrottle = 87
    | TwoFactorCodeMismatch = 88
    | TwoFactorActivationCodeMismatch = 89

exception SteamException of Result

/// Raises the specified Result as a SteamException
let raiseResult result = raise (SteamException(result))

/// Raises a SteamException if the specified Result is not Result.OK
let raiseFailResult result = 
    match result with
        | Result.OK -> ()
        | _ -> raiseResult result

type AccountType = 
    | Invalid = 0
    | Individual = 1
    | Multiseat = 2
    | GameServer = 3
    | AnonGameServer = 4
    | Pending = 5
    | ContentServer = 6
    | Clan = 7
    | Chat = 8
    | SuperSeeder = 9
    | AnonUser = 10

[<Flags>]
type Instance = 
    | All = 0
    | Desktop = 1
    | Console = 2
    | Web = 4

type AccountInstance = 
    private | Value of int with

    member i.ToInt32() =
        let (Value n) = i in n

    member this.Instance : Instance = enum ( this.ToInt32() &&& 0b111 )

    member this.IsGroupChat = not (this.ToInt32() &&& 0x80000 = 0)

    member this.IsLobby = not (this.ToInt32() &&& 0x40000 = 0)

    static member op_Explicit (i : AccountInstance) = i.ToInt32()

    /// Creates a new account instance and returns
    /// Option.None if the value is out of range
    static member TryCreate v =
        if v < 0 || v > 0xFFFFF then
            Some (Value v)
        else
            None

    /// Creates a new account instance and raises an
    /// ArgumentOutOfRangeException if the value is out of range
    static member Create v =
        if v < 0 || v > 0xFFFFF then
            Value v
        else
            raise (ArgumentOutOfRangeException "v")

type AccountUniverse = 
    | Invalid = 0
    | Public = 1
    | Beta = 2
    | Internal = 3
    | Dev = 4

type SteamId = {
    Type : AccountType
    AccountId : uint32
    InstanceId : AccountInstance
    Universe : AccountUniverse
} with 
    static member op_Explicit s = uint64 s.AccountId ||| uint64 (s.InstanceId.ToInt32()) <<< 32 ||| uint64 s.Type <<< 52 ||| uint64 s.Universe <<< 56

    override this.ToString() =
        let getCharFromAccountType = 
            match this.Type with
                | AccountType.Invalid | AccountType.SuperSeeder | _ -> 'I'
                | AccountType.Individual -> 'U'
                | AccountType.AnonGameServer -> 'A'
                | AccountType.Multiseat -> 'M'
                | AccountType.GameServer -> 'G'
                | AccountType.Pending -> 'P'
                | AccountType.ContentServer -> 'C'
                | AccountType.Chat when this.InstanceId.IsLobby -> 'L'
                | AccountType.Chat when this.InstanceId.IsGroupChat -> 'c'
                | AccountType.Clan -> 'g'
                | AccountType.Chat -> 'T'
                | AccountType.AnonUser -> 'a'

        let typeCharacter = getCharFromAccountType
        let renderInstance = this.Type = AccountType.AnonGameServer 
                            || this.Type = AccountType.Multiseat 
                            || (this.Type = AccountType.Individual && this.InstanceId.Instance = Instance.Desktop)
        let universe = int this.Universe
        if renderInstance then
            let instance = int32 this.InstanceId
            sprintf "[%O:%i:%u:%u]" typeCharacter universe this.AccountId instance
        else
            sprintf "[%O:%i:%u]" typeCharacter universe this.AccountId

/// Represents a Steam2 text ID
type Steam2Id = 
    | SteamId of SteamId
    | Pending
    | Unknown
    member this.ToString (newFormat : bool option) =
        match this with
            | Pending -> "STEAM_ID_PENDING"
            | Unknown -> "UNKNOWN"
            | SteamId steamId -> 
                let universe = 
                    match newFormat with
                        | Some format when format -> 0
                        | _ -> int steamId.Universe
                let yVal = steamId.AccountId &&& 1u
                let zVal = int (Math.Floor (decimal steamId.AccountId / 2.0m))
                sprintf "STEAM_%i:%i:%i" universe yVal zVal

    override this.ToString() = this.ToString (Some true)

open System.Text.RegularExpressions

/// A shorthand for AccountInstance.Create
let instance = AccountInstance.Create

let steamid64 value =
    let id = uint32 (value &&& 0xFFFFFFFFuL)
    let instanceId = int (value >>> 32 &&& 0x000FFFFFuL)
    let accountType = enum (int (value >>> 52 &&& 0xFuL))
    let universe = enum (int (value >>> 56))
    { AccountId = id; InstanceId = instance instanceId; Type = accountType; Universe = universe }

let steamid3 value =
    let regex = new Regex "^\\[([IUMGAPCgTLca]):([0-4]):([0-9]+)(?::([0-9]+))?\\]"
    let regexMatch = regex.Match value
    if regexMatch.Success then
        let getTypeForChar c = 
            match c with
                | 'I' -> Some AccountType.Invalid
                | 'U' -> Some AccountType.Individual
                | 'A' -> Some AccountType.AnonGameServer
                | 'M' -> Some AccountType.Multiseat
                | 'G' -> Some AccountType.GameServer
                | 'P' -> Some AccountType.Pending
                | 'C' -> Some AccountType.ContentServer
                | 'g' -> Some AccountType.Clan
                | 'T' | 'L' | 'c' -> Some AccountType.Chat
                | 'a' -> Some AccountType.AnonUser
                | _ -> None
        let typeChar = regexMatch.Groups.[1].Value.[0]
        let possibleAccountType = getTypeForChar typeChar
        match possibleAccountType with
            | Some accountType -> 
                let universe = enum (Int32.Parse regexMatch.Groups.[2].Value)
                let id = UInt32.Parse regexMatch.Groups.[3].Value
                let endGroupValue = regexMatch.Groups.[4].Value
                let mutable instanceId = 0
                if not (String.IsNullOrWhiteSpace endGroupValue) then
                    instanceId <- Int32.Parse endGroupValue
                else if accountType = AccountType.Individual then
                    instanceId <- 1
                if typeChar = 'c' then
                    instanceId <- instanceId ||| 0x80000
                else if typeChar = 'L' then
                    instanceId <- instanceId ||| 0x40000

                Some { 
                    AccountId = id; 
                    Universe = universe; 
                    InstanceId = instance instanceId; 
                    Type = accountType
                }
            | None -> None
    else
        None

let steamid2 value = 
    match value with
        | "STEAM_ID_PENDING" -> Some Pending
        | "UNKNOWN" -> Some Unknown
        | _ -> 
            let steam2Regex = new Regex "STEAM_([0-4]{1}):([01]{1}):([0-9]+)"
            let regexMatch = steam2Regex.Match value
            if regexMatch.Success then
                let universe = enum ( Int32.Parse regexMatch.Groups.[1].Value )
                let magic = UInt32.Parse regexMatch.Groups.[2].Value
                let id = UInt32.Parse regexMatch.Groups.[3].Value
                Some (SteamId { 
                    Type = AccountType.Individual;
                    InstanceId = instance 1;
                    Universe = universe;
                    AccountId = (id * 2u) + magic
                })
            else
                None

module SteamId = 
    let zero = steamid64 0UL
    let outOfDateServer = zero
    let lanModeGameServer = { zero with Universe = AccountUniverse.Public }
    let notYetInitializedGameServer = { zero with AccountId = 1u }
    let noSteamGameServer = { zero with AccountId = 2u }

type GameType = 
    | App = 0 
    | Mod = 1 
    | Shortcut = 2 
    | P2PFile = 3

type GameId = {
    App : App
    Type : GameType
    Mod : uint32
} with
    static member op_Explicit s = uint64 (s.App.ToUInt32()) ||| uint64 s.Type <<< 24 ||| uint64 s.Mod <<< 32

let gameid value = 
    let appId = app (int32 (value &&& 0xFFFFFFUL))
    let gameType = enum (int32 (value >>> 24 &&& 0xFFUL))
    let modId = uint32 (value >>> 32 &&& 0xFFFFFFFFUL)
    { App = appId; Type = gameType; Mod = modId }

module GameId =
    let shortcut = gameid 0x8000000002000000UL

type Language = 
    | Arabic
    | Bulgarian
    | SimplifiedChinese
    | TraditionalChinese
    | Czech
    | Danish
    | Dutch
    | English
    | Finnish
    | French
    | German
    | Greek
    | Hungarian
    | Italian
    | Japanese
    | Korean
    | Norwegian
    | Polish
    | Portuguese
    | PortugueseBrazil
    | Romanian
    | Russian
    | Spanish
    | Swedish
    | Thai
    | Turkish
    | Ukrainian

type LanguageInfo = { 
    EnglishName : string;
    NativeName : string;
    ApiCode : string;
    WebApiCode : string;
    Culture : CultureInfo;
}

let private languageInfo = Map.ofList [ 
                            (Language.Arabic,             {EnglishName = "Arabic";                NativeName = "العربية";         ApiCode = "arabic";     WebApiCode = "ar";    Culture = CultureInfo.ReadOnly(new CultureInfo("ar"))});
                            (Language.Bulgarian,          {EnglishName = "Bulgarian";             NativeName = "български език";   ApiCode = "bulgarian";  WebApiCode = "bg";    Culture = CultureInfo.ReadOnly(new CultureInfo("bg"))});
                            (Language.SimplifiedChinese,  {EnglishName = "Chinese (Simplified)";  NativeName = "简体中文";          ApiCode = "schinese";   WebApiCode = "zh-CN"; Culture = CultureInfo.ReadOnly(new CultureInfo("zh-CN"))});
                            (Language.TraditionalChinese, {EnglishName = "Chinese (Traditional)"; NativeName = "繁體中文";          ApiCode = "tchinese";   WebApiCode = "zh-TW"; Culture = CultureInfo.ReadOnly(new CultureInfo("zh-TW"))});
                            (Language.Czech,              {EnglishName = "Czech";                 NativeName = "čeština";          ApiCode = "czech";      WebApiCode = "cs";    Culture = CultureInfo.ReadOnly(new CultureInfo("cs"))});
                            (Language.Danish,             {EnglishName = "Danish";                NativeName = "Dansk";            ApiCode = "danish";     WebApiCode = "da";    Culture = CultureInfo.ReadOnly(new CultureInfo("da"))});
                            (Language.Dutch,              {EnglishName = "Dutch";                 NativeName = "Nederlands";       ApiCode = "dutch";      WebApiCode = "nl";    Culture = CultureInfo.ReadOnly(new CultureInfo("nl"))});
                            (Language.English,            {EnglishName = "English";               NativeName = "English";          ApiCode = "english";    WebApiCode = "en";    Culture = CultureInfo.ReadOnly(new CultureInfo("en"))});
                            (Language.Finnish,            {EnglishName = "Finnish";               NativeName = "Suomi";            ApiCode = "finnish";    WebApiCode = "fi";    Culture = CultureInfo.ReadOnly(new CultureInfo("fi"))});
                            (Language.French,             {EnglishName = "French";                NativeName = "Français";         ApiCode = "french";     WebApiCode = "fr";    Culture = CultureInfo.ReadOnly(new CultureInfo("fr"))});
                            (Language.German,             {EnglishName = "German";                NativeName = "Deutsch";          ApiCode = "german";     WebApiCode = "de";    Culture = CultureInfo.ReadOnly(new CultureInfo("de"))});
                            (Language.Greek,              {EnglishName = "Greek";                 NativeName = "Ελληνικά";         ApiCode = "greek";      WebApiCode = "el";    Culture = CultureInfo.ReadOnly(new CultureInfo("el"))});
                            (Language.Hungarian,          {EnglishName = "Hungarian";             NativeName = "Magyar";           ApiCode = "hungarian";  WebApiCode = "hu";    Culture = CultureInfo.ReadOnly(new CultureInfo("hu"))});
                            (Language.Italian,            {EnglishName = "Italian";               NativeName = "Italiano";         ApiCode = "italian";    WebApiCode = "it";    Culture = CultureInfo.ReadOnly(new CultureInfo("it"))});
                            (Language.Japanese,           {EnglishName = "Japanese";              NativeName = "日本語";            ApiCode = "japanese";  WebApiCode = "ja";     Culture = CultureInfo.ReadOnly(new CultureInfo("ja"))});
                            (Language.Korean,             {EnglishName = "Korean";                NativeName = "한국어";            ApiCode = "koreana";   WebApiCode = "ko";     Culture = CultureInfo.ReadOnly(new CultureInfo("ko"))});
                            (Language.Norwegian,          {EnglishName = "Norwegian";             NativeName = "Norsk";            ApiCode = "norwegian";  WebApiCode = "no";    Culture = CultureInfo.ReadOnly(new CultureInfo("no"))});
                            (Language.Polish,             {EnglishName = "Polish";                NativeName = "Polski";           ApiCode = "polish";     WebApiCode = "pl";    Culture = CultureInfo.ReadOnly(new CultureInfo("pl"))});
                            (Language.Portuguese,         {EnglishName = "Portuguese";            NativeName = "Português";        ApiCode = "portuguese"; WebApiCode = "pt";    Culture = CultureInfo.ReadOnly(new CultureInfo("pt"))});
                            (Language.PortugueseBrazil,   {EnglishName = "Portuguese-Brazil";     NativeName = "Português-Brasil"; ApiCode = "brazilian";  WebApiCode = "pt-BR"; Culture = CultureInfo.ReadOnly(new CultureInfo("pt-BR"))});
                            (Language.Romanian,           {EnglishName = "Romanian";              NativeName = "Română";           ApiCode = "romanian";   WebApiCode = "ro";    Culture = CultureInfo.ReadOnly(new CultureInfo("ro"))});
                            (Language.Russian,            {EnglishName = "Russian";               NativeName = "Русский";          ApiCode = "russian";    WebApiCode = "ru";    Culture = CultureInfo.ReadOnly(new CultureInfo("ru"))});
                            (Language.Spanish,            {EnglishName = "Spanish";               NativeName = "Español";          ApiCode = "spanish";    WebApiCode = "es";    Culture = CultureInfo.ReadOnly(new CultureInfo("es"))});
                            (Language.Swedish,            {EnglishName = "Swedish";               NativeName = "Svenska";          ApiCode = "swedish";    WebApiCode = "sv";    Culture = CultureInfo.ReadOnly(new CultureInfo("sv"))});
                            (Language.Thai,               {EnglishName = "Thai";                  NativeName = "ไทย";              ApiCode = "thai";       WebApiCode = "th";    Culture = CultureInfo.ReadOnly(new CultureInfo("th"))});
                            (Language.Turkish,            {EnglishName = "Turkish";               NativeName = "Türkçe";           ApiCode = "turkish";    WebApiCode = "tr";    Culture = CultureInfo.ReadOnly(new CultureInfo("tr"))});
                            (Language.Ukrainian,          {EnglishName = "Ukrainian";             NativeName = "Українська";       ApiCode = "ukrainian";  WebApiCode = "uk";    Culture = CultureInfo.ReadOnly(new CultureInfo("uk"))})
                        ]

type Language with
    member this.LanguageInfo = languageInfo.[this]

let private valveEpoch : DateTimeOffset = DateTimeOffset(2005, 1, 1, 0, 0, 0, TimeSpan.Zero)

type SteamGid (sequentialCount, startTime : DateTimeOffset, processId, boxId) = 
    let sequentialCount = 
        match sequentialCount with
            | value when value < 0xFFFFFu -> value
            | _ -> invalidArg "sequentialCount" (sprintf "Value passed in was %d." sequentialCount)

    let startTime = 
        let span : TimeSpan = startTime.Subtract valveEpoch
        match span with
            | value when value.Ticks > 0L && value.TotalSeconds < float 0x3FFFFFFF -> startTime
            | _ -> invalidArg "sequentialCount" (sprintf "Value passed in was %d." sequentialCount)

    let processId = 
        match processId with
            | value when value < byte 0xF -> value
            | _ -> invalidArg "sequentialCount" (sprintf "Value passed in was %d." sequentialCount)

    let boxId = 
        match boxId with
            | value when value > int16 0 && value < int16 0x3FF -> value
            | _ -> invalidArg "sequentialCount" (sprintf "Value passed in was %d." sequentialCount)
    
    member __.SequentialCount = sequentialCount
    member  __.StartTime : DateTimeOffset = startTime
    member __.ProcessId = processId
    member __.BoxId = boxId

    override this.GetHashCode() = 
        let value = uint64 this
        value.GetHashCode()

    override this.Equals value = 
        match value with
            | :? SteamGid as gid -> (this :> IEquatable<SteamGid>).Equals (gid)
            | _ -> false

    override this.ToString() = 
        let value = uint64 this
        value.ToString()

    static member op_Explicit (value : SteamGid) = 
        let count = uint64 value.SequentialCount
        let span : TimeSpan = value.StartTime.Subtract (valveEpoch)
        let time =  uint64 span.TotalSeconds <<< 20
        let processx = uint64 value.ProcessId <<< 50
        let box = uint64 value.BoxId <<< 54

        count ||| time ||| processx ||| box

    interface IEquatable<SteamGid> with
        member this.Equals value = 
            value.SequentialCount = this.SequentialCount &&
            value.BoxId = this.BoxId &&
            value.ProcessId = this.ProcessId &&
            value.StartTime = this.StartTime

let steamgid (value : uint64) = 
    let countMask = 0xFFFFFu
    let startTimeMask = 0x3FFFFFFFu
    let processIdMask = 0xFu
    let boxIdMask = 0x3FFu

    let count = uint32 (uint32 value &&& countMask)
    let startTime = uint32 ((uint32 value >>> 20) &&& startTimeMask)
    let processId = uint32 ((uint32 value >>> 50) &&& processIdMask)
    let boxId = uint32 ((uint32 value >>> 54) &&& boxIdMask)
    SteamGid(count, valveEpoch.AddSeconds (float startTime), byte processId, int16 boxId)

module SteamGid = 
    let invalid = steamgid 0UL
    let transactionInvalid = steamgid System.UInt64.MaxValue