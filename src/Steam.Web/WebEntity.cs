using System.Threading.Tasks;
using Steam.Rest;

namespace Steam.Web
{
    public abstract class WebEntity<T> : RestEntity<T> where T : SteamWebClient
    {
        protected WebEntity(T client) : base(client)
        {
        }

        protected Task<RestResponse> SendAsync(HttpMethod httpMethod, string interfaceName, string method, int version, bool requireKey, RequestOptions options = null, params (string, string)[] parameters)
            => Client.SendAsync(httpMethod, interfaceName, method, version, requireKey, options, parameters);
    }
}
