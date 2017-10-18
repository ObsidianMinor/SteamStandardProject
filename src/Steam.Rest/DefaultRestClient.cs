using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Rest
{
    internal sealed class DefaultRestClient : IRestClient, IDisposable
    {
        readonly HttpClientHandler _handler;
        readonly HttpClient _client;

        public DefaultRestClient()
        {
            _handler = new HttpClientHandler()
            {
                UseProxy = false,
                UseCookies = true
            };
            _client = new HttpClient(_handler);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
        
        public async Task<RestResponse> SendAsync(RestRequest request, CancellationToken token)
        {
            HttpRequestMessage message = CreateRequest(request);
            HttpResponseMessage response = await _client.SendAsync(message, token).ConfigureAwait(false);
            return await CreateResponse(response).ConfigureAwait(false);
        }

        public void SetCookie(Uri uri, Cookie cookie)
        {
            _handler.CookieContainer.Add(uri, cookie);
        }

        public void SetHeader(string key, params string[] values)
        {
            _client.DefaultRequestHeaders.Remove(key);
            if (values != null)
                _client.DefaultRequestHeaders.Add(key, values);
        }

        private async Task<RestResponse> CreateResponse(HttpResponseMessage response)
        {
            return new RestResponse(response.StatusCode, response.Headers.ToDictionary(k => k.Key, k => new RestHeaderValue(k.Value)), await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
        }

        private HttpRequestMessage CreateRequest(RestRequest request)
        {
            HttpRequestMessage message = new HttpRequestMessage(new System.Net.Http.HttpMethod(Enum.GetName(typeof(HttpMethod), request.Method).ToUpper()), request.RequestUri);
            foreach (var header in request.RequestHeaders)
            {
                message.Headers.Remove(header.Key);

                List<string> headers = header.Value.ToList();
                if (headers.Count > 0)
                    message.Headers.Add(header.Key, headers);
            }

            if (request.Content != null)
                message.Content = request.Content;

            return message;
        }
    }
}
