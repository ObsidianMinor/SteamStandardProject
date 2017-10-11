using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Rest
{
    /// <summary>
    /// Provides access to methods to send data via a REST client
    /// </summary>
    public interface IRestClient
    {
        Task<RestResponse> SendAsync(RestRequest request, CancellationToken token);

        void SetHeader(string key, params string[] values);
        void SetCookie(Uri uri, Cookie cookie);
    }
}
