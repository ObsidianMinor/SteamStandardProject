namespace FSharp.Steam

open System

[<AutoOpen>]
module Primitives =

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

        static member op_Explicit (id : App) = id.ToUInt32 ()

        member this.ToUInt32 () =
            let (Id id) = this in id

    type Depot =
        private | Id of uint32 with

        static member Create id = Id id

        static member TryCreate id = Some (Id id)

        static member op_Explicit (id : App) = id.ToUInt32 ()

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

    /// Raises a SteamException if the specified int value is not 1
    let raiseIntResult result =
        match result with
            | 1 -> ()
            | _ -> raiseResult (enum result)
