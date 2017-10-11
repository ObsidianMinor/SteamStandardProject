using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;

namespace Steam.Rest
{
    /// <summary>
    /// Represents a request to send on a <see cref="IRestClient"/>
    /// </summary>
    public class RestRequest
    {
        /// <summary>
        /// The method to use
        /// </summary>
        public HttpMethod Method { get; }

        /// <summary>
        /// The URI to request
        /// </summary>
        public Uri RequestUri { get; }
        
        /// <summary>
        /// The content to send
        /// </summary>
        public HttpContent Content { get; }

        /// <summary>
        /// The headers unique to this request
        /// </summary>
        public ImmutableDictionary<string, RestHeaderValue> RequestHeaders { get; }
        
        public RestRequest(HttpMethod method, Uri uri, HttpContent content, IDictionary<string, string> headers)
        {
            Method = method;
            RequestUri = uri;
            Content = content;
            RequestHeaders = headers?.ToImmutableDictionary(kv => kv.Key, kv => new RestHeaderValue(kv.Value)) ?? ImmutableDictionary<string, RestHeaderValue>.Empty;
        }

        public RestRequest(HttpMethod method, Uri uri, HttpContent content, IDictionary<string, RestHeaderValue> headers)
        {
            Method = method;
            RequestUri = uri;
            Content = content;
            RequestHeaders = headers?.ToImmutableDictionary() ?? ImmutableDictionary<string, RestHeaderValue>.Empty;
        }
    }
}
