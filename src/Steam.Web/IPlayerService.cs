using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Steam.Web
{
    [WebInterface(IsService = true)]
    public interface IPlayerService
    {
        Task<JToken> RecordOfflinePlaytime(SteamId steamid, string ticket, JToken play_sessions);
        Task<JToken> GetRecentlyPlayedGames(SteamId steamId, long count);
        Task<JToken> GetOwnedGames(SteamId steamid, bool include_appinfo, bool include_played_free_games, long appids_filter);
        Task<JToken> GetSteamLevel(SteamId steamid);
        Task<JToken> GetBadges(SteamId steamid);
        Task<JToken> GetCommunityBadgeProgress(SteamId steamid, int badgeid);
        Task<JToken> IsPlayingSharedGame(SteamId steamid, long appid_playing);
    }
}
