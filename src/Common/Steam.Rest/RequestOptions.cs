using System.Net;
using System.Net.Http.Headers;
using System.Threading;

namespace Steam.Rest
{
    public class RequestOptions
    {
        public int RequestTimeout { get; set; }

        public RetryMode RetryMode { get; set; }

        public CancellationToken CancellationToken { get; set; }
    }
}
