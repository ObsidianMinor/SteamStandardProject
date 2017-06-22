using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Steam.Rest
{
    public class RestResponse
    {
        public HttpStatusCode Status { get; }
        public Dictionary<string, IEnumerable<string>> Headers { get; }
        public Stream Content { get; }

        public RestResponse(HttpStatusCode statusCode, Dictionary<string, IEnumerable<string>> headers, Stream content)
        {
            Status = statusCode;
            Headers = headers;
            Content = content;
        }
    }
}
