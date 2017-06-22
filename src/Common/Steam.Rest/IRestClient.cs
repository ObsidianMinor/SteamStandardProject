using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Rest
{
    public interface IRestClient
    {
        Task<RestResponse> SendAsync(string method, string endpoint, CancellationToken token);
        Task<RestResponse> SendAsync(string method, string endpoint, string content, CancellationToken token);
        Task<RestResponse> SendAsync(string method, string endpoint, Stream content, CancellationToken token);

        void SetHeader(string key, params string[] values);
        void AddCookie(Uri uri, Cookie cookie);
    }
}
