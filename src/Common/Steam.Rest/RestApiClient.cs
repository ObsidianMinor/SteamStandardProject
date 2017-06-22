using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Rest
{
    public abstract class RestApiClient
    {
        protected SteamRestConfig Config { get; }
        protected IRestClient RestClient;

        public RestApiClient(SteamRestConfig restConfig)
        {
            Config = restConfig;
            RestClient = Config.RestClient(Config.BaseUri); 
        }

        protected abstract T ReadAsType<T>(RestResponse response);

        public async Task<T> SendAsync<T>(string method, string endpoint, RequestOptions options)
        {
            options = options ?? new RequestOptions { CancellationToken = default(CancellationToken), RequestTimeout = Config.DefaultRequestTimeout, RetryMode = Config.DefaultRetryMode };
            RestResponse response = await SendAsync(method, endpoint, options).ConfigureAwait(false);
            return ReadAsType<T>(response);
        }

        public async Task<RestResponse> SendAsync(string method, string endpoint, RequestOptions options)
        {
            options = options ?? new RequestOptions { CancellationToken = default(CancellationToken), RequestTimeout = Config.DefaultRequestTimeout, RetryMode = Config.DefaultRetryMode };
            return await SendInternalAsync(() => RestClient.SendAsync(method, endpoint, options.CancellationToken), options).ConfigureAwait(false);
        }

        public async Task<RestResponse> SendAsync(string method, string endpoint, string content, RequestOptions options)
        {
            options = options ?? new RequestOptions { CancellationToken = default(CancellationToken), RequestTimeout = Config.DefaultRequestTimeout, RetryMode = Config.DefaultRetryMode };
            return await SendInternalAsync(() => RestClient.SendAsync(method, endpoint, content, options.CancellationToken), options).ConfigureAwait(false);
        }

        private async Task<RestResponse> SendInternalAsync(Func<Task<RestResponse>> taskFunction, RequestOptions options)
        {
            options.CancellationToken.ThrowIfCancellationRequested();
            
            while(true)
            {
                RestResponse response = null;
                Task timeoutTask = Task.Delay(options.RequestTimeout, options.CancellationToken);
                Task<RestResponse> responseTask = taskFunction();
                Task returnTask = await Task.WhenAny(timeoutTask, responseTask).ConfigureAwait(false);
                if (returnTask == timeoutTask)
                {
                    if (options.RetryMode.HasFlag(RetryMode.RetryTimeouts))
                        continue;
                    else
                        throw new TimeoutException("The request timed out");
                }

                response = responseTask.Result;

                switch(response.Status)
                {
                    case var successCode when (successCode >= (HttpStatusCode)200 && successCode <= (HttpStatusCode)299):
                        return response;
                    case HttpStatusCode.BadGateway when (options.RetryMode.HasFlag(RetryMode.RetryBadGateway)):
                        continue;
                    default:
                        throw new HttpException(response.Status, response.Status.ToString());
                }
            }
        }
    }
}
