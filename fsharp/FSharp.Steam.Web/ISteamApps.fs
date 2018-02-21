module ISteamApps
    open FSharp.Steam
    open FSharp.Steam.Rest
    open FSharp.Steam.Web
    open System

    let getAppBetas (appid : App) = {
        Interface = "ISteamApps";
        Method = "GetAppBetas";
        Version = 1;
        RequireKey = false;
        HttpMethod = Get;
        Parameters = [ "appid", param appid ]
    }

    let getAppBuilds (appid : App, count : uint32 option, depotDetails : bool option) = {
        Interface = "ISteamApps";
        Method = "GetAppBuilds";
        Version = 1;
        RequireKey = false;
        HttpMethod = Get;
        Parameters = [ "appid", param appid; "count", optional count; "depot_details", optional depotDetails  ]
    }

    let getAppDepotVersions (appid : App) = {
        Interface = "ISteamApps";
        Method = "GetAppDepotVersions";
        Version = 1;
        RequireKey = false;
        HttpMethod = Get;
        Parameters = [ "appid", param appid ]
    }

    let getAppList () = {
        Interface = "ISteamApps";
        Method = "GetAppList";
        Version = 1;
        RequireKey = false;
        HttpMethod = Get;
        Parameters = [ ]
    }

    let getAppList2 () = { getAppList () with Version = 2; }

    let getCheatingReports (appid : App, timeBegin : DateTimeOffset, timeEnd : DateTimeOffset, includeReports : bool, includeBans : bool, minReportId : uint64 option) = {
        Interface = "ISteamApps";
        Method = "GetCheatingReports";
        Version = 1;
        RequireKey = false;
        HttpMethod = Get;
        Parameters = 
        [
            "appid", param appid
            "timebegin", param (timeBegin.ToUnixTimeSeconds ())
            "timeend", param (timeEnd.ToUnixTimeSeconds ())
            "includereports", param includeReports
            "includebans", param includeBans
            "reportidmin", optional minReportId
        ]
    }

    let getPlayersBanned (appid : App) = {
        Interface = "ISteamApps";
        Method = "GetPlayersBanned";
        Version = 1;
        RequireKey = false;
        HttpMethod = Get;
        Parameters = [ "appid", param appid ]
    }

    let getServerList (filter : string option, limit : uint32 option) = {
        Interface = "ISteamApps";
        Method = "GetServerList";
        Version = 1;
        RequireKey = false;
        HttpMethod = Get;
        Parameters = [ "filter", optional filter; "limit", optional limit ]
    }

    type Address = 
        | IPAddress of System.Net.IPAddress 
        | IPEndPoint of System.Net.IPEndPoint

    let getServersAtAddress (address : Address) = 
        let addr = 
            match address with
                | IPAddress ip -> ip.ToString()
                | IPEndPoint endpoint -> endpoint.ToString()
        {
            Interface = "ISteamApps";
            Method = "GetServersAtAddress";
            Version = 1;
            RequireKey = false;
            HttpMethod = Get;
            Parameters = [ "addr", param addr ]
        }

    let setAppBuildLive (appid : App, buildid : uint32, betaKey : string, description : string option) = {
        Interface = "ISteamApps";
        Method = "SetAppBuildLive";
        Version = 1;
        RequireKey = false;
        HttpMethod = Post;
        Parameters = 
        [
            "appid", param appid
            "buildid", param buildid
            "betakey", param betaKey
            "description", param description            
        ]
    }

    let upToDateCheck (appid : App, version : uint32) = {
        Interface = "ISteamApps";
        Method = "UpToDateCheck";
        Version = 1;
        RequireKey = false;
        HttpMethod = Get;
        Parameters = [ "appid", param appid; "version", param version ]
    }