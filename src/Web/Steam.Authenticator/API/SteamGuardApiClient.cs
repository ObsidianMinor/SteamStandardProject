using System;
using Steam.Rest;

namespace Steam.Authenticator.API
{
    internal class SteamGuardApiClient : RestApiClient
    {
        const string _steamApi = "https://api.steampowered.com";
        const string _steamCommunity = "https://steamcommunity.com";
        
        public SteamGuardApiClient(SteamRestConfig restConfig) : base(restConfig)
        {
            RestClient.SetHeader("UserAgent", "Mozilla/5.0 (Linux; U; Android 4.1.1; en-us; Google Nexus 4 - 4.1.1 - API 16 - 768x1280 Build/JRO03S) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30");
        }

        protected override T ReadAsType<T>(RestResponse response)
        {
            throw new NotImplementedException();
        }
    }
}
