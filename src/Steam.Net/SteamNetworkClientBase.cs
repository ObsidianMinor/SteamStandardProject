using Steam.Logging;
using Steam.Net.GameCoordinators;
using Steam.Net.Messages;
using Steam.Net.Messages.Protobufs;
using Steam.Net.Messages.Structs;
using Steam.Net.Sockets;
using Steam.Net.Utilities;
using Steam.Rest;
using Steam.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Net
{
    public abstract class SteamNetworkClientBase : SteamWebClient
    {
        private static readonly NoEncryptor _defaultEncryptor = new NoEncryptor();
        private readonly SteamId _defaultSteamId;

        private readonly SemaphoreSlim _stateLock;
        private readonly SemaphoreSlim _connectionStateLock;
        private readonly JobManager<NetworkMessage> _jobs;
        private readonly ConnectionManager _connection;
        private readonly ISocketClient Socket;
        private readonly Dictionary<MessageType, MessageReceiver> _eventDispatchers = new Dictionary<MessageType, MessageReceiver>();
        private readonly Dictionary<MessageType, MessageReceiver> _highPriorityDispatchers = new Dictionary<MessageType, MessageReceiver>();
        private IEncryptor _encryptor;
        private Dictionary<int, GameCoordinator> _gameCoordinators = new Dictionary<int, GameCoordinator>();
        private Func<Exception, Task> _socketDisconnected;
        private List<Server> _connectionManagers;
        private int _serverIndex = -1;
        private CancellationTokenSource _connectCancellationToken;

        private SteamId _steamId;
        private int _sessionId;

        public SteamId SteamId => _steamId == SteamId.Zero ? _defaultSteamId : _steamId;

        public int SessionId => _sessionId;

        /// <summary>
        /// The client is connected to a connection manager
        /// </summary>
        public event AsyncEventHandler Connected;

        /// <summary>
        /// The client has been disconnected from the connection manager
        /// </summary>
        public event AsyncEventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        /// Gets the client's current connection state
        /// </summary>
        public ConnectionState ConnectionState => _connection.State;

        /// <summary>
        /// Gets a collection of all game coordinators attached to this client
        /// </summary>
        public IReadOnlyCollection<GameCoordinator> GameCoordinators => _gameCoordinators.Values;

        /// <summary>
        /// Gets the logger for this network client
        /// </summary>
        protected internal Logger NetLog { get; }
        
        protected Server CurrentServer { get; set; }

        internal IReceiveMethodResolver Resolver { get; }

        protected internal int TaskTimeout => GetConfig<SteamNetworkConfig>().ReceiveMethodTimeout;

        internal int ConnectionTimeout => GetConfig<SteamNetworkConfig>().NetworkConnectionTimeout;
        
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

        public IPAddress LocalIp => Socket.LocalIp;

        protected SteamNetworkClientBase(SteamNetworkConfig config) : base(config)
        {
            config = GetConfig<SteamNetworkConfig>();
            Socket = config.SocketClient();
            Socket.MessageReceived += ReceiveAsync;
            Socket.Disconnected += async (_, ex) =>
            {
                await DisconnectAsync().ConfigureAwait(false);
                await _socketDisconnected(ex.Exception).ConfigureAwait(false);
            };
            _stateLock = new SemaphoreSlim(1, 1);
            _connectionStateLock = new SemaphoreSlim(1, 1);
            NetLog = LogManager.CreateLogger("Net");
            _jobs = new JobManager<NetworkMessage>(NetLog);
            _connectionManagers = new List<Server>();
            
            _connection = new ConnectionManager(_connectionStateLock, LogManager.CreateLogger("CM"), ConnectionTimeout,
                OnConnectingInternalAsync, OnDisconnectingInternalAsync, (x) => _socketDisconnected = x);

            _defaultSteamId = SteamId.CreateAnonymousUser(config.DefaultUniverse);
            
            _connection.Disconnected += async (_, args) => 
            {
                await Disconnected.TimedInvokeAsync(this, args, TaskTimeout, NetLog).ConfigureAwait(false);
            };
            _connection.Connected += async (_, __) =>
            {
                await Connected.TimedInvokeAsync(this, EventArgs.Empty, TaskTimeout, NetLog).ConfigureAwait(false);
            };

            HighPrioritySubscribe(MessageType.Multi, ProcessMulti);
            HighPrioritySubscribe(MessageType.ChannelEncryptRequest, ProcessEncryptRequest);
            HighPrioritySubscribe(MessageType.ChannelEncryptResult, ProcessEncryptResult);
            HighPrioritySubscribe(MessageType.JobHeartbeat, ProcessJobHeartbeat);
            HighPrioritySubscribe(MessageType.DestJobFailed, ProcessFailedJob);

            Resolver = config.ReceiveMethodResolver == null ? new DefaultReceiveMethodResolver() : config.ReceiveMethodResolver() ?? new DefaultReceiveMethodResolver();
            
            foreach (MethodInfo method in this.GetAllTypes().Select(t => t.GetTypeInfo()).SelectMany(t => t.DeclaredMethods))
            {
                var attribute = method.GetCustomAttribute<MessageReceiverAttribute>();
                if (attribute != null)
                {
                    if (Resolver.TryResolve(method, this, out MessageReceiver receiver))
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

        protected void HighPrioritySubscribe(MessageType type, MessageReceiver receiver)
        {
            if (!_highPriorityDispatchers.ContainsKey(type))
                _highPriorityDispatchers[type] = receiver;
            else
                _highPriorityDispatchers[type] += receiver;
        }

        protected void HighPriorityUnsubscribe(MessageType type, MessageReceiver receiver)
        {
            if (!_highPriorityDispatchers.ContainsKey(type))
                _highPriorityDispatchers[type] -= receiver;
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
        private async Task OnConnectingInternalAsync()
        {
            await NetLog.DebugAsync("Connecting client").ConfigureAwait(false);
            await ConnectAsync().ConfigureAwait(false);

            if (!(Socket is IWebSocketClient))
            {
                await NetLog.DebugAsync("Waiting for encryption").ConfigureAwait(false);
                await _connection.WaitAsync().ConfigureAwait(false);
            }

            await OnConnectedAsync().ConfigureAwait(false);
            
            await _connection.CompleteAsync().ConfigureAwait(false);
        }

        protected virtual Task OnConnectedAsync() => Task.CompletedTask;

        private async Task OnDisconnectingInternalAsync(Exception ex)
        {
            await NetLog.DebugAsync("Cancelling all jobs").ConfigureAwait(false);
            await _jobs.CancelAllJobs().ConfigureAwait(false);

            await OnDisconnectingAsync(ex).ConfigureAwait(false);

            await NetLog.DebugAsync("Disconnecting client").ConfigureAwait(false);
            await DisconnectAsync().ConfigureAwait(false);
        }

        protected virtual Task OnDisconnectingAsync(Exception ex) => Task.CompletedTask;

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

        protected virtual async Task<Server> GetConnectionServerAsync()
        {
            while (!AdvanceToMatchingServerIndex(s => s.IsEndPoint))
            {
                _connectionManagers.Clear();

                var cms = GetConfig<SteamNetworkConfig>().ConnectionManagers;
                _connectionManagers.AddRange(cms?.Select(cm => new Server(cm)) ?? Enumerable.Empty<Server>());
                if (_connectionManagers.Count == 0)
                {
                    ISteamDirectory directory = GetInterface<ISteamDirectory>();
                    try
                    {
                        var cmList = await directory.GetConnectionManagerListAsync(GetConfig<SteamNetworkConfig>().CellId).ConfigureAwait(false);
                        _connectionManagers.AddRange(cmList.Response.ServerList.Select(cm => new Server(cm)));
                    }
                    catch (HttpException) { }
                }
            }

            return _connectionManagers[_serverIndex];
        }
        
        protected virtual async Task<Server> GetWebSocketConnectionServerAsync()
        {
            while (!AdvanceToMatchingServerIndex(s => s.IsUri))
            {
                _connectionManagers.Clear();

                IEnumerable<Uri> webSockets = GetConfig<SteamNetworkConfig>().WebSockets;
                _connectionManagers.AddRange(webSockets?.Select(cm => new Server(cm)) ?? Enumerable.Empty<Server>());
                if (_connectionManagers.Count == 0)
                {
                    ISteamDirectory directory = GetInterface<ISteamDirectory>();
                    try
                    {
                        var cmList = await directory.GetConnectionManagerListAsync(GetConfig<SteamNetworkConfig>().CellId).ConfigureAwait(false);
                        _connectionManagers.AddRange(cmList.Response.WebSocketServerList.Select(cm => new Server(cm)));
                    }
                    catch (HttpException) { } // go back around and try again
                }
            }

            return _connectionManagers[_serverIndex];
        }

        private bool AdvanceToMatchingServerIndex(Func<Server, bool> predicate)
        {
            // move index up by one, since if we're here that means we need a new server, and the current index satisfies.
            // move index up by one while the server index is smaller than the CM count and the predicate function returns false
            for (_serverIndex++; _serverIndex < _connectionManagers.Count && !predicate(_connectionManagers[_serverIndex]); _serverIndex++) { }
            
            // if the server index is over the connection manager count or there are no connection managers, set the index to -1
            if (_serverIndex > _connectionManagers.Count || _connectionManagers.Count == 0)
                _serverIndex = -1;

            // then return if the index isn't -1
            return _serverIndex != -1;
        }
        
        private async Task ConnectInternalAsync()
        {
            _connectCancellationToken = new CancellationTokenSource();
            Socket.SetCancellationToken(_connectCancellationToken.Token);
            if (Socket is IWebSocketClient webSocketClient)
            {
                CurrentServer = await GetWebSocketConnectionServerAsync().ConfigureAwait(false);

                await NetLog.InfoAsync($"Connecting to WebSocket {CurrentServer}").ConfigureAwait(false);
                await webSocketClient.ConnectAsync(new Uri($"wss://{CurrentServer.ToString()}/cmsocket/")).ConfigureAwait(false);
            }
            else
            {
                CurrentServer = await GetConnectionServerAsync().ConfigureAwait(false);

                await NetLog.InfoAsync($"Connecting to endpoint {CurrentServer}").ConfigureAwait(false);
                await Socket.ConnectAsync(CurrentServer.GetIPEndPoint()).ConfigureAwait(false);
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
        
        /// <summary>
        /// Sends a message to Steam
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <returns>An awaitable task</returns>
        protected internal async Task SendAsync(NetworkMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            await NetLog.VerboseAsync($"Sending {(message.Protobuf ? "protobuf" : "struct")} message with type {message.MessageType}");

            await SendAsync(message.WithClientInfo(_steamId, _sessionId).Serialize()).ConfigureAwait(false);
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

            await NetLog.DebugAsync($"Sending {message.Length} byte message.").ConfigureAwait(false);

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

        private async Task ReceiveAsync(object sender, DataReceivedEventArgs args)
        {
            await DispatchData(Encryption.Decrypt(args.Data)).ConfigureAwait(false);
        }

        private async Task DispatchData(byte[] data)
        {
            NetworkMessage message = NetworkMessage.CreateFromByteArray(data);
            bool received = false;

            UpdateSessionInfo(message);

            if (_highPriorityDispatchers.TryGetValue(message.MessageType, out var highPriorityDispatch))
            {
                foreach (MessageReceiver dispatcher in highPriorityDispatch.GetInvocationList())
                {
                    received = true;
                    await dispatcher(message).ConfigureAwait(false);
                }
            }
            
            if (_jobs.IsRunningJob(message.Header.JobId))
                await _jobs.SetJobResult(message, message.Header.JobId).ConfigureAwait(false);

            if (_eventDispatchers.TryGetValue(message.MessageType, out var dispatch))
            {
                foreach (MessageReceiver dispatcher in dispatch.GetInvocationList())
                {
                    received = true;
                    await dispatcher(message).TimeoutWrap(TaskTimeout, NetLog).ConfigureAwait(false);
                }
            }

            if (!received)
                await NetLog.DebugAsync($"No receiver found for message type {message.MessageType} ({(int)message.MessageType})").ConfigureAwait(false);
        }

        private async Task ProcessMulti(NetworkMessage message)
        {
            CMsgMulti multi = message.Deserialize<CMsgMulti>();
            byte[] payload = multi.message_body;
            if (multi.size_unzipped > 0)
            {
                using (MemoryStream compressedStream = new MemoryStream(payload))
                using (GZipStream decompressionStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                using (MemoryStream decompressedStream = new MemoryStream())
                {
                    await decompressionStream.CopyToAsync(decompressedStream).ConfigureAwait(false);
                    payload = decompressedStream.ToArray();
                }
            }

            using (MemoryStream stream = new MemoryStream(payload))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                while (stream.Length - stream.Position != 0)
                {
                    int subSize = reader.ReadInt32();
                    byte[] subData = reader.ReadBytes(subSize);

                    await DispatchData(subData).ConfigureAwait(false);
                }
            }
        }

        private async Task ProcessEncryptRequest(NetworkMessage message)
        {
            ChannelEncryptRequest encryptRequest = message.Deserialize<ChannelEncryptRequest>();
            await NetLog.VerboseAsync($"Encrypting channel on protocol version {encryptRequest.ProtocolVersion} in universe {encryptRequest.Universe}").ConfigureAwait(false);

            byte[] challange = encryptRequest.Challenge.All(b => b == 0) ? encryptRequest.Challenge : null; // check if all the values were made 0 by the marshal
            byte[] publicKey = UniverseUtils.GetPublicKey(encryptRequest.Universe);
            if (publicKey == null)
            {
                await NetLog.ErrorAsync($"Cannot find public key for universe {encryptRequest.Universe}").ConfigureAwait(false);
                throw new InvalidOperationException($"Public key does not exist for universe {encryptRequest.Universe}");
            }

            byte[] tempSessionKey = CryptoUtils.GenerateBytes(32);
            byte[] encryptedHandshake = null;

            using (RsaCrypto rsa = new RsaCrypto(publicKey))
            {
                if (challange != null)
                {
                    byte[] handshakeToEncrypt = new byte[tempSessionKey.Length + challange.Length];
                    Array.Copy(tempSessionKey, handshakeToEncrypt, tempSessionKey.Length);
                    Array.Copy(challange, 0, handshakeToEncrypt, tempSessionKey.Length, challange.Length);

                    encryptedHandshake = rsa.Encrypt(handshakeToEncrypt);
                }
                else
                {
                    encryptedHandshake = rsa.Encrypt(tempSessionKey);
                }
            }
            
            Encryption = challange != null ? (IEncryptor)new HmacEncryptor(tempSessionKey) : new SimpleEncryptor(tempSessionKey);

            var encryptResponse = NetworkMessage.CreateMessage(MessageType.ChannelEncryptResponse, new ChannelEncryptResponse 
            { 
                KeySize = 128,
                KeyHash =  CryptoUtils.CrcHash(encryptedHandshake),
                EncryptedHandshake = encryptedHandshake,
                ProtocolVersion = 1,
            });
            await SendAsync(encryptResponse).ConfigureAwait(false);
        }

        private async Task ProcessEncryptResult(NetworkMessage message)
        {
            ChannelEncryptResult encryptResult = message.Deserialize<ChannelEncryptResult>();
            if (encryptResult.Result == Result.OK)
            {
                await NetLog.DebugAsync("Channel encrypted").ConfigureAwait(false);
                await _connection.CompleteAsync().ConfigureAwait(false);
            }
        }

        private async Task ProcessJobHeartbeat(NetworkMessage message)
        {
            await _jobs.HeartbeatJob(message.Header.JobId).ConfigureAwait(false);
        }

        private async Task ProcessFailedJob(NetworkMessage message)
        {
            await _jobs.SetJobFail(message.Header.JobId, new DestinationJobFailedException(message.Header.JobId)).ConfigureAwait(false);
        }

        private void UpdateSessionInfo(NetworkMessage message)
        {
            if (message.Header is ClientHeader clientHeader)
            {
                _steamId = clientHeader.SteamId;
                _sessionId = clientHeader.SessionId;
            }
        }
    }
}