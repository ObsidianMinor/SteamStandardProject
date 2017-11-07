using System;

namespace Steam.Rest
{
    /// <summary>
    /// Indicates an request didn't return a successful status code
    /// </summary>
    public class HttpException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="HttpException"/> with the specifed <see cref="RestResponse"/>
        /// </summary>
        /// <param name="response"></param>
        public HttpException(RestResponse response) : base("The response returned didn't contain a success status code")
        {
            Response = response;
        }

        /// <summary>
        /// Gets the response that created this exception
        /// </summary>
        public RestResponse Response { get; }

        public override string ToString()
        {
            return $"{(int)Response.Status}: {Response.Status}";
        }
    }
}
