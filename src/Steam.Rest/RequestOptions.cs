using System.Threading;

namespace Steam.Rest
{
    public class RequestOptions
    {
        public int RequestTimeout { get; set; } = -1;

        public RetryMode RetryMode { get; set; } = RetryMode.UseDefault;

        public CancellationToken CancellationToken { get; set; }
        
        internal RequestOptions CloneWithConfig(SteamRestConfig config)
        {
            RequestOptions options = MemberwiseClone() as RequestOptions;
            if (options.RequestTimeout <= 0)
                options.RequestTimeout = config.DefaultRequestTimeout;

            if (options.RetryMode == RetryMode.UseDefault)
                options.RetryMode = config.DefaultRetryMode;

            return options;
        }
    }
}
