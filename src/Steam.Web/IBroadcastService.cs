using System.Threading.Tasks;
using Steam.Rest;

namespace Steam.Web
{
    [WebInterface(IsService = true)]
    public interface IBroadcastService
    {
        [WebMethod(Method = HttpMethod.Post, RequireKey = true)]
        Task PostGameDataFrame(long appid, SteamId steamid, decimal broadcast_id, string frame_data);
    }
}
