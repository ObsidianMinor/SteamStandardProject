using Steam.Web.API;
using Steam.Rest;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System;
using Steam.Common;
using System.Linq;

namespace Steam.Web
{
    public class SteamWebClient
    {
        internal readonly SteamWebApiClient ApiClient;

        /// <summary>
        /// Initializes this <see cref="SteamWebClient"/> without a API key. The key will need to be set to access endpoints which require it.
        /// </summary>
        public SteamWebClient() : this(new SteamWebConfig())
        {
        }

        /// <summary>
        /// Initializes this <see cref="SteamWebClient"/> with an API key.
        /// </summary>
        /// <param name="apiKey">The API key</param>
        public SteamWebClient(string apiKey) : this(new SteamWebConfig { WebApiKey = apiKey })
        {
        }

        public SteamWebClient(SteamWebConfig config) : this(new SteamWebApiClient(config))
        {
        }

        internal SteamWebClient(SteamWebApiClient apiClient)
        {
            ApiClient = apiClient;
        }

        /// <summary>
        /// Gets the list of endpoints for Steam connection managers in the provided cell Id
        /// </summary>
        /// <param name="cellId"></param>
        /// <param name="maxCount"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<IReadOnlyCollection<IPEndPoint>> GetConnectionManagerEndpointsAsync(uint cellId, uint? maxCount = null, RequestOptions options = null)
        {
            return await ApiClient.GetConnectionManagerList(cellId, maxCount, options).ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<string>> GetWebSocketConnectionManagerEndpointsAsync(uint cellId, uint? maxCount = null, RequestOptions options = null)
        {
            return await ApiClient.GetWebSocketConnectionManagerList(cellId, maxCount, options).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the time of the Steam servers
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<DateTimeOffset> GetServerTimeAsync(RequestOptions options = null)
        {
            return await ApiClient.GetServerTime(options).ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<UserProfile>> GetPlayerSummariesAsync(IEnumerable<SteamId> steamIds, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the first game server at the specified address
        /// </summary>
        /// <param name="gameServerIp"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<GameServer> GetServerAtAddressAsync(IPEndPoint gameServerIp, RequestOptions options = null)
        {
            return (await GetServersAtAddressAsync(gameServerIp, options).ConfigureAwait(false)).FirstOrDefault();
        }

        /// <summary>
        /// Returns all game servers at the specified address
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<IReadOnlyCollection<GameServer>> GetServersAtAddressAsync(IPEndPoint endpoint, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a dictionary of app IDs and their respective app names
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<Dictionary<uint, string>> GetAppListAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Resolves a user profile vanity url
        /// </summary>
        /// <param name="vanityUrl"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<SteamId> ResolveVanityUrlAsync(string vanityUrl, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public static Uri GetAvatarImageUrl(string hash, ImageSize size)
        {
            return GetCdnImageUrl("avatar", hash.Substring(0, 2), hash, size);
        }

        public static Uri GetItemImageUrl(uint appId, string hash)
        {
            return GetCdnImageUrl("items", appId.ToString(), hash, ImageSize.Small); // say small because there is no other sizes for items
        }

        private const string cdnUrl = "http://cdn.akamai.steamstatic.com/steamcommunity/public/images";
        private static readonly Uri cdnUri = new Uri(cdnUrl);
        private static Uri GetCdnImageUrl(string type, string sub, string hash, ImageSize size)
        {
            string imageSizeString = size == ImageSize.Small ? "" : "_" + size.ToString().ToLower();
            return new Uri(cdnUri, $"/{type}/{sub}/{hash}{imageSizeString}.jpg");
        }
    }
}
