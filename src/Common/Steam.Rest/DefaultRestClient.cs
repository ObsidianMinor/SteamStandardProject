using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Rest
{
    internal sealed class DefaultRestClient : IRestClient, IDisposable
    {
        readonly Uri _baseUri;
        readonly HttpClientHandler _handler;
        readonly HttpClient _client;

        public DefaultRestClient(Uri baseUri)
        {
            _baseUri = baseUri;
            _handler = new HttpClientHandler()
            {
                UseProxy = false
            };
            _client = new HttpClient();
        }

        public void Dispose()
        {
            _client.Dispose();
        }
        
        public async Task<RestResponse> SendAsync(string method, string endpoint, CancellationToken token)
        {
            using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), new Uri(_baseUri, endpoint)))
                return await SendInternalAsync(request, token).ConfigureAwait(false);
        }

        public async Task<RestResponse> SendAsync(string method, string endpoint, string content, CancellationToken token)
        {
            using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), new Uri(_baseUri, endpoint)))
            using (StringContent stream = new StringContent(content))
            {
                request.Content = stream;
                return await SendInternalAsync(request, token).ConfigureAwait(false);
            }
        }

        public async Task<RestResponse> SendAsync(string method, string endpoint, Stream content, CancellationToken token)
        {
            using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), new Uri(_baseUri, endpoint)))
            using (StreamContent stream = new StreamContent(content))
            {
                request.Content = stream;
                return await SendInternalAsync(request, token).ConfigureAwait(false);
            }
        }

        private async Task<RestResponse> SendInternalAsync(HttpRequestMessage message, CancellationToken token)
        {
            HttpResponseMessage response = await _client.SendAsync(message, token);
            return await CreateResponse(response);
        }

        public void AddCookie(Uri uri, Cookie cookie)
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
            return new RestResponse(response.StatusCode, response.Headers.ToDictionary(k => k.Key, k => k.Value), await response.Content.ReadAsStreamAsync());
        }
    }
}
