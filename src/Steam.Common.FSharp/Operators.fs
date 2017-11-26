namespace Steam

/// <summary> Basic operators for converting uint64 to Steam value types </summary>
[<AutoOpen>]
module Operators =

    [<CompiledName("FromSteamId")>]
    let inline steamid (value : uint64) = SteamId.FromCommunityId value
    [<CompiledName("FromSteam2Id")>]
    let inline steamid2 value = SteamId.FromSteamId value
    [<CompiledName("FromSteam3Id")>]
    let inline steamid3 value = SteamId.FromSteam3Id value

    [<CompiledName("FromGameId")>]
    let inline gameid value = GameId.FromUInt64 value

    [<CompiledName("FromSteamGid")>]
    let inline steamgid value = SteamGid.FromUInt64 value
