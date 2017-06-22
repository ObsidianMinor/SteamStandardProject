using System;
using System.Net;

namespace Steam.Rest
{
    public class HttpException : Exception
    {
        public HttpException(HttpStatusCode status, string message) : base("The response returned didn't contain a success status code")
        {
            StatusCode = status;
            StatusMessage = message;
        }

        public HttpStatusCode StatusCode { get; }

        public string StatusMessage { get; }

        public override string ToString()
        {
            return $"{(int)StatusCode}: {StatusMessage}";
        }
    }
}
