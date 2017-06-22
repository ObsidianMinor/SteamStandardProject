using Newtonsoft.Json;
using Steam.Web.API.Responses;
using Steam.Rest;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System;
using System.IO;

namespace Steam.Web.API
{
    internal class SteamWebApiClient : RestApiClient
    {
        readonly string _key;

        internal SteamWebApiClient(SteamWebConfig config) : base(config)
        {
            _key = config.WebApiKey;
        }

        private static string CreateEndpoint(string interfaceName, string method, int version, params (string, object)[] args)
        {
            return $"{interfaceName}/{method}/v{version.ToString("D4")}/{Utils.CreateQueryString(args)}";
        }

        #region ICSGOServers_730

        internal async Task GetCsgoServersStatus(RequestOptions options)
        {
            await SendAsync("GET", CreateEndpoint("ICSGOServers_730", "GetGameServersStatus", 1, ("key", _key)), options).ConfigureAwait(false);
        }
        
        #endregion

        #region ISteamDirectory

        internal async Task<IReadOnlyCollection<IPEndPoint>> GetConnectionManagerList(uint cellId, uint? maxCount, RequestOptions options)
        {
            var cmList = await SendAsync<ConnectionManagerListResponse>("GET", CreateEndpoint("ISteamDirectory", "GetCMList", 1, ("cellid", cellId), ("maxcount", maxCount)), options).ConfigureAwait(false);
            return cmList.Endpoints
                .Select(e => e.Split(new[] { ':' }, 2))
                .Select(endpoint => (IPAddress.TryParse(endpoint.ElementAtOrDefault(0), out IPAddress address) && int.TryParse(endpoint.ElementAtOrDefault(1), out int portValue)) ? new IPEndPoint(address, portValue) : null)
                .Where(endpoint => endpoint != null)
                .ToList();
        }

        internal async Task<IReadOnlyCollection<string>> GetWebSocketConnectionManagerList(uint cellId, uint? maxCount, RequestOptions options)
        {
            var cmList = await SendAsync<ConnectionManagerListResponse>("GET", CreateEndpoint("ISteamDirectory", "GetCMList", 1, ("cellid", cellId), ("maxcount", maxCount)), options).ConfigureAwait(false);
            return cmList.WebSocketEndpoints;
        }

        #endregion

        #region ITwoFactorService

        internal async Task<DateTimeOffset> GetServerTime(RequestOptions options)
        {
            var serverTime = await SendAsync<ServerTimeResponse>("POST", CreateEndpoint("ITwoFactorService", "QueryTime", 1), options).ConfigureAwait(false);
            return DateTimeOffset.FromUnixTimeSeconds(serverTime.ServerTime);
        }

        protected override T ReadAsType<T>(RestResponse response)
        {
            using (StreamReader reader = new StreamReader(response.Content))
            {
                JsonResponse<T> jsonResponse = JsonConvert.DeserializeObject<JsonResponse<T>>(reader.ReadToEnd());
                return jsonResponse.Response;
            }
        }

        #endregion
    }
}
