using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Steam.Rest
{
    /// <summary>
    /// A response from a sent request on a <see cref="IRestClient"/>
    /// </summary>
    public class RestResponse
    {
        public HttpStatusCode Status { get; }
        public IDictionary<string, RestHeaderValue> Headers { get; }
        public Stream Content { get; }

        public RestResponse(HttpStatusCode statusCode, IDictionary<string, RestHeaderValue> headers, Stream content)
        {
            Status = statusCode;
            Headers = headers;
            Content = content;
        }
    }
}
