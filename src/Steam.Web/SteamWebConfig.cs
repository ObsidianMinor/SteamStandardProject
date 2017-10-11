using System;
using Steam.Rest;
using Steam.Web.Interface;

namespace Steam.Web
{
    public class SteamWebConfig : SteamRestConfig
    {
        /// <summary>
        /// The default base URL
        /// </summary>
        public const string BaseUrl = "https://api.steampowered.com/";

        /// <summary>
        /// The partners only URL
        /// </summary>
        public const string PartnerUrl = "https://partner.steam-api.com";

        /// <summary>
        /// Sets the web API key to use for web requests
        /// </summary>
        public string WebApiKey { get; set; }
        
        /// <summary>
        /// Sets the response format for all requests. The default is <see cref="ResponseFormat.Json"/> 
        /// </summary>
        public ResponseFormat Format { get; set; } = ResponseFormat.Json;

        /// <summary>
        /// Sets the base URL for all requests. The default is <see cref="BaseUrl"/>
        /// </summary>
        public Uri ApiUri { get; set; } = new Uri(BaseUrl);

        /// <summary>
        /// Sets the contract resolver for interfaces
        /// </summary>
        public Func<IWebInterfaceContractResolver> ResolverProvider { get; set; } = () => DefaultWebInterfaceResolver.Instance;

        /// <summary>
        /// Sets the invoker to run methods for dynamically created interfaces
        /// </summary>
        public Func<SteamWebClient, WebInvoker> InvokerProvider { get; set; } = (client) => new WebInvoker(client);
    }
}
