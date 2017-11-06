using System.Threading.Tasks;

namespace Steam.Rest
{
    public abstract class RestEntity<T> : Entity<T> where T : SteamRestClient
    {
        protected RestEntity(T client) : base(client)
        {
        }

        protected Task<RestResponse> SendAsync(RestRequest request, RequestOptions options) => Client.SendAsync(request, options);
    }
}
