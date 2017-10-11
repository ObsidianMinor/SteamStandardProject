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
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
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
        private const string _source = "NET";
        private const uint _currentProtocolVer = 65579;
        private const uint _obfuscationMask = 0xBAADF00D;
        
        private readonly SemaphoreSlim _stateLock;
        private readonly JobManager<NetworkMessage> _jobs;
        private readonly ConnectionManager _connection;
        private readonly ISocketClient _socket;
        private readonly bool _isWebSocket;
        private readonly Dictionary<MessageType, MessageReceiver> _eventDispatchers = new Dictionary<MessageType, MessageReceiver>();
        private int _connectTimeout;
        private uint _defaultCellId;
        private bool _encryptionPending = false;
        private IEncryptor _encryption;
        private IPAddress _localIp;
        private ServerList _currentServers;
        private bool _firstConnect = true;
        private Task _heartBeatTask;
        private CancellationTokenSource _heartbeatCancel;
        private bool _continueLogin;
        // login continuation
        private NetworkMessage _logonContinuation;
        private LogonResponse _previousLogonResponse;
        private Dictionary<int, GameCoordinator> _gameCoordinators = new Dictionary<int, GameCoordinator>();
        private TaskCompletionSource<object> _loginPromise;

        /// <summary>
        /// Gets a collection of all game coordinators attached to this client
        /// </summary>
        public IReadOnlyCollection<GameCoordinator> GameCoordinators { get; }
        
        public ConnectionState ConnectionState { get; private set; }
        
        public int SessionId { get; private set; }

        public SteamId SteamId { get; private set; }
        
        public SteamNetworkClient() : this(new SteamNetworkConfig()) { }
        
        public SteamNetworkClient(SteamNetworkConfig config) : base(config)
        {
            _socket = config.SocketClient(ReceiveAsync, OnConnected, OnSocketDisconnectedAsync);
            _connectTimeout = config.NetworkConnectionTimeout;
            _isWebSocket = _socket is IWebSocketClient;
            _defaultCellId = config.CellId > uint.MaxValue || config.CellId < uint.MinValue ? 0 : (uint)config.CellId;
            _stateLock = new SemaphoreSlim(1, 1);
            _currentServers = new ServerList(_defaultCellId, GetInterface<ISteamDirectory>(), GetConfig<SteamNetworkConfig>().WebSockets ?? Enumerable.Empty<Uri>(), GetConfig<SteamNetworkConfig>().ConnectionManagers ?? Enumerable.Empty<IPEndPoint>());
            _jobs = new JobManager<NetworkMessage>(LogManager);
            _connection = new ConnectionManager(_stateLock, LogManager, GetConfig<SteamNetworkConfig>().NetworkConnectionTimeout, OnConnectingAsync, OnConnected, OnDisconnectingAsync, x => Disconnected += x);
            _connection.Disconnected += OnDisconnected;

            IReceiveMethodResolver resolver;
            if (config.ReceiveMethodResolver == null)
            {
                resolver = new DefaultReceiveMethodResolver();
            }
            else
            {
                resolver = config.ReceiveMethodResolver() ?? new DefaultReceiveMethodResolver();
            }

            foreach(MethodInfo method in GetType().GetMethods())
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
            _eventDispatchers[type] += receiver;
        }

        /// <summary>
        /// Unsubscribes the specified receiver from messages of the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="receiver"></param>
        public void Unsubscribe(MessageType type, MessageReceiver receiver)
        {
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
        public async Task StopAsync()
        {
            if (_gameCoordinators.Count != 0)
            {
                foreach (var gc in _gameCoordinators)
                {
                    await gc.Value.StopAsync();
                }
            }

            await _connection.StopAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Connects this client to a connection manager 
        /// </summary>
        /// <returns></returns>
        private async Task OnConnectingAsync()
        {
            if (_continueLogin && LoginActionRequested.GetInvocationList().Length != 0)
            {
                LogDebug(_source, "Previous login failed and LoginActionRequested has subscribers, running event before resuming connection");
                LoginActionRequested?.Invoke(this, EventArgs.Empty);
            }

            if (_firstConnect)
            {
                LogInfo(_source, $"Steam.Net");
                _firstConnect = false;
            }

            List<Win32Exception> exceptions = new List<Win32Exception>();

            if (_isWebSocket)
            {
                IWebSocketClient webSocket = _socket as IWebSocketClient;
                do
                {
                    Uri endpoint = await _currentServers.GetCurrentWebSocketConnectionManagerAsync();
                    try
                    {
                        await webSocket.ConnectAsync(endpoint);
                        return;
                    }
                    catch (WebSocketException e)
                    {
                        LogError(_source, $"Could not connect to {endpoint}: {e.ErrorCode} {e.Message}");
                        _currentServers.MarkCurrentWebSocket();
                        exceptions.Add(e);
                    }
                } while (_currentServers.HasValidWebSocketManagers);
            }
            else
            {
                do
                {
                    IPEndPoint endpoint = await _currentServers.GetCurrentConnectionManagerAsync();
                    try
                    {
                        await _socket.ConnectAsync(endpoint, _connectTimeout);
                        return;
                    }
                    catch (SocketException e)
                    {
                        LogWarning(_source, $"Could not connect to {endpoint}: {e.GetType()} {e.Message}");
                        _currentServers.MarkCurrent();
                        exceptions.Add(e);
                    }
                } while (_currentServers.HasValidManagers);
            }

            throw new AggregateException("Could not connect to any of the specified, retreived, or fallback endpoints", exceptions);
        }

        private async Task OnDisconnectingAsync(Exception ex)
        {
            await _socket.DisconnectAsync().ConfigureAwait(false);
            _localIp = null;

            ConnectionState = ConnectionState.Disconnected;
        }

        private async Task OnConnected()
        {
            _localIp = _socket.LocalIp;

            if (_isWebSocket)
                await ConnectedAsync();
        }

        private void OnDisconnected(object sender, DisconnectedEventArgs exception)
        {
            _encryption = null;
            Disconnected?.Invoke(this, exception.Exception);
            _connection.Error(exception.Exception);
        }

        private Task OnSocketDisconnectedAsync(Exception ex)
        {
            LogDebug(_source, "Socket disconnected. The connection manager will handle reconnecting soon.");
            return Task.CompletedTask;
        }

        private async Task ConnectedAsync()
        {
            await _connection.CompleteAsync().ConfigureAwait(false);
            Connected?.Invoke(this, new EventArgs());

            if (!_continueLogin)
            {
                if (_loginPromise != null)
                {
                    _loginPromise.SetResult(new object());
                }
                else
                {
                    CanLogin?.Invoke(this, new EventArgs());
                }
            }
            else
            {
                LogDebug(_source, "Can continue login from previous disconnect, sending previous info");
                await ContinueLoginAsync().ConfigureAwait(false);
            }
        }
        
        private async Task ContinueLoginAsync()
        {
            await SendAsync(_logonContinuation);
            _logonContinuation = null;
            _continueLogin = false;
        }
        
        // in exchange for a gc instance to track + a name, we give back a log manager
        internal LogManager AttachGC(GameCoordinator gc)
        {
            _gameCoordinators.Add(gc.AppId, gc);
            return LogManager;
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

            LogInfo(_source, $"Logging in as {username ?? (instance == 0 ? "an anonymous user" : "a console user")}");
            Task<byte[]> machineIdTask = HardwareUtils.GetMachineId(); // while we set up the logon object, we will start to get the machine ID

            var body = new LogonRequest
            {
                protocol_version = 65579,
                client_os_type = (uint)(accountId == 0 ? HardwareUtils.GetCurrentOsType() : OsType.PS3),
                client_language = GetConfig<SteamNetworkConfig>().Language.GetApiLanguageCode(),
                cell_id = (uint)GetConfig<SteamNetworkConfig>().CellId,
                machine_id = await machineIdTask
            };
            
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
                body.obfustucated_private_ip = (uint)(GetConfig<SteamNetworkConfig>().LoginId < 0 ? _socket.LocalIp.ToUInt32() ^ _obfuscationMask : GetConfig<SteamNetworkConfig>().LoginId);
                body.supports_rate_limit_response = true;
            }
            
            var logon = NetworkMessage.CreateProtobufMessage(MessageType.ClientLogon, body);

            (logon.Header as ClientHeader).SteamId = new SteamId(accountId, GetConfig<SteamNetworkConfig>().DefaultUniverse, accountType, instance);

            await SendAsync(logon).ConfigureAwait(false);

            _logonContinuation = logon;
        }
        
        public async Task<Result> RequestValidationEmailAsync()
        {
            GenericResponse response = await SendJobAsync<GenericResponse>(NetworkMessage.CreateProtobufMessage(MessageType.ClientRequestValidationMail, null));
            return response.Result;
        }

        /// <summary>
        /// Continues a denied login request using the provided code as an auth or two factor code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public void ContinueLogin(string code)
        {
            if (_logonContinuation == null || _previousLogonResponse == null)
                throw new InvalidOperationException("Can't continue previous logon, original logon request or response doesn't exist");

            switch ((Result)_previousLogonResponse.Result)
            {
                case Result.AccountLogonDeniedVerifiedEmailRequired:
                    (_logonContinuation.Body as LogonRequest).auth_code = code;
                    break;
                case Result.AccountLoginDeniedNeedTwoFactor:
                case Result.AccountLogonDenied:
                    (_logonContinuation.Body as LogonRequest).two_factor_code = code;
                    break;
                default:
                    throw new InvalidOperationException("Previous logon error was not denied for verification email or two factor failure");
            }

            _previousLogonResponse = null;
        }

        /// <summary>
        /// Sets the currently playing game to the specified app ID
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public async Task SetPlayingGameAsync(int appId)
        {
            GameId id = new GameId(appId, GameType.App, 0);

            var message = NetworkMessage.CreateProtobufMessage(MessageType.ClientGamesPlayed, new ClientGamesPlayed
            {
                OsType = (uint)HardwareUtils.GetCurrentOsType(),
                GamesPlayed = new List<GamePlayed>
                {
                    new GamePlayed
                    {
                        GameId = id
                    }
                }
            });

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

            var message = NetworkMessage.CreateProtobufMessage(MessageType.ClientGamesPlayed, new ClientGamesPlayed 
            { 
               OsType = (uint)HardwareUtils.GetCurrentOsType(),
               GamesPlayed = new List<GamePlayed>
               {
                   new GamePlayed
                   {
                       GameId = id,
                       ExtraGameInfo = game
                   }
               }
            });

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
            
            var message = NetworkMessage.CreateProtobufMessage(MessageType.ClientGamesPlayed, new ClientGamesPlayed 
            { 
               OsType = (uint)HardwareUtils.GetCurrentOsType(),
               GamesPlayed = new List<GamePlayed>
               {
                   new GamePlayed
                   {
                       GameId = id
                   }
               }
            });

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
        /// </para>If the response result is not <see cref="Result.OK"/>, the returned value is <value>-1</value> and the result code is logged
        /// </para>
        /// </returns>
        public async Task<int> GetNumberOfCurrentPlayersAsync(long appId)
        {
            if (appId < uint.MinValue || appId > uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(appId));

            var message = NetworkMessage.CreateProtobufMessage(MessageType.ClientGetNumberOfCurrentPlayersDP, new DPGetNumberOfCurrentPlayers {AppId = (uint) appId});
            var response = await SendJobAsync<DPGetNumberOfCurrentPlayersResponse>(message);

            if ((Result)response.Result != Result.OK)
            {
                LogInfo(_source, $"A request for the current player count of app ID {appId} did not complete succesfully. Result: {(Result) response.Result}");
                return -1;
            }
            else
                return response.PlayerCount;
        }
        
        internal async Task SendGameCoordinatorMessage(int appid, GameCoordinatorMessage gcmessage)
        {
            GameCoordinatorClientMessage body = new GameCoordinatorClientMessage()
            {
                MessageType = MessageTypeUtils.MergeMessage((uint)gcmessage.MessageType, gcmessage.Protobuf),
                AppId = (uint)appid,
                Payload = gcmessage.Serialize()
            };
            
            await SendAsync(NetworkMessage.CreateAppRoutedMessage(MessageType.ClientToGC, body, appid)).ConfigureAwait(false);
        }
        
        private async Task RunHeartbeatAsync(int interval, CancellationToken token)
        {
            NetworkMessage message = NetworkMessage.CreateProtobufMessage(MessageType.Heartbeat, new Heartbeat());
            try
            {
                LogDebug(_source, $"Heartbeat started on a {interval} ms interval");
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(interval, token).ConfigureAwait(false);

                    await SendAsync(message).ConfigureAwait(false);
                    LogDebug(_source, "Sent heartbeat");
                }
            }
            catch (OperationCanceledException)
            {
                LogDebug(_source, "Heartbeat stopped");
            }
            catch (Exception ex)
            {
                LogError(_source, $"The heartbeat task encountered an unknown exception: {ex}");
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
                if (SessionId > 0)
                    clientHeader.SessionId = SessionId;

                if (SteamId > 0 && clientHeader.SteamId == SteamId.Zero)
                    clientHeader.SteamId = SteamId;
            }

            byte[] data = message.Serialize();

            if (_encryption != null && !_encryptionPending)
                data = _encryption.Encrypt(data);

            LogDebug(_source, $"Sending message of message type {message.MessageType} as a {(message.Protobuf ? "protobuf" : "struct")}. Body object type is {message.Body.GetType()}. Resulting packet is {data.Length} bytes long.");

            await _socket.SendAsync(data).ConfigureAwait(false);
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
                return (T) (object) await SendJobAsync(message).ConfigureAwait(false); // ok c#

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
        
        private byte[] Decrypt(byte[] data)
        {
            if (_encryption != null && !_encryptionPending)
                return _encryption.Decrypt(data);
            else
                return data;
        }
        
        private Task ReceiveAsync(byte[] data)
        {
            data = Decrypt(data);
            DispatchData(data).ContinueWith(ContinueDispatch);
            return Task.CompletedTask;
        }

        private void ContinueDispatch(Task task)
        {
            if (task.IsFaulted)
            {
                LogError(_source, $"The dispatcher threw an exception: {(task.Exception.InnerExceptions.Count == 1 ? task.Exception.InnerException.ToString() : task.Exception.ToString())}");
            }
        }

        private async Task DispatchData(byte[] data)
        {
            NetworkMessage message = NetworkMessage.Deserialize(data);
            
            if(_jobs.IsRunningJob(message.Header.JobId))
                _jobs.SetJobResult(message, message.Header.JobId);

            if (_eventDispatchers.TryGetValue(message.MessageType, out var dispatch))
            {
                foreach (MessageReceiver dispatcher in dispatch.GetInvocationList())
                {
                    try
                    {
                        await dispatcher(message).ConfigureAwait(false);
                    }
                    catch (Exception e) // I don't trust anyone that uses my lib
                    {
                        LogError(_source, $"A message receiver threw an exception: {e}");
                    }
                }
            }
        }

        #endregion
    }
}
