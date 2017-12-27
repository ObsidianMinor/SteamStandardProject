namespace FSharp.Steam

open System
open System.Text.RegularExpressions

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
                let mutable universe = int steamId.Universe
                match newFormat with
                    | Some format when format -> universe <- 0
                    | _ -> ()
                let yVal = steamId.AccountId &&& 1u
                let zVal = int (Math.Floor (decimal steamId.AccountId / 2.0m))
                sprintf "STEAM_%i:%i:%i" universe yVal zVal

    override this.ToString() = this.ToString (Some true)

[<AutoOpen>]
module SteamId = 

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