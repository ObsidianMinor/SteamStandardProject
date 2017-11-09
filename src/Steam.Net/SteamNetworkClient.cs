using Steam.Logging;
using Steam.Net.GameCoordinators;
using Steam.Net.GameCoordinators.Messages;
using Steam.Net.Messages;
using Steam.Net.Messages.Protobufs;
using Steam.Net.Messages.Structs;
using Steam.Net.Sockets;
using Steam.Net.Utilities;
using Steam.Rest;
using Steam.Web;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Net
{
    /// <summary>
    /// Provides a client with access to Steam's network and the Steam Web API
    /// </summary>
    public partial class SteamNetworkClient : SteamNetworkClientBase
    {
        private const uint _currentProtocolVer = 65579;
        private const uint _obfuscationMask = 0xBAADF00D;

        private Task _heartBeatTask;
        private CancellationTokenSource _heartbeatCancel;
        private Func<Task> _logonFunc;
        private bool _gracefulLogoff;
        private ConcurrentDictionary<ServerType, ImmutableHashSet<Server>> _servers = new ConcurrentDictionary<ServerType, ImmutableHashSet<Server>>();
        private FriendsList _friends;
        
        /// <summary>
        /// Gets the client's current session ID
        /// </summary>
        public int SessionId { get; private set; }

        /// <summary>
        /// Gets the client's current user's Steam ID
        /// </summary>
        public SteamId SteamId => CurrentUser.Id;

        /// <summary>
        /// Gets the client's current user
        /// </summary>
        public SelfUser CurrentUser { get; private set; }

        /// <summary>
        /// Gets the current user's wallet
        /// </summary>
        public Wallet Wallet { get; private set; }

        /// <summary>
        /// Gets the client's current cell ID
        /// </summary>
        public long CellId { get; private set; }

        /// <summary>
        /// Gets the client's current instance ID
        /// </summary>
        public long InstanceId { get; private set; }
        
        /// <summary>
        /// Creates a new <see cref="SteamNetworkClient"/> with the default config
        /// </summary>
        public SteamNetworkClient() : this(new SteamNetworkConfig()) { }

        /// <summary>
        /// Creates a new <see cref="SteamNetworkClient"/> with the specified config
        /// </summary>
        /// <param name="config"></param>
        public SteamNetworkClient(SteamNetworkConfig config) : base(config)
        {
            CurrentUser = new SelfUser(SteamId.CreateAnonymousUser(config.DefaultUniverse));
            
            CellId = config.CellId > uint.MaxValue || config.CellId < uint.MinValue ? 0 : config.CellId;
        }

        protected override async Task OnDisconnectingAsync(Exception ex)
        {
            if (!_gracefulLogoff)
            {
                CurrentServer = null;
            }

            await NetLog.DebugAsync("Waiting for heartbeat").ConfigureAwait(false);
            _heartbeatCancel?.Cancel();
            if (_heartBeatTask != null)
                await _heartBeatTask.ConfigureAwait(false);
            _heartBeatTask = null;
        }

        protected override async Task OnConnectedAsync()
        {
            await NetLog.DebugAsync("Performing possible login actions").ConfigureAwait(false);
            // todo: resume connections and things
            if (_logonFunc != null)
            {
                await _logonFunc().ConfigureAwait(false);
            }
            else
            {
                await Ready.InvokeAsync(this, EventArgs.Empty).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Logs into the steam network using the provided username and password
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <param name="twoFactor">The optional two factor code</param>
        /// <param name="authCode">The optional auth code</param>
        /// <param name="rememberPassword">Whether to return a login key upon logging in</param>
        /// <param name="requestSteam2Ticket">Whether to return a Steam2 ticket upon logging in</param>
        /// <returns>An awaitable task</returns>
        public async Task LoginAsync(string username, string password, string twoFactor = null, string authCode = null, bool rememberPassword = false, bool requestSteam2Ticket = false)
        {
            await LoginAsync(new LoginInfo 
            {
                Username = username,
                Password = password,
                TwoFactorCode = twoFactor,
                AuthCode = authCode,
                ShouldRememberPassword = rememberPassword,
                RequestSteam2Ticket = requestSteam2Ticket,
                AccountType = AccountType.Individual
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Logs into the Steam network using the provided username, password, and sentry file hash
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="sentryFileHash"></param>
        /// <param name="requestSteam2Ticket"></param>
        /// <returns></returns>
        public async Task LoginAsync(string username, string password, byte[] sentryFileHash, bool requestSteam2Ticket = false)
        {
            await LoginAsync(new LoginInfo
            {
                Username = username,
                Password = password,
                SentryFileHash = sentryFileHash,
                RequestSteam2Ticket = requestSteam2Ticket,
                AccountType = AccountType.Individual
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Logs into the Steam network using the provided username and login key
        /// </summary>
        /// <param name="username"></param>
        /// <param name="loginKey"></param>
        /// <param name="requestSteam2Ticket"></param>
        /// <returns></returns>
        public async Task LoginAsync(string username, string loginKey, bool requestSteam2Ticket = false)
        {
            await LoginAsync(new LoginInfo 
            { 
                Username = username, 
                LoginKey = loginKey, 
                RequestSteam2Ticket = requestSteam2Ticket,
                AccountType = AccountType.Individual,
                ShouldRememberPassword = true
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Logs into the Steam network anonymously
        /// </summary>
        /// <returns></returns>
        public async Task LoginAnonymousAsync()
        {
            await LoginAsync(new LoginInfo { AccountType = AccountType.AnonUser }).ConfigureAwait(false);
        }

        /// <summary>
        /// Logs into the Steam network as a console user using the provided account ID
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public async Task LoginConsoleAsync(long accountId)
        {
            await LoginAsync(new LoginInfo { AccountId = (uint)accountId, AccountType = AccountType.Individual }).ConfigureAwait(false);
        }

        /// <summary>
        /// Logs into the Steam network as a game server
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task LoginGameServerAsync(int appId, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("The token cannot be null or whitespace", nameof(token));

            CMsgClientLogon logon = new CMsgClientLogon()
            {
                protocol_version = _currentProtocolVer,
                obfustucated_private_ip = (uint)(GetConfig<SteamNetworkConfig>().LoginId < 0 ? LocalIp.ToUInt32() ^ _obfuscationMask : GetConfig<SteamNetworkConfig>().LoginId),
                client_os_type = (uint)HardwareUtils.GetCurrentOsType(),
                game_server_app_id = appId,
                machine_id = await HardwareUtils.GetMachineId().ConfigureAwait(false),
                game_server_token = token
            };

            NetworkMessage message = NetworkMessage
                .CreateProtobufMessage(MessageType.ClientLogonGameServer, logon)
                .WithClientInfo(new SteamId(0, GetConfig<SteamNetworkConfig>().DefaultUniverse, AccountType.GameServer, 0), 0);

            await SendAsync(message.Serialize()).ConfigureAwait(false);
        }

        /// <summary>
        /// Logs into the Steam network as an anonymous game server
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task LoginGameServerAnonymousAsync(int appId)
        {
            CMsgClientLogon logon = new CMsgClientLogon()
            {
                protocol_version = _currentProtocolVer,
                obfustucated_private_ip = (uint)(GetConfig<SteamNetworkConfig>().LoginId < 0 ? LocalIp.ToUInt32() ^ _obfuscationMask : GetConfig<SteamNetworkConfig>().LoginId),
                client_os_type = (uint)HardwareUtils.GetCurrentOsType(),
                game_server_app_id = appId,
                machine_id = await HardwareUtils.GetMachineId().ConfigureAwait(false),
            };

            NetworkMessage message = NetworkMessage
                .CreateProtobufMessage(MessageType.ClientLogon, logon)
                .WithClientInfo(SteamId.CreateAnonymousGameServer(GetConfig<SteamNetworkConfig>().DefaultUniverse), 0);

            await SendAsync(message.Serialize()).ConfigureAwait(false);
        }

        // StartAndLogin feels hacky, please post suggestions for it

        /// <summary>
        /// Starts the client and logs in using the specified username and password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="twoFactor"></param>
        /// <param name="authCode"></param>
        /// <param name="rememberPassword"></param>
        /// <param name="requestSteam2Ticket"></param>
        /// <returns></returns>
        public async Task StartAndLoginAsync(string username, string password, string twoFactor = null,
            string authCode = null, bool rememberPassword = false, bool requestSteam2Ticket = false)
        {
            await StartAsync().ConfigureAwait(false);
            _logonFunc = () => LoginAsync(username, password, twoFactor, authCode, rememberPassword, requestSteam2Ticket);
        }

        /// <summary>
        /// Starts the client and logs in using the specified username and password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="sentryFileHash"></param>
        /// <param name="requestSteam2Ticket"></param>
        /// <returns></returns>
        public async Task StartAndLoginAsync(string username, string password, byte[] sentryFileHash,
            bool requestSteam2Ticket = false)
        {
            await StartAsync().ConfigureAwait(false);
            _logonFunc = () => LoginAsync(username, password, sentryFileHash, requestSteam2Ticket);
        }

        /// <summary>
        /// Starts the client and logs in using the specified username and login key
        /// </summary>
        /// <param name="username"></param>
        /// <param name="loginKey"></param>
        /// <param name="requestSteam2Ticket"></param>
        /// <returns></returns>
        public async Task StartAndLoginAsync(string username, string loginKey, bool requestSteam2Ticket = false)
        {
            await StartAsync().ConfigureAwait(false);
            _logonFunc = () => LoginAsync(username, loginKey, requestSteam2Ticket);
        }

        /// <summary>
        /// Starts the client and logs in anonymously
        /// </summary>
        /// <returns></returns>
        public async Task StartAndLoginAnonymousAsync()
        {
            await StartAsync().ConfigureAwait(false);
            _logonFunc = LoginAnonymousAsync;
        }

        /// <summary>
        /// Starts the client and logs in as a console
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public async Task StartAndLoginConsoleAsync(long accountId)
        {
            await StartAsync().ConfigureAwait(false);
            _logonFunc = () => LoginConsoleAsync(accountId);
        }

        /// <summary>
        /// Starts the client and logs in as a game server
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task StartAndLoginGameServerAsync(int appId, string token)
        {
            await StartAsync().ConfigureAwait(false);
            _logonFunc = () => LoginGameServerAsync(appId, token);
        }

        /// <summary>
        /// Starts the client and logs in as an anonymous game server
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task StartAndLoginGameServerAnonymousAsync(int appId)
        {
            await StartAsync().ConfigureAwait(false);
            _logonFunc = () =>  LoginGameServerAnonymousAsync(appId);
        }

        private async Task LoginAsync(LoginInfo info)
        {
            uint instance = 0;
            if (info.AccountId != 0)
                instance = 2;
            else if (info.AccountType != AccountType.AnonUser)
                instance = 1;

            await NetLog.InfoAsync($"Logging in as {info.Username ?? (instance == 0 ? "an anonymous user" : "a console user")}").ConfigureAwait(false);
            byte[] machineId = await HardwareUtils.GetMachineId().ConfigureAwait(false); // while we set up the logon object, we will start to get the machine ID

            var body = new CMsgClientLogon
            {
                protocol_version = 65579,
                client_os_type = (uint)(info.AccountId == 0 ? HardwareUtils.GetCurrentOsType() : OsType.PS3),
                client_language = GetConfig<SteamNetworkConfig>().Language.GetApiLanguageCode(),
                cell_id = (uint)GetConfig<SteamNetworkConfig>().CellId,
            };

            if (machineId != null && machineId.Length != 0)
                body.machine_id = machineId;

            if (info.AccountType != AccountType.AnonUser)
            {
                body.account_name = info.Username;
                body.password = info.Password;
                body.should_remember_password = info.ShouldRememberPassword;
                body.steam2_ticket_request = info.RequestSteam2Ticket;
                body.two_factor_code = info.TwoFactorCode;
                body.auth_code = info.AuthCode;
                body.login_key = info.LoginKey;
                body.sha_sentryfile = info.SentryFileHash;
                body.eresult_sentryfile = (int)(info.SentryFileHash is null ? Result.FileNotFound : Result.OK);
                body.client_package_version = 1771;
                body.obfustucated_private_ip = (uint)(GetConfig<SteamNetworkConfig>().LoginId < 0 ? LocalIp.ToUInt32() ^ _obfuscationMask : GetConfig<SteamNetworkConfig>().LoginId);
                body.supports_rate_limit_response = true;
            }

            var logon = NetworkMessage
                .CreateProtobufMessage(MessageType.ClientLogon, body)
                .WithClientInfo(new SteamId(info.AccountId, GetConfig<SteamNetworkConfig>().DefaultUniverse, info.AccountType, instance), 0);

            await SendAsync(logon.Serialize()).ConfigureAwait(false);
        }
        
        /// <summary>
        /// Gets servers of the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IReadOnlyCollection<Server> GetServers(ServerType type)
        {
            return _servers.ContainsKey(type) ? _servers[type] : ImmutableHashSet<Server>.Empty;
        }

        /// <summary>
        /// Gets a new Steam Web API nonce 
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetWebUserNonceKey()
        {
            var nonce = await SendJobAsync<CMsgClientRequestWebAPIAuthenticateUserNonceResponse>(
                NetworkMessage.CreateProtobufMessage(MessageType.ClientRequestWebAPIAuthenticateUserNonce, null)).ConfigureAwait(false);

            SteamException.ThrowIfNotOK(nonce.eresult, "The request for a Web API user nonce key did not complete succesfully");
            return nonce.webapi_authenticate_user_nonce;
        }

        /// <summary>
        /// Requests a validation email to be sent for the current account
        /// </summary>
        /// <returns></returns>
        public async Task<Result> RequestValidationEmailAsync()
        {
            GenericResponse response = await SendJobAsync<GenericResponse>(NetworkMessage.CreateProtobufMessage(MessageType.ClientRequestValidationMail, null)).ConfigureAwait(false);
            return response.Result;
        }

        /// <summary>
        /// Sets the currently playing game to the specified app ID
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task SetPlayingGameAsync(int appId)
        {
            GameId id = new GameId(appId, GameType.App, 0);
            var body = new CMsgClientGamesPlayed
            {
                client_os_type = (uint)HardwareUtils.GetCurrentOsType(),
            };
            body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
            {
                game_id = id
            });

            var message = NetworkMessage.CreateProtobufMessage(MessageType.ClientGamesPlayed, body);

            await SendAsync(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the currently playing game to a shortcut of the specified name
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public async Task SetPlayingGameAsync(string game)
        {
            GameId id = GameId.Shortcut;
            var body = new CMsgClientGamesPlayed
            {
                client_os_type = (uint)HardwareUtils.GetCurrentOsType(),
            };
            body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
            {
                game_id = id,
                game_extra_info = game
            });

            var message = NetworkMessage.CreateProtobufMessage(MessageType.ClientGamesPlayed, body);

            await SendAsync(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the currently playing game to a mod for the specified app
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="modId"></param>
        /// <returns></returns>
        public async Task SetPlayingGameAsync(int appId, long modId)
        {
            GameId id = new GameId(appId, GameType.Mod, modId);
            var body = new CMsgClientGamesPlayed
            {
                client_os_type = (uint)HardwareUtils.GetCurrentOsType(),
            };
            body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
            {
                game_id = id
            });
            var message = NetworkMessage.CreateProtobufMessage(MessageType.ClientGamesPlayed, body);
            
            await SendAsync(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the number of current players playing the specified game on Steam
        /// </summary>
        /// <param name="appId"></param>
        /// <returns>
        /// <para>
        /// A task that when awaited returns the number of players playing the specified app. 
        /// </para>
        /// </returns>
        public async Task<int> GetNumberOfCurrentPlayersAsync(long appId)
        {
            if (appId < uint.MinValue || appId > uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(appId));

            var message = NetworkMessage.CreateAppRoutedMessage(MessageType.ClientGetNumberOfCurrentPlayersDP, new CMsgDPGetNumberOfCurrentPlayers { appid = (uint)appId }, appId);
            var response = await SendJobAsync<CMsgDPGetNumberOfCurrentPlayersResponse>(message).ConfigureAwait(false);

            SteamException.ThrowIfNotOK(response.eresult, $"A request for the current player count of app ID {appId} did not complete succesfully. Result: {(Result)response.eresult}");
            return response.player_count;
        }

        /// <summary>
        /// Gets a list of changes that have occured in the PICS database since the specified change number
        /// </summary>
        /// <param name="lastChangeNumber"></param>
        /// <param name="sendAppChangelist"></param>
        /// <param name="sendPackageChangeList"></param>
        /// <returns></returns>
        public async Task<PicsChanges> GetPicsChangesSinceChangeNumber(long lastChangeNumber = 0, bool sendAppChangelist = true, bool sendPackageChangeList = true)
        {
            if (lastChangeNumber < uint.MinValue || lastChangeNumber > uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(lastChangeNumber));

            CMsgClientPICSChangesSinceRequest request = new CMsgClientPICSChangesSinceRequest
            {
                send_app_info_changes = sendAppChangelist,
                send_package_info_changes = sendPackageChangeList,
                since_change_number = (uint)lastChangeNumber
            };

            var response = await SendJobAsync<CMsgClientPICSChangesSinceResponse>(NetworkMessage.CreateProtobufMessage(MessageType.ClientPICSChangesSinceRequest, request)).ConfigureAwait(false);
            return PicsChanges.Create(response);
        }

        private async Task RunHeartbeatAsync(int interval, CancellationToken token)
        {
            #region Valve sucks at naming enum members
            /*
             * Let me tell you a story about 10/14/2017
             * So I was working on this lib ironing out some kinks, rewriting the connection code
             * when I finally completed it and decided to test it. So I pop open a console just to 
             * find obvious bugs like the whole thing exploding for no reason. Suddenly at about 5:08 PM
             * it starts working properly. Stuff starts printing in the console and it's working properly. 
             * It works correctly until about 20 seconds later when the console window vanishes and up pops an error
             * "IOException: Unable to transfer data" bla bla bla BASICALLY the connection was aborted.
             * This brought up another bug in my TCP client where I wouldn't actually tell anyone the connection disconnected,
             * but who cares about that. This brought in a more important problem: Steam drops my connection even though my heart is beating
             * So I try a WebSocket. Same thing. Check the headers (because there's nothing the in body), everything lines up.
             * I reference the SteamKit, everything related to serialization is in order. Everything is right except one thing.
             * One small piece that nobody would notice.
             * The correct message type for a heartbeat is "ClientHeartBeat", not "Heartbeat"...
             * I found this out after 3 hours. Please kill me.
             * 
             * 
             * TLDR: Valve sucks at naming enum members.
             */
            #endregion
            
            // cache the data so we don't serialize 500 times per session. Our session ID will never change and neither will our Steam ID
            var beat = NetworkMessage.CreateProtobufMessage(MessageType.ClientHeartBeat, new CMsgClientHeartBeat()).WithClientInfo(SteamId, SessionId).Serialize();

            try
            {
                await NetLog.DebugAsync($"Heartbeat started on a {interval} ms interval").ConfigureAwait(false);
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(interval, token).ConfigureAwait(false);

                    try
                    {
                        await SendAsync(beat).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        await NetLog.ErrorAsync($"The heartbeat task encountered an unknown exception while sending the heartbeat message", ex).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                await NetLog.DebugAsync("Heartbeat stopped").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await NetLog.ErrorAsync($"The heartbeat task encountered an unknown exception", ex).ConfigureAwait(false);
            }
        }
    }
}
