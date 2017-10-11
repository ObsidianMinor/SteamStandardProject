using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Rest
{
    /// <summary>
    /// Provides a base class for interacting with Steam's REST APIs
    /// </summary>
    public abstract class SteamRestClient : SteamClient
    {
        private IRestClient _client;

        public SteamRestClient(SteamRestConfig config) : base(config)
        {
            var cloneConfig = GetConfig<SteamRestConfig>();
            if (cloneConfig.RestClient != null)
            {
                var client = cloneConfig.RestClient();
                _client = client ?? new DefaultRestClient();
            }
            else
                _client = new DefaultRestClient();
        }
        
        protected internal async Task<RestResponse> SendAsync(RestRequest request, RequestOptions options)
        {
            options = options?.CloneWithConfig(GetConfig<SteamRestConfig>()) ?? new RequestOptions().CloneWithConfig(GetConfig<SteamRestConfig>());

            while (!options.CancellationToken.IsCancellationRequested)
            {
                CancellationTokenSource linkedTimeout = CancellationTokenSource.CreateLinkedTokenSource(options.CancellationToken);

                Task<RestResponse> responseTask = _client.SendAsync(request, linkedTimeout.Token);
                Task timeout = Task.Delay(options.RequestTimeout);
                Task result = await Task.WhenAny(responseTask, timeout).ConfigureAwait(false);

                if (result == timeout)
                {
                    linkedTimeout.Cancel();
                    try
                    {
                        await responseTask.ConfigureAwait(false);
                    }
                    catch(TaskCanceledException) { }

                    if ((options.RetryMode & RetryMode.RetryTimeouts) == 0)
                        throw new TimeoutException("The request timed out");
                    else
                        continue;
                }

                RestResponse response = await responseTask;

                switch (response.Status)
                {
                    case var status when status >= HttpStatusCode.OK || status <= (HttpStatusCode)299:
                        return response;
                    case HttpStatusCode.BadGateway when (options.RetryMode & RetryMode.RetryBadGateway) != 0:
                        continue;
                    default:
                        throw new HttpException(response);
                }
            }

            throw new OperationCanceledException(options.CancellationToken);
        }
    }
}