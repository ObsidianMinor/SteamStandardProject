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
        private NetworkMessage _logonContinuation;
        private CMsgClientLogonResponse _previousLogonResponse;
        private Dictionary<int, GameCoordinator> _gameCoordinators = new Dictionary<int, GameCoordinator>();
        private TaskCompletionSource<object> _loginPromise;
        private Func<Task> _loginAction;

        private List<ServerType> _serverTypesAvailable = new List<ServerType>();

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
        public SteamId SteamId { get; private set; }

        /// <summary>
        /// Gets the client's current cell ID
        /// </summary>
        public long CellId { get; private set; }
        
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

            _connection.Disconnected += (ex, recon) => TimedInvokeAsync(_disconnected, nameof(Disconnected), ex);
            _connection.Connected += async () =>
            {
                await TimedInvokeAsync(_connected, nameof(Connected));
            };

            IReceiveMethodResolver resolver;
            if (config.ReceiveMethodResolver == null)
            {
                resolver = new DefaultReceiveMethodResolver();
            }
            else
            {
                resolver = config.ReceiveMethodResolver() ?? new DefaultReceiveMethodResolver();
            }

            foreach (MethodInfo method in GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                var attribute = method.GetCustomAttribute<MessageReceiverAttribute>();
                if (attribute != null)
                {
                    if (resolver.TryResolve(method, this, out MessageReceiver receiver))
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
                await NetLog.DebugAsync("Waiting for encryption");
                await _connection.WaitAsync().ConfigureAwait(false);
            }

            await NetLog.DebugAsync("Performing possible login actions");
            // todo: resume connections and things
            if (_loginPromise != null)
            {
                _loginPromise.SetResult(new object());
            }
            else
            {
                await _ready.InvokeAsync().ConfigureAwait(false);
            }

            await _connection.CompleteAsync();
        }

        private async Task OnDisconnectingAsync(Exception ex)
        {
            await NetLog.DebugAsync("Disconnecting all game coordinators").ConfigureAwait(false);
            foreach (var gc in _gameCoordinators)
            {
                await gc.Value.StopAsync();
                _gameCoordinators.Remove(gc.Key);
            }

            await NetLog.InfoAsync("Logging off");
            await SendAsync(NetworkMessage.CreateProtobufMessage(MessageType.ClientLogOff, null));

            await NetLog.DebugAsync("Cancelling all jobs").ConfigureAwait(false);
            await _jobs.CancelAllJobs().ConfigureAwait(false);

            await NetLog.DebugAsync("Waiting for heartbeat").ConfigureAwait(false);
            if (_heartBeatTask != null)
                await _heartBeatTask.ConfigureAwait(false);
            _heartBeatTask = null;

            await NetLog.DebugAsync("Disconnecting client").ConfigureAwait(false);
            await DisconnectAsync();
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
            try
            {
                _connectCancellationToken = new CancellationTokenSource();
                Socket.SetCancellationToken(_connectCancellationToken.Token);

                if (Socket is IWebSocketClient webSocketClient)
                {
                    if (_webSockets.Count == 0)
                    {
                        ISteamDirectory directory = GetInterface<ISteamDirectory>();
                        try
                        {
                            var cmList = await directory.GetConnectionManagerListAsync(CellId);
                            _webSockets.AddRange(cmList.Response.WebSocketServerList);
                            if (_webSockets.Count == 0)
                                throw new InvalidOperationException();
                        }
                        catch (HttpException e)
                        {
                            await NetLog.ErrorAsync("Could not fetch the Steam directory: ", e);
                            throw;
                        }
                    }

                    if (CurrentWebSocket == null)
                        CurrentWebSocket = new Uri($"wss://{_webSockets.First()}/cmsocket/");

                    await NetLog.InfoAsync($"Connecting to WebSocket {CurrentWebSocket}");
                    await webSocketClient.ConnectAsync(CurrentWebSocket);
                }
                else
                {
                    if (_endpoints.Count == 0)
                    {
                        ISteamDirectory directory = GetInterface<ISteamDirectory>();
                        try
                        {
                            var cmList = await directory.GetConnectionManagerListAsync(CellId);
                            _endpoints.AddRange(cmList.Response.ServerList);
                            if (_endpoints.Count == 0)
                                throw new InvalidOperationException();
                        }
                        catch (HttpException e)
                        {
                            await NetLog.ErrorAsync("Could not fetch the Steam directory: ", e);
                            throw;
                        }
                    }

                    if (CurrentEndPoint == null)
                        CurrentEndPoint = _endpoints.First();

                    await NetLog.InfoAsync($"Connecting to endpoint {CurrentEndPoint}");
                    await Socket.ConnectAsync(CurrentEndPoint);
                }
            }
            catch (Exception e)
            {
                await DisconnectInternalAsync().ConfigureAwait(false);
                throw e;
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

            await Socket.DisconnectAsync();
        }

        // in exchange for a gc instance to track, we give back a log manager
        internal (Logger, JobManager<GameCoordinatorMessage>) AttachGC(GameCoordinator gc, string name)
        {
            _gameCoordinators.Add(gc.AppId, gc);
            return (LogManager.CreateLogger(name), new JobManager<GameCoordinatorMessage>(LogManager.CreateLogger("GCJobs")));
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
            await LoginAsync(username, password, twoFactor, authCode, null, rememberPassword, requestSteam2Ticket, null, 0, AccountType.Individual).ConfigureAwait(false);
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
            await LoginAsync(username, password, null, null, null, false, requestSteam2Ticket, sentryFileHash, 0, AccountType.Individual).ConfigureAwait(false);
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
            await LoginAsync(username, null, null, null, loginKey, true, requestSteam2Ticket, null, 0, AccountType.Individual).ConfigureAwait(false);
        }

        /// <summary>
        /// Logs into the Steam network anonymously
        /// </summary>
        /// <returns></returns>
        public async Task LoginAnonymousAsync()
        {
            await LoginAsync(null, null, null, null, null, false, false, null, 0, AccountType.AnonUser).ConfigureAwait(false);
        }

        /// <summary>
        /// Logs into the Steam network as a console user using the provided account ID
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public async Task LoginConsoleAsync(long accountId)
        {
            await LoginAsync(null, null, null, null, null, false, false, null, (uint)accountId, AccountType.Individual).ConfigureAwait(false);
        }

        /// <summary>
        /// Logs into the Steam network as a game server
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task LoginGameServerAsync(long appId, string token)
        {
            throw new NotImplementedException();
        }

        public Task LoginGameServerAnonymousAsync(long appid)
        {
            throw new NotImplementedException();
        }

        // StartAndLogin feels hacky, please post suggestions for it

        private async Task WaitForConnectionAsync()
        {
            _loginPromise = new TaskCompletionSource<object>();
            await _loginPromise.Task;
            _loginPromise = null;
        }

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
            await StartAsync();
            await WaitForConnectionAsync();
            await LoginAsync(username, password, twoFactor, authCode, rememberPassword, requestSteam2Ticket);
        }

        public async Task StartAndLoginAsync(string username, string password, byte[] sentryFileHash,
            bool requestSteam2Ticket = false)
        {
            await StartAsync();
            await WaitForConnectionAsync();
            await LoginAsync(username, password, sentryFileHash, requestSteam2Ticket);
        }

        public async Task StartAndLoginAsync(string username, string loginKey, bool requestSteam2Ticket = false)
        {
            await StartAsync();
            await WaitForConnectionAsync();
            await LoginAsync(username, loginKey, requestSteam2Ticket);
        }

        public async Task StartAndLoginAnonymousAsync()
        {
            await StartAsync();
            await WaitForConnectionAsync();
            await LoginAnonymousAsync();
        }

        public async Task StartAndLoginConsoleAsync(long accountId)
        {
            await StartAsync();
            await WaitForConnectionAsync();
            await LoginConsoleAsync(accountId);
        }

        public async Task StartAndLoginGameServerAsync(long appId, string token)
        {
            await StartAsync();
            await WaitForConnectionAsync();
            await LoginGameServerAsync(appId, token);
        }

        public async Task StartAndLoginGameServerAnonymousAsync(long appId)
        {
            await StartAsync();
            await WaitForConnectionAsync();
            await LoginGameServerAnonymousAsync(appId);
        }

        private async Task LoginAsync(string username, string password, string twoFactorCode, string authCode, string loginKey, bool shouldRememberPassword, bool requestSteam2Ticket, byte[] sentryFileHash, uint accountId, AccountType accountType)
        {
            uint instance = 0;
            if (accountId != 0)
                instance = 2;
            else if (accountType != AccountType.AnonUser)
                instance = 1;

            await NetLog.InfoAsync($"Logging in as {username ?? (instance == 0 ? "an anonymous user" : "a console user")}");
            byte[] machineId = await HardwareUtils.GetMachineId(); // while we set up the logon object, we will start to get the machine ID

            var body = new CMsgClientLogon
            {
                protocol_version = 65579,
                client_os_type = (uint)(accountId == 0 ? HardwareUtils.GetCurrentOsType() : OsType.PS3),
                client_language = GetConfig<SteamNetworkConfig>().Language.GetApiLanguageCode(),
                cell_id = (uint)GetConfig<SteamNetworkConfig>().CellId,
            };

            if (machineId != null && machineId.Length != 0)
                body.machine_id = machineId;

            if (accountType != AccountType.AnonUser)
            {
                body.account_name = username;
                body.password = password;
                body.should_remember_password = shouldRememberPassword;
                body.steam2_ticket_request = requestSteam2Ticket;
                body.two_factor_code = twoFactorCode;
                body.auth_code = authCode;
                body.login_key = loginKey;
                body.sha_sentryfile = sentryFileHash;
                body.eresult_sentryfile = (int)(sentryFileHash is null ? Result.FileNotFound : Result.OK);
                body.client_package_version = 1771;
                body.obfustucated_private_ip = (uint)(GetConfig<SteamNetworkConfig>().LoginId < 0 ? Socket.LocalIp.ToUInt32() ^ _obfuscationMask : GetConfig<SteamNetworkConfig>().LoginId);
                body.supports_rate_limit_response = true;
            }

            var logon = NetworkMessage.CreateProtobufMessage(MessageType.ClientLogon, body);

            (logon.Header as ClientHeader).SteamId = new SteamId(accountId, GetConfig<SteamNetworkConfig>().DefaultUniverse, accountType, instance);

            await SendAsync(logon).ConfigureAwait(false);

            _logonContinuation = logon;
        }

        /// <summary>
        /// Requests a validation email to be sent for the current account
        /// </summary>
        /// <returns></returns>
        public async Task<Result> RequestValidationEmailAsync()
        {
            GenericResponse response = await SendJobAsync<GenericResponse>(NetworkMessage.CreateProtobufMessage(MessageType.ClientRequestValidationMail, null));
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
        /// <para>
        /// If the response result is not <see cref="Result.OK"/>, the returned value is <value>-1</value> and the result code is logged
        /// </para>
        /// </returns>
        public async Task<int> GetNumberOfCurrentPlayersAsync(long appId)
        {
            if (appId < uint.MinValue || appId > uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(appId));

            var message = NetworkMessage.CreateProtobufMessage(MessageType.ClientGetNumberOfCurrentPlayersDP, new CMsgDPGetNumberOfCurrentPlayers { appid = (uint)appId });
            var response = await SendJobAsync<CMsgDPGetNumberOfCurrentPlayersResponse>(message);

            SteamException.ThrowIfNotOK(response.eresult, $"A request for the current player count of app ID {appId} did not complete succesfully. Result: {(Result)response.eresult}");
            return response.player_count;
        }

        internal async Task SendGameCoordinatorMessage(int appid, GameCoordinatorMessage gcmessage)
        {
            CMsgGCClient body = new CMsgGCClient()
            {
                msgtype = MessageTypeUtils.MergeMessage((uint)gcmessage.MessageType, gcmessage.Protobuf),
                appid = (uint)appid,
                payload = gcmessage.Serialize()
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

            NetworkMessage message = NetworkMessage.CreateProtobufMessage(MessageType.ClientHeartBeat, new CMsgClientHeartBeat());
            try
            {
                await NetLog.DebugAsync($"Heartbeat started on a {interval} ms interval");
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(interval, token).ConfigureAwait(false);

                    try
                    {
                        await SendAsync(message).ConfigureAwait(false);
                        await NetLog.DebugAsync("Sent heartbeat");
                    }
                    catch (Exception ex)
                    {
                        await NetLog.ErrorAsync($"The heartbeat task encountered an unknown exception", ex);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                await NetLog.DebugAsync("Heartbeat stopped");
            }
            catch (Exception ex)
            {
                await NetLog.ErrorAsync($"The heartbeat task encountered an unknown exception", ex);
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

            if (message.Header is ClientHeader clientHeader)
            {
                if (SessionId != 0)
                    clientHeader.SessionId = SessionId;

                if (SteamId > 0 && clientHeader.SteamId == SteamId.Zero)
                    clientHeader.SteamId = SteamId;
            }

            byte[] data = message.Serialize();

            Encryption.Encrypt(ref data);

            await NetLog.DebugAsync($"Sending message of message type {message.MessageType} as a {(message.Protobuf ? "protobuf" : "struct")}. Resulting packet is {data.Length} bytes long.");

            await Socket.SendAsync(data).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a message to Steam as a job and returns the body of the response
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
            message.Header.JobId = job;

            await SendAsync(message).ConfigureAwait(false);
            return await task;
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
            message.Header.JobId = job;

            await SendAsync(message).ConfigureAwait(false);

            List<TResponse> responses = new List<TResponse>();
            NetworkMessage response = await task;
            TResponse result = response.Deserialize<TResponse>();
            while (!completionFunc(result))
            {
                responses.Add(result);

                task = _jobs.AddJob(job);
                response = await task;
                result = response.Deserialize<TResponse>();
            }

            return selector(responses.ToReadOnlyCollection());
        }

        private async Task ReceiveAsync(byte[] data)
        {
            Encryption.Decrypt(ref data);
            await DispatchData(data).ConfigureAwait(false);
        }

        private async Task DispatchData(byte[] data)
        {
            NetworkMessage message = NetworkMessage.CreateFromByteArray(data);

            await NetLog.DebugAsync($"Received message of type {message.MessageType}");

            if (_jobs.IsRunningJob(message.Header.JobId))
                await _jobs.SetJobResult(message, message.Header.JobId);

            if (_eventDispatchers.TryGetValue(message.MessageType, out var dispatch))
            {
                foreach (MessageReceiver dispatcher in dispatch.GetInvocationList())
                {
                    await TimeoutWrap(dispatcher.Method.ToString(), () => dispatcher(message));
                }
            }
            else
            {
                await NetLog.DebugAsync($"No receiver found for message type {message.MessageType}");
            }
        }

        #endregion

        private async Task TimedInvokeAsync(AsyncEvent<Func<Task>> eventHandler, string name)
        {
            if (eventHandler.HasSubscribers)
            {
                if (GetConfig<SteamNetworkConfig>().ReceiveMethodTimeout > 0)
                    await TimeoutWrap(name, () => eventHandler.InvokeAsync()).ConfigureAwait(false);
                else
                    await eventHandler.InvokeAsync().ConfigureAwait(false);
            }
        }

        private async Task TimedInvokeAsync<T>(AsyncEvent<Func<T, Task>> eventHandler, string name, T arg)
        {
            if (eventHandler.HasSubscribers)
            {
                if (GetConfig<SteamNetworkConfig>().ReceiveMethodTimeout > 0)
                    await TimeoutWrap(name, () => eventHandler.InvokeAsync(arg)).ConfigureAwait(false);
                else
                    await eventHandler.InvokeAsync(arg).ConfigureAwait(false);
            }
        }

        private async Task TimedInvokeAsync<T1, T2>(AsyncEvent<Func<T1, T2, Task>> eventHandler, string name, T1 arg1, T2 arg2)
        {
            if (eventHandler.HasSubscribers)
            {
                if (GetConfig<SteamNetworkConfig>().ReceiveMethodTimeout > 0)
                    await TimeoutWrap(name, () => eventHandler.InvokeAsync(arg1, arg2)).ConfigureAwait(false);
                else
                    await eventHandler.InvokeAsync(arg1, arg2).ConfigureAwait(false);
            }
        }

        private async Task TimedInvokeAsync<T1, T2, T3>(AsyncEvent<Func<T1, T2, T3, Task>> eventHandler, string name, T1 arg1, T2 arg2, T3 arg3)
        {
            if (eventHandler.HasSubscribers)
            {
                if (GetConfig<SteamNetworkConfig>().ReceiveMethodTimeout > 0)
                    await TimeoutWrap(name, () => eventHandler.InvokeAsync(arg1, arg2, arg3)).ConfigureAwait(false);
                else
                    await eventHandler.InvokeAsync(arg1, arg2, arg3).ConfigureAwait(false);
            }
        }

        private async Task TimedInvokeAsync<T1, T2, T3, T4>(AsyncEvent<Func<T1, T2, T3, T4, Task>> eventHandler, string name, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (eventHandler.HasSubscribers)
            {
                if (GetConfig<SteamNetworkConfig>().ReceiveMethodTimeout > 0)
                    await TimeoutWrap(name, () => eventHandler.InvokeAsync(arg1, arg2, arg3, arg4)).ConfigureAwait(false);
                else
                    await eventHandler.InvokeAsync(arg1, arg2, arg3, arg4).ConfigureAwait(false);
            }
        }

        private async Task TimedInvokeAsync<T1, T2, T3, T4, T5>(AsyncEvent<Func<T1, T2, T3, T4, T5, Task>> eventHandler, string name, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            if (eventHandler.HasSubscribers)
            {
                if (GetConfig<SteamNetworkConfig>().ReceiveMethodTimeout > 0)
                    await TimeoutWrap(name, () => eventHandler.InvokeAsync(arg1, arg2, arg3, arg4, arg5)).ConfigureAwait(false);
                else
                    await eventHandler.InvokeAsync(arg1, arg2, arg3, arg4, arg5).ConfigureAwait(false);
            }
        }

        private async Task TimeoutWrap(string name, Func<Task> action)
        {
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            cancellationToken.CancelAfter(GetConfig<SteamNetworkConfig>().ReceiveMethodTimeout);
            await Task.Run(action, cancellationToken.Token).ContinueWith(async (t) =>
            {
                if (t.IsCanceled)
                    await NetLog.ErrorAsync($"A receiver method or event handler took too long to complete execution and was cancelled prematurely", new TaskCanceledException(t));

                if (t.IsFaulted)
                    await NetLog.ErrorAsync($"A receiver method or event handler threw an exception", t.Exception);
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
