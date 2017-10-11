using System;
using System.Threading.Tasks;

namespace Steam.Rest
{
    /// <summary>
    /// Represents a object that can send data on a <see cref="SteamRestClient"/>
    /// </summary>
    public abstract class RestEntity
    {
        /// <summary>
        /// The client connected to this entity
        /// </summary>
        protected SteamRestClient Client { get; }

        protected RestEntity(SteamRestClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        protected Task<RestResponse> SendAsync(RestRequest request, RequestOptions options) => Client.SendAsync(request, options);
    }
}
