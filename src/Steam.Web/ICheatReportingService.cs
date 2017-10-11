using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Steam.Rest;

namespace Steam.Web
{
    [WebInterface(IsService = true)]
    public interface ICheatReportingService
    {
        /// <summary>
        /// Designed to gather community reports of cheating, where one player reports another player within the game.
        /// </summary>
        /// <param name="steamid">The Steam ID of the user who is being reported for cheating.</param>
        /// <param name="appid">The App ID for the game.</param>
        /// <param name="steamidreporter">(Optional) The Steam ID of the user or game server who is reporting the cheating.</param>
        /// <param name="appdata">(Optional) App specific data about the type of cheating set by developer. (ex 1 = Aimbot, 2 = Wallhack, 3 = Griefing)</param>
        /// <param name="heuristic">(Optional) Extra information about the source of the cheating - was it a heuristic.</param>
        /// <param name="detection">(Optional) Extra information about the source of the cheating - was it a detection.</param>
        /// <param name="playerreport">(Optional) Extra information about the source of the cheating - was it a player report.</param>
        /// <param name="noreportid">(Optional) Dont return reportid. This should only be passed if you don't intend to issue a ban based on this report.</param>
        /// <param name="gamemode">(Optional) Extra information about state of game - was it a specific type of game play or game mode. (0 = generic)</param>
        /// <param name="suspicionstarttime">(Optional) Extra information indicating how far back the game thinks is interesting for this user.</param>
        /// <param name="severity">(Optional) Level of severity of bad action being reported. Scale set by developer.</param>
        /// <returns></returns>
        [WebMethod(Method = HttpMethod.Post, Name = "ReportPlayerCheating", RequireKey = true)]
        Task<JToken> ReportPlayerCheatingAsync(
            SteamId steamid,
            long appid,
            SteamId? steamidreporter = null,
            decimal? appdata = null,
            bool? heuristic = null,
            bool? detection = null,
            bool? playerreport = null,
            bool? noreportid = null,
            long? gamemode = null,
            DateTimeOffset? suspicionstarttime = null,
            long? severity = null);

        /// <summary>
        /// Requests a game ban on a specific player. This is designed to be used after the incidents from <see cref="ReportPlayerCheating"/> have been reviewed and cheating has been confirmed.
        /// </summary>
        /// <param name="steamid">Steam ID of the user who is reported as cheating.</param>
        /// <param name="appid">The appid of the game.</param>
        /// <param name="reportid">The reportid originally used to report cheating.</param>
        /// <param name="cheatdescription">Text describing cheating infraction.</param>
        /// <param name="duration">Ban duration requested in seconds. (duration 0 will issue infinite - less than a year is a suspension and not visible on profile)</param>
        /// <param name="delayban">Delay the ban according to default ban delay rules.</param>
        /// <param name="flags">Additional information about the ban request. (Unused)</param>
        /// <returns></returns>
        [WebMethod(Method = HttpMethod.Post, Name = "RequestPlayerGameBan", RequireKey = true)]
        Task<JToken> RequestPlayerGameBanAsync(SteamId steamid, long appid, decimal reportid, string cheatdescription, TimeSpan duration, bool delayban, long flags);

        /// <summary>
        /// Remove a game ban on a player. This is used if a Game ban is determined to be a false positive.
        /// </summary>
        /// <param name="steamid">The Steam ID of the user to remove the game ban on.</param>
        /// <param name="appid">The App ID of the game.</param>
        /// <returns></returns>
        [WebMethod(Method = HttpMethod.Post, Name = "RemovePlayerGameBan", RequireKey = true)]
        Task<JToken> RemovePlayerGameBanAsync(SteamId steamid, long appid);

        // todo: finish
    }
}
