module ISteamDirectory 

    open FSharp.Steam.Rest
    open FSharp.Steam.Web

    let getCMList (cell : uint32, count : uint32 option) = {
        Interface = "ISteamDirectory";
        Method = "GetCMList";
        Version = 1;
        RequireKey = false;
        HttpMethod = Get;
        Parameters = [ "cellid", param cell; "maxcount", optional count; ]
    }

    let getCSList (cell : uint32, count : uint32 option) = {
        Interface = "ISteamDirectory";
        Method = "GetCSList";
        Version = 1;
        RequireKey = false;
        HttpMethod = Get;
        Parameters = [ "cellid", param cell; "maxcount", optional count; ]
    }