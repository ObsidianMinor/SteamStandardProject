namespace Steam.Rest
{
    public abstract class SteamRestConfig : SteamConfig
    {
        public int DefaultRequestTimeout { get; set; } = 15000;

        public RetryMode DefaultRetryMode { get; set; }

        public RestClientProvider RestClient { get; set; } = () => new DefaultRestClient();
    }
}
