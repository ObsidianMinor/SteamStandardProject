using Steam.Rest;
using Steam.Web.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steam.Web
{
    /// <summary>
    /// Provides a client with access to the Steam Web API
    /// </summary>
    public class SteamWebClient : SteamRestClient
    {
        private readonly ConcurrentDictionary<Type, object> _interfaces = new ConcurrentDictionary<Type, object>();
        private readonly WebInvoker _invoker;
        private readonly IWebInterfaceContractResolver _contractResolver;

        public SteamWebClient() : this(new SteamWebConfig()) { }

        public SteamWebClient(string key) : this(new SteamWebConfig { WebApiKey = key }) { }

        public SteamWebClient(SteamWebConfig config) : base(config)
        {
            config = GetConfig<SteamWebConfig>();
            if (config.InvokerProvider == null)
                _invoker = new WebInvoker(this);
            else
                _invoker = config.InvokerProvider(this) ?? new WebInvoker(this);

            if (config.ResolverProvider == null)
                _contractResolver = DefaultWebInterfaceResolver.Instance;
            else
                _contractResolver = config.ResolverProvider() ?? DefaultWebInterfaceResolver.Instance;
        }

        /// <summary>
        /// Gets a Steam Web interface of the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetInterface<T>() => (T)_interfaces.GetOrAdd(typeof(T), (t) => WebDispatchProxy.Create<T>(_contractResolver, _invoker));
        
        protected internal async Task<RestResponse> SendAsync(HttpMethod httpMethod, string interfaceName, string method, int version, bool requireKey, RequestOptions options = null, params (string, string)[] parameters)
        {
            SteamWebConfig config = GetConfig<SteamWebConfig>();
            Uri newUri = new Uri(config.ApiUri, $"{interfaceName}/{method}/v{version}/{Utils.CreateQueryString(config.Format, requireKey && !string.IsNullOrWhiteSpace(config.WebApiKey) ? config.WebApiKey : null, parameters)}");
            RestRequest request = new RestRequest(httpMethod, newUri, null, default(IDictionary<string,string>));
            return await SendAsync(request, options).ConfigureAwait(false);
        }
    }
}