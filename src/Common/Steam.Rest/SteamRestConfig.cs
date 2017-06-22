using Steam.Common;
using System;

namespace Steam.Rest
{
    public abstract class SteamRestConfig : SteamConfig
    {
        public abstract Uri BaseUri { get; }

        public int DefaultRequestTimeout { get; set; } = 15000;

        public RetryMode DefaultRetryMode { get; set; }

        public RestClientProvider RestClient { get; set; } = uri => new DefaultRestClient(uri);
    }
}
