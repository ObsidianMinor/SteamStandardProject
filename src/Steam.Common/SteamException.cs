using System;

namespace Steam
{
    /// <summary>
    /// Represents a basic Steam exception
    /// </summary>
    public class SteamException : Exception
    {
        public Result ResultCode { get; set; }

        public SteamException() : this(Result.Fail) { }

        public SteamException(Result result) : this(result, "") { }

        public SteamException(string message) : this(Result.Fail, message) { }

        public SteamException(Result result, string message) : base(message)
        {
            ResultCode = result;
        }

        public SteamException(Result result, Exception innerException) : base("A Steam request did not return a successful status code", innerException)
        {
            ResultCode = result;
        }

        public SteamException(string message, Exception innerException) : base(message, innerException) { }

        public SteamException(Result result, string message, Exception innerException) : base(message, innerException)
        {
            ResultCode = result;
        }

        public static void ThrowIfNotOK(Result result, string message = null)
        {
            if (result != Result.OK)
                throw new SteamException(result, message ?? "A Steam request did not return a successful status code");
        }

        public static void ThrowIfNotOK(int result, string message = null)
        {
            if (result != 1)
                throw new SteamException((Result)result, message ?? "A Steam request did not return a successful status code");
        }
    }
}
