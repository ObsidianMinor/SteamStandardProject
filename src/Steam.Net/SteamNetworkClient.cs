using Steam.Logging;
using Steam.Net.GameCoordinators;
using Steam.Net.GameCoordinators.Messages;
using Steam.Net.Messages;
using Steam.Net.Messages.Protobufs;
using Steam.Net.Messages.Structs;
using Steam.Net.Sockets;
using Steam.Net.Utilities;
using Steam.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Net
{
    /// <summary>
    /// Provides a client with access to Steam's network and the Steam Web API
    /// </summary>
    public partial class SteamNetworkClient : SteamWebClient
    {
        private const uint _currentProtocolVer = 65579;
        private const uint _obfuscationMask = 0xBAADF00D;

        private static readonly NoEncryptor _defaultEncryptor = new NoEncryptor();

        private readonly IReceiveMethodResolver _resolver;
        private readonly SemaphoreSlim _stateLock;
        private readonly SemaphoreSlim _connectionStateLock;
        private readonly JobManager<NetworkMessage> _jobs;
        private readonly ConnectionManager _connection;
        private readonly ISocketClient Socket;
        private readonly Dictionary<MessageType, MessageReceiver> _eventDispatchers = new Dictionary<MessageType, MessageReceiver>();
        private readonly List<IPEndPoint> _endpoints;
        private readonly List<Uri> _webSockets;
        private Func<Exception, Task> _socketDisconnected;

        private CancellationTokenSource _connectCancellationToken;
        private IEncryptor _encryptor;

        private Task _heartBeatTask;
        private CancellationTokenSource _heartbeatCancel;

        // login continuation
        private CMsgClientLogonResponse _previousLogonResponse;
        private LoginInfo _previousLogonRequest;
        private Func<Task> _logonFunc;

        private Dictionary<int, GameCoordinator> _gameCoordinators = new Dictionary<int, GameCoordinator>();
        private bool _gracefulLogoff;

        private TaskCompletionSource<bool> _continueWaiter;

        /// <summary>
        /// Gets a collection of all game coordinators attached to this client
        /// </summary>
        public IReadOnlyCollection<GameCoordinator> GameCoordinators { get; }

        /// <summary>
        /// Gets the logger for this network client
        /// </summary>
        protected Logger NetLog { get; }

        internal IPEndPoint CurrentEndPoint { get; private set; }

        internal Uri CurrentWebSocket { get; private set; }

        internal IEncryptor Encryption
        {
            get
            {
                if ((ConnectionState != ConnectionState.Connected && !_connection.IsConnectionComplete) || _encryptor == null)
                    return _defaultEncryptor;
                else
                    return _encryptor;
            }
            set
            {
                _encryptor = value;
            }
        }

        /// <summary>
        /// Gets the client's current connection state
        /// </summary>
        public ConnectionState ConnectionState => _connection.State;

        /// <summary>
        /// Gets the client's current session ID
        /// </summary>
        public int SessionId { get; private set; }

        /// <summary>
        /// Gets the client's current user's Steam ID
        /// </summary>
        public SteamId SteamId => CurrentUser?.Id ?? SteamId.Zero;

        /// <summary>
        /// Gets the client's current user
        /// </summary>
        public SelfUser CurrentUser { get; private set; }

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
            Socket = config.SocketClient();
            Socket.MessageReceived += ReceiveAsync;
            Socket.Disconnected += async ex =>
            {
                await DisconnectAsync().ConfigureAwait(false);
                await _socketDisconnected(ex).ConfigureAwait(false);
            };
            CellId = config.CellId > uint.MaxValue || config.CellId < uint.MinValue ? 0 : config.CellId;
            _stateLock = new SemaphoreSlim(1, 1);
            _connectionStateLock = new SemaphoreSlim(1, 1);
            _jobs = new JobManager<NetworkMessage>(LogManager.CreateLogger("Jobs"));
            NetLog = LogManager.CreateLogger("Net");
            if (Socket is IWebSocketClient)
                _webSockets = new List<Uri>(GetConfig<SteamNetworkConfig>().WebSockets ?? Enumerable.Empty<Uri>());
            else
                _endpoints = new List<IPEndPoint>(GetConfig<SteamNetworkConfig>().ConnectionManagers ?? Enumerable.Empty<IPEndPoint>());

            _connection = new ConnectionManager(_connectionStateLock, LogManager.CreateLogger("CM"), GetConfig<SteamNetworkConfig>().NetworkConnectionTimeout,
                OnConnectingAsync, OnDisconnectingAsync, (x) => _socketDisconnected = x);

            _connection.Disconnected += async (ex, recon) => 
            {
                await TimedInvokeAsync(_disconnected, nameof(Disconnected), ex).ConfigureAwait(false);
            };
            _connection.Connected += async () =>
            {
                await TimedInvokeAsync(_connected, nameof(Connected)).ConfigureAwait(false);
            };
            
            _resolver = config.ReceiveMethodResolver == null ? new DefaultReceiveMethodResolver() : config.ReceiveMethodResolver() ?? new DefaultReceiveMethodResolver();

            List<TypeInfo> types = new List<TypeInfo>();
            for (TypeInfo type = GetType().GetTypeInfo(); type != null; type = type.BaseType?.GetTypeInfo())
            {
                types.Add(type);
            }
            
            foreach (MethodInfo method in types.SelectMany(t => t.DeclaredMethods))
            {
                var attribute = method.GetCustomAttribute<MessageReceiverAttribute>();
                if (attribute != null)
                {
                    if (_resolver.TryResolve(method, this, out MessageReceiver receiver))
                        Subscribe(attribute.Type, receiver);
                }
            }
        }
 
        /// <summary>
        /// Subscribes the specified receiver to messages of the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="receiver"></param>
        public void Subscribe(MessageType type, MessageReceiver receiver)
        {
            if (!_eventDispatchers.ContainsKey(type))
                _eventDispatchers[type] = receiver;
            else
                _eventDispatchers[type] += receiver;
        }

        /// <summary>
        /// Unsubscribes the specified receiver from messages of the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="receiver"></param>
        public void Unsubscribe(MessageType type, MessageReceiver receiver)
        {
            if (!_eventDispatchers.ContainsKey(type))
                _eventDispatchers[type] -= receiver;
        }

        /// <summary>
        /// Connects this client to the Steam network
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync() => await _connection.StartAsync().ConfigureAwait(false);

        /// <summary>
        /// Disconnects this client from the Steam network and disconnects all game coordinators
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync() => await _connection.StopAsync().ConfigureAwait(false);

        /// <summary>
        /// Connects this client to a connection manager 
        /// </summary>
        /// <returns></returns>
        private async Task OnConnectingAsync()
        {
            await NetLog.DebugAsync("Connecting client").ConfigureAwait(false);
            await ConnectAsync().ConfigureAwait(false);

            if (!(Socket is IWebSocketClient))
            {
                await NetLog.DebugAsync("Waiting for encryption").ConfigureAwait(false);
                await _connection.WaitAsync().ConfigureAwait(false);
            }

            await NetLog.DebugAsync("Performing possible login actions").ConfigureAwait(false);
            // todo: resume connections and things
            if (_previousLogonResponse != null)
            {
                await LoginAsync(_previousLogonRequest).ConfigureAwait(false);
            }
            else if (_logonFunc != null)
            {
                await _logonFunc().ConfigureAwait(false);
            }
            else
            {
                await _ready.InvokeAsync().ConfigureAwait(false);
            }

            await _connection.CompleteAsync().ConfigureAwait(false);
        }

        private async Task OnDisconnectingAsync(Exception ex)
        {
            if (!_gracefulLogoff)
            {
                CurrentEndPoint = null;
                CurrentWebSocket = null;
            }

            await NetLog.DebugAsync("Disconnecting all game coordinators").ConfigureAwait(false);
            foreach (var gc in _gameCoordinators)
            {
                await gc.Value.StopAsync().ConfigureAwait(false);
                _gameCoordinators.Remove(gc.Key);
            }

            await NetLog.DebugAsync("Cancelling all jobs").ConfigureAwait(false);
            await _jobs.CancelAllJobs().ConfigureAwait(false);

            await NetLog.DebugAsync("Waiting for heartbeat").ConfigureAwait(false);
            _heartbeatCancel?.Cancel();
            if (_heartBeatTask != null)
                await _heartBeatTask.ConfigureAwait(false);
            _heartBeatTask = null;

            await NetLog.DebugAsync("Disconnecting client").ConfigureAwait(false);
            await DisconnectAsync().ConfigureAwait(false);

            if (_continueWaiter != null)
            {
                await NetLog.DebugAsync("Waiting for previous logon result");
                await WaitAsync();
            }
        }

        private async Task ConnectAsync()
        {
            await _stateLock.WaitAsync().ConfigureAwait(false);
            try
            {
                await ConnectInternalAsync().ConfigureAwait(false);
            }
            finally
            {
                _stateLock.Release();
            }
        }

        private async Task ConnectInternalAsync()
        {
            _connectCancellationToken = new CancellationTokenSource();
            Socket.SetCancellationToken(_connectCancellationToken.Token);
            if (Socket is IWebSocketClient webSocketClient)
            {
                if (_webSockets.Count == 0)
                {
                    IEnumerable<Uri> webSockets = GetConfig<SteamNetworkConfig>().WebSockets;
                    _webSockets.AddRange(webSockets ?? Enumerable.Empty<Uri>());
                    if (_webSockets.Count == 0)
                    {
                        ISteamDirectory directory = GetInterface<ISteamDirectory>();
                        var cmList = await directory.GetConnectionManagerListAsync(CellId).ConfigureAwait(false);
                        _webSockets.AddRange(cmList.Response.WebSocketServerList);
                        if (_webSockets.Count == 0)
                        {
                            await NetLog.WarningAsync("The Steam directory did not return any WebSocket addresses").ConfigureAwait(false);
                            throw new InvalidOperationException("Could not find any WebSocket addresses to connect to");
                        }
                    }
                }

                while (CurrentWebSocket == null && _webSockets.Count != 0)
                {
                    if (_webSockets.First() == null)
                    {
                        _webSockets.RemoveAt(0);
                        continue;
                    }
                    CurrentWebSocket = new Uri($"wss://{_webSockets.First()}/cmsocket/");
                }

                await NetLog.InfoAsync($"Connecting to WebSocket {CurrentWebSocket}").ConfigureAwait(false);
                await webSocketClient.ConnectAsync(CurrentWebSocket).ConfigureAwait(false);
            }
            else
            {
                if (_endpoints.Count == 0)
                {
                    IEnumerable<IPEndPoint> endPoints = GetConfig<SteamNetworkConfig>().ConnectionManagers;
                    _endpoints.AddRange(endPoints ?? Enumerable.Empty<IPEndPoint>());
                    if (_endpoints.Count == 0)
                    {
                        ISteamDirectory directory = GetInterface<ISteamDirectory>();
                        var cmList = await directory.GetConnectionManagerListAsync(CellId).ConfigureAwait(false);
                        _endpoints.AddRange(cmList.Response.ServerList);
                        if (_endpoints.Count == 0)
                        {
                            await NetLog.WarningAsync("The Steam directory did not return any IP end points").ConfigureAwait(false);
                            throw new InvalidOperationException("Could not find any WebSocket addresses to connect to");
                        }
                    }
                }

                while (CurrentEndPoint == null && _endpoints.Count != 0)
                {
                    CurrentEndPoint = _endpoints.First();
                    _endpoints.RemoveAt(0);
                }

                await NetLog.InfoAsync($"Connecting to endpoint {CurrentEndPoint}").ConfigureAwait(false);
                await Socket.ConnectAsync(CurrentEndPoint).ConfigureAwait(false);
            }
        }

        private async Task DisconnectAsync()
        {
            await _stateLock.WaitAsync().ConfigureAwait(false);
            try
            {
                await DisconnectInternalAsync().ConfigureAwait(false);
            }
            finally
            {
                _stateLock.Release();
            }
        }

        private async Task DisconnectInternalAsync()
        {
            try
            {
                _connectCancellationToken?.Cancel(false);
            }
            catch { }

            await Socket.DisconnectAsync().ConfigureAwait(false);
        }

        private async Task StartEventWait()
        {
            await WaitAsync();
            _continueWaiter = new TaskCompletionSource<bool>();
        }

        private async Task WaitAsync()
        {
            await (_continueWaiter?.Task ?? Task.FromResult(true));
        }

        private async Task CompleteAsync()
        {
            await _continueWaiter?.TrySetResultAsync(true);
            _continueWaiter = null;
        }

        // in exchange for a gc instance to track, we give back a logger, job manager, and method resolver
        internal (Logger, JobManager<GameCoordinatorMessage>, IReceiveMethodResolver) AttachGC(GameCoordinator gc, string name)
        {
            _gameCoordinators.Add(gc.AppId, gc);
            return (LogManager.CreateLogger(name), new JobManager<GameCoordinatorMessage>(LogManager.CreateLogger("GCJobs")), _resolver);
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
                AccountType = AccountType.Individual 
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
                obfustucated_private_ip = (uint)(GetConfig<SteamNetworkConfig>().LoginId < 0 ? Socket.LocalIp.ToUInt32() ^ _obfuscationMask : GetConfig<SteamNetworkConfig>().LoginId),
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
                obfustucated_private_ip = (uint)(GetConfig<SteamNetworkConfig>().LoginId < 0 ? Socket.LocalIp.ToUInt32() ^ _obfuscationMask : GetConfig<SteamNetworkConfig>().LoginId),
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

        public async Task StartAndLoginAsync(string username, string password, byte[] sentryFileHash,
            bool requestSteam2Ticket = false)
        {
            await StartAsync().ConfigureAwait(false);
            _logonFunc = () => LoginAsync(username, password, sentryFileHash, requestSteam2Ticket);
        }

        public async Task StartAndLoginAsync(string username, string loginKey, bool requestSteam2Ticket = false)
        {
            await StartAsync().ConfigureAwait(false);
            _logonFunc = () => LoginAsync(username, loginKey, requestSteam2Ticket);
        }

        public async Task StartAndLoginAnonymousAsync()
        {
            await StartAsync().ConfigureAwait(false);
            _logonFunc = LoginAnonymousAsync;
        }

        public async Task StartAndLoginConsoleAsync(long accountId)
        {
            await StartAsync().ConfigureAwait(false);
            _logonFunc = () => LoginConsoleAsync(accountId);
        }

        public async Task StartAndLoginGameServerAsync(int appId, string token)
        {
            await StartAsync().ConfigureAwait(false);
            _logonFunc = () => LoginGameServerAsync(appId, token);
        }

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
                body.obfustucated_private_ip = (uint)(GetConfig<SteamNetworkConfig>().LoginId < 0 ? Socket.LocalIp.ToUInt32() ^ _obfuscationMask : GetConfig<SteamNetworkConfig>().LoginId);
                body.supports_rate_limit_response = true;
            }

            var logon = NetworkMessage
                .CreateProtobufMessage(MessageType.ClientLogon, body)
                .WithClientInfo(new SteamId(info.AccountId, GetConfig<SteamNetworkConfig>().DefaultUniverse, info.AccountType, instance), 0);

            await SendAsync(logon.Serialize()).ConfigureAwait(false);

            _previousLogonRequest = info;
        }

        /// <summary>
        /// Sets a two factor code based on the previous logon response
        /// </summary>
        public void SetLoginInformation(string code)
        {
            if (_previousLogonResponse == null)
                throw new InvalidOperationException("Cannot continue successful logon");

            if (string.IsNullOrEmpty(code))
                throw new ArgumentException("Provided code cannot be null or empty", nameof(code));

            switch((Result)_previousLogonResponse.eresult)
            {
                case Result.AccountLoginDeniedNeedTwoFactor:
                case Result.TwoFactorCodeMismatch:
                    _previousLogonRequest.TwoFactorCode = code;
                    break;
                case Result.AccountLogonDeniedVerifiedEmailRequired:
                    _previousLogonRequest.AuthCode = code;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_previousLogonResponse.eresult), "The response cannot be continue with a provided auth code");
            }
        }

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

        internal async Task SendGameCoordinatorMessage(int appid, GameCoordinatorMessage message)
        {
            CMsgGCClient body = new CMsgGCClient()
            {
                msgtype = MessageTypeUtils.MergeMessage((uint)message.MessageType, message.Protobuf),
                appid = (uint)appid,
                payload = message.Serialize()
            };

            await SendAsync(NetworkMessage.CreateAppRoutedMessage(MessageType.ClientToGC, body, appid)).ConfigureAwait(false);
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
            byte[] data = NetworkMessage.CreateProtobufMessage(MessageType.ClientHeartBeat, new CMsgClientHeartBeat()).WithClientInfo(SteamId, SessionId).Serialize();

            try
            {
                await NetLog.DebugAsync($"Heartbeat started on a {interval} ms interval").ConfigureAwait(false);
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(interval, token).ConfigureAwait(false);

                    try
                    {
                        await SendAsync(data).ConfigureAwait(false);
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

        #region Send Receive

        /// <summary>
        /// Sends a message to Steam
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <returns>An awaitable task</returns>
        protected internal async Task SendAsync(NetworkMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            
            await NetLog.DebugAsync($"Sending message of message type {message.MessageType} as a {(message.Protobuf ? "protobuf" : "struct")}.").ConfigureAwait(false);
            
            await SendAsync(message.WithClientInfo(SteamId, SessionId).Serialize()).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends an array of bytes to Steam
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected internal async Task SendAsync(byte[] message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            
            await Socket.SendAsync(Encryption.Encrypt(message)).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a message to Steam as a job and returns the body of the response. If the type of T is <see cref="NetworkMessage"/> this returns the response message
        /// </summary>
        /// <typeparam name="T">The type of the response</typeparam>
        /// <param name="message">The message to send</param>
        /// <returns>An awaitable task</returns>
        protected internal async Task<T> SendJobAsync<T>(NetworkMessage message)
        {
            if (typeof(T) == typeof(NetworkMessage))
                return (T)(object)await SendJobAsync(message).ConfigureAwait(false); // ok c#

            NetworkMessage response = await SendJobAsync(message).ConfigureAwait(false);

            return response.Deserialize<T>();
        }

        /// <summary>
        /// Sends a message to Steam as a job and returns the response message
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <returns>An awaitable task</returns>
        protected internal async Task<NetworkMessage> SendJobAsync(NetworkMessage message)
        {
            (var task, var job) = _jobs.AddJob();

            await SendAsync(message.WithJobId(job)).ConfigureAwait(false);
            return await task.ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a message to Steam as a job with multiple responses.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response</typeparam>
        /// <typeparam name="TResult">The type of the returned result</typeparam>
        /// <param name="message">The message to send</param>
        /// <param name="completionFunc">The function to evaluate if the job is complete</param>
        /// <param name="selector">The function to combine all responses into one result</param>
        protected internal async Task<TResult> SendJobAsync<TResponse, TResult>(NetworkMessage message, Func<TResponse, bool> completionFunc, Func<IEnumerable<TResponse>, TResult> selector)
        {
            (var task, var job) = _jobs.AddJob();
            message = message.WithJobId(job);

            await SendAsync(message).ConfigureAwait(false);

            List<TResponse> responses = new List<TResponse>();
            NetworkMessage response = await task.ConfigureAwait(false);
            TResponse result = response.Deserialize<TResponse>();
            while (!completionFunc(result))
            {
                responses.Add(result);

                task = _jobs.AddJob(job);
                response = await task.ConfigureAwait(false);
                result = response.Deserialize<TResponse>();
            }

            return selector(responses.ToReadOnlyCollection());
        }
        
        private async Task ReceiveAsync(byte[] data)
        {
            await DispatchData(Encryption.Decrypt(data)).ConfigureAwait(false);
        }

        private async Task DispatchData(byte[] data)
        {
            NetworkMessage message = NetworkMessage.CreateFromByteArray(data);
            
            if (_jobs.IsRunningJob(message.Header.JobId))
                await _jobs.SetJobResult(message, message.Header.JobId).ConfigureAwait(false);

            if (_eventDispatchers.TryGetValue(message.MessageType, out var dispatch))
            {
                foreach (MessageReceiver dispatcher in dispatch.GetInvocationList())
                {
                    TimeoutWrap(dispatcher.Method.ToString(), () => dispatcher(message));
                }
            }
            else
            {
                await NetLog.DebugAsync($"No receiver found for message type {message.MessageType} ({(int)message.MessageType})").ConfigureAwait(false);
            }
        }

        #endregion

        private async Task TimedInvokeAsync(AsyncEvent<Func<Task>> eventHandler, string name)
        {
            if (eventHandler.HasSubscribers)
            {
                if (GetConfig<SteamNetworkConfig>().ReceiveMethodTimeout > 0)
                    TimeoutWrap(name, () => eventHandler.InvokeAsync());
                else
                    await eventHandler.InvokeAsync().ConfigureAwait(false);
            }
        }

        private async Task TimedInvokeAsync<T>(AsyncEvent<Func<T, Task>> eventHandler, string name, T arg)
        {
            if (eventHandler.HasSubscribers)
            {
                if (GetConfig<SteamNetworkConfig>().ReceiveMethodTimeout > 0)
                    TimeoutWrap(name, () => eventHandler.InvokeAsync(arg));
                else
                    await eventHandler.InvokeAsync(arg).ConfigureAwait(false);
            }
        }

        private async Task TimedInvokeAsync<T1, T2>(AsyncEvent<Func<T1, T2, Task>> eventHandler, string name, T1 arg1, T2 arg2)
        {
            if (eventHandler.HasSubscribers)
            {
                if (GetConfig<SteamNetworkConfig>().ReceiveMethodTimeout > 0)
                    TimeoutWrap(name, () => eventHandler.InvokeAsync(arg1, arg2));
                else
                    await eventHandler.InvokeAsync(arg1, arg2).ConfigureAwait(false);
            }
        }

        private async Task TimedInvokeAsync<T1, T2, T3>(AsyncEvent<Func<T1, T2, T3, Task>> eventHandler, string name, T1 arg1, T2 arg2, T3 arg3)
        {
            if (eventHandler.HasSubscribers)
            {
                if (GetConfig<SteamNetworkConfig>().ReceiveMethodTimeout > 0)
                    TimeoutWrap(name, () => eventHandler.InvokeAsync(arg1, arg2, arg3));
                else
                    await eventHandler.InvokeAsync(arg1, arg2, arg3).ConfigureAwait(false);
            }
        }

        private async Task TimedInvokeAsync<T1, T2, T3, T4>(AsyncEvent<Func<T1, T2, T3, T4, Task>> eventHandler, string name, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (eventHandler.HasSubscribers)
            {
                if (GetConfig<SteamNetworkConfig>().ReceiveMethodTimeout > 0)
                    TimeoutWrap(name, () => eventHandler.InvokeAsync(arg1, arg2, arg3, arg4));
                else
                    await eventHandler.InvokeAsync(arg1, arg2, arg3, arg4).ConfigureAwait(false);
            }
        }

        private async Task TimedInvokeAsync<T1, T2, T3, T4, T5>(AsyncEvent<Func<T1, T2, T3, T4, T5, Task>> eventHandler, string name, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            if (eventHandler.HasSubscribers)
            {
                if (GetConfig<SteamNetworkConfig>().ReceiveMethodTimeout > 0)
                    TimeoutWrap(name, () => eventHandler.InvokeAsync(arg1, arg2, arg3, arg4, arg5));
                else
                    await eventHandler.InvokeAsync(arg1, arg2, arg3, arg4, arg5).ConfigureAwait(false);
            }
        }

        private void TimeoutWrap(string name, Func<Task> action)
        {
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            cancellationToken.CancelAfter(GetConfig<SteamNetworkConfig>().ReceiveMethodTimeout);
            var _ = Task.Run(action, cancellationToken.Token).ContinueWith(async (t) =>
            {
                if (t.IsCanceled)
                    await NetLog.ErrorAsync($"A receiver method or event handler took too long to complete execution and was cancelled prematurely", new TaskCanceledException(t)).ConfigureAwait(false);

                if (t.IsFaulted)
                    await NetLog.ErrorAsync($"A receiver method or event handler threw an exception", t.Exception).ConfigureAwait(false);
            }).ContinueWith(SwallowExceptions);
        }

        private void SwallowExceptions(Task task)
        {
            if (task.IsFaulted)
            {
                Exception e = task.Exception; // if you get here you should rethink life
            }
        }
    }
}
