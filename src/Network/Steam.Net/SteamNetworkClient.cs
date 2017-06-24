using Steam.Common;
using Steam.Common.Logging;
using Steam.Community;
using Steam.Web;
using System;
using System.Threading.Tasks;

namespace Steam.Net
{
    public partial class SteamNetworkClient : SteamWebClient, ICommunityClient
    {
        internal new readonly SteamNetworkApiClient ApiClient; // severe lack of private protected
        
        public event EventHandler<LogMessage> Log
        {
            add => ApiClient.LogEvent += value;
            remove => ApiClient.LogEvent -= value;
        }

        public SteamNetworkClient() : this(new SteamNetworkConfig()) { }
        
        public SteamNetworkClient(SteamNetworkConfig config) : base(new SteamNetworkApiClient(config, new LogManager("Client", config.LogLevel)))
        {
            ApiClient = base.ApiClient as SteamNetworkApiClient;
            ApiClient.State.CurrentUser = SelfUser.Create(this, config.DefaultUniverse);
        }

        public async Task StartAsync() => await ApiClient.StartAsync().ConfigureAwait(false);
        public async Task StopAsync() => await ApiClient.StopAsync().ConfigureAwait(false);
        
        public async Task LoginAsync(string username, string password, string authCode = null, string twoFactorCode = null, uint? loginId = null, bool rememberPassword = false, bool requestSteam2Ticket = false, byte[] sentryFileHash = null, OsType osType = OsType.Unknown, string language = "english")
        {
            await ApiClient.LoginAsync(username, password, loginId, authCode, twoFactorCode, null, rememberPassword, requestSteam2Ticket, sentryFileHash, osType, language, 0, AccountType.Individual).ConfigureAwait(false);
        }

        public async Task LoginAsync(string username, string loginKey, uint? loginId = null, bool requestSteam2Ticket = false, OsType osType = OsType.Unknown, string language = "english")
        {
            await ApiClient.LoginAsync(username, null, loginId, null, null, loginKey, true, requestSteam2Ticket, null, osType, language, 0, AccountType.Individual).ConfigureAwait(false);
        }

        public async Task LoginAnonymousAsync(OsType osType = OsType.Unknown, string language = "english")
        {
            await ApiClient.LoginAsync(null, null, null, null, null, null, null, false, null, osType, language, 0, AccountType.AnonUser).ConfigureAwait(false);
        }

        public async Task LoginConsoleAsync(uint accountId, string language = "english")
        {
            await ApiClient.LoginAsync(null, null, null, null, null, null, null, false, null, OsType.PS3, language, accountId, AccountType.Individual).ConfigureAwait(false);
        }

        public async Task<PicsChanges> GetPicsChangesSince(uint changeNumber, bool changesForApps = true, bool changesForPackages = true)
        {
            return await ApiClient.GetPicsChangesAsync(changeNumber, changesForApps, changesForPackages).ConfigureAwait(false);
        }

        /// <summary>
        /// Continues a denied login request using the provided code as an auth or two factor code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public Task ContinueLoginAsync(string code) => ApiClient.SetLoginContinuationAsync(code);
    }
}
