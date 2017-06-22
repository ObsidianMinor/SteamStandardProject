using Steam.KeyValues;
using System;
using System.Collections.Generic;

namespace Steam.Web.API.Responses
{
    internal class CsgoGameServersStatusResponse
    {
        [KeyValueProperty("app")]
        internal AppInfo App { get; set; }
        [KeyValueProperty("services")]
        internal Services Services { get; set; }
        [KeyValueProperty("datacenters")]
        internal Dictionary<string, Datacenter> Datacenters { get; set; }
        [KeyValueProperty("matchmaking")]
        internal Matchmaking Matchmaking { get; set; }
    }

    internal class AppInfo
    {
        [KeyValueProperty("version")]
        internal int Version { get; set; }
        [KeyValueProperty("timestamp")]
        internal DateTimeOffset Timestamp { get; set; }
        [KeyValueProperty("time")]
        internal DateTime Timme { get; set; }
    }

    internal class Services
    {
        [KeyValueProperty("SessionsLogon")]
        internal ServiceAvailablity SessionsLogon { get; set; }
        [KeyValueProperty("SteamCommunity")]
        internal ServiceAvailablity SteamCommunity { get; set; }
        [KeyValueProperty("IEconItems")]
        internal ServiceAvailablity EconomyItems { get; set; }
        [KeyValueProperty("Leaderboards")]
        internal ServiceAvailablity Leaderboards { get; set; }
    }
    
    internal class Matchmaking
    {
        [KeyValueProperty("scheduler")]
        internal ServiceAvailablity Scheduler { get; set; }
        [KeyValueProperty("online_servers")]
        internal int OnlineServers { get; set; }
        [KeyValueProperty("online_players")]
        internal int OnlinePlayers { get; set; }
        [KeyValueProperty("searching_players")]
        internal int SearchingPlayers { get; set; }
        [KeyValueProperty("search_seconds_avg")]
        internal int SearchSecondsAverage { get; set; }
    }
    
    internal class Datacenter
    {
        [KeyValueProperty("capacity")]
        internal string Capacity { get; set; }
        [KeyValueProperty("load")]
        internal ServerLoad Load { get; set; }
    }
}
