module ISteamWebAPIUtil
    
    open FSharp.Steam.Rest
    open FSharp.Steam.Web
    
    let getServerInfo = {
        Interface = "ISteamWebAPIUtil";
        Method = "GetServerInfo";
        Version = 1;
        HttpMethod = Get;
        RequireKey = false;
        Parameters = [ ]
    }

    let getSupportedAPIList = {
        Interface = "ISteamWebAPIUtil";
        Method = "GetSupportedAPIList";
        Version = 1;
        HttpMethod = Get;
        RequireKey = false;
        Parameters = [ ]
    }

    let getFullAPIList = { getSupportedAPIList with RequireKey = true; }
