using System;
using Steam.Rest;

namespace Steam.Web
{
    public class SteamWebConfig : SteamRestConfig
    {
        public const string BaseUrl = "https://api.steampowered.com/";

        public string WebApiKey { get; set; }

        public override Uri BaseUri => new Uri(BaseUrl);
    }
}
