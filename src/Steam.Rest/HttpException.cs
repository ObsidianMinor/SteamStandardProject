using System;

namespace Steam.Rest
{
    /// <summary>
    /// Indicates an request didn't return a successful status code
    /// </summary>
    public class HttpException : Exception
    {
        public HttpException(RestResponse response) : base("The response returned didn't contain a success status code")
        {
            Response = response;
        }

        public RestResponse Response { get; }

        public override string ToString()
        {
            return $"{(int)Response.Status}: {Response.Status}";
        }
    }
}
