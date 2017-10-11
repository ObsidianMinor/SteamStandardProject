using Steam.Rest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Steam.Tests.Mocks
{
    public class AssertingRestClient : IRestClient
    {
        private string uri;
        private HttpMethod method;
        private string response;

        public AssertingRestClient(HttpMethod requestMethod, string uri, string response)
        {
            this.uri = uri;
            method = requestMethod;
            this.response = response;
        }

        public Task<RestResponse> SendAsync(RestRequest request, CancellationToken token)
        {
            Assert.Equal(method, request.Method);
            Assert.Equal(new Uri(uri), request.RequestUri);
            return Task.FromResult(new RestResponse(HttpStatusCode.OK, new Dictionary<string, RestHeaderValue>(), new MemoryStream(Encoding.UTF8.GetBytes(response))));
        }

        public void SetCookie(Uri uri, Cookie cookie)
        {
            throw new NotImplementedException();
        }

        public void SetHeader(string key, params string[] values)
        {
            throw new NotImplementedException();
        }
    }
}
