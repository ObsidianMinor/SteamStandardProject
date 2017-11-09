using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Steam.Logging;
using Steam.Net.Messages;
using Steam.Net.Messages.Protobufs;
using Steam.Net.GameCoordinators.Messages;
using Steam.Net.Utilities;

namespace Steam.Net.GameCoordinators
{
    /// <summary>
    /// Represents an abstract game coordinator client
    /// </summary>
    public abstract class GameCoordinator : NetEntity<SteamNetworkClientBase>
    {
        private readonly JobManager<GameCoordinatorMessage> _jobs;
        private readonly Dictionary<GameCoordinatorMessageType, GameCoordinatorReceiver> _dispatchers;
        private readonly ConnectionManager _connectionManager;
        private readonly Logger _log;
        private readonly SemaphoreSlim _stateLock;

        public AsyncEventHandler Connected;

        public AsyncEventHandler<DisconnectedEventArgs> Disconnected;

        public int AppId { get; }
        
        protected GameCoordinator(SteamNetworkClientBase client, int appId) : base(client)
        {
            if(appId < uint.MinValue)
                throw new ArgumentOutOfRangeException(nameof(appId));

            _log = Client.NetLog;
            _dispatchers = new Dictionary<GameCoordinatorMessageType, GameCoordinatorReceiver>();
            _jobs = new JobManager<GameCoordinatorMessage>(_log);
            _stateLock = new SemaphoreSlim(1, 1);
            _connectionManager = new ConnectionManager(_stateLock, _log, Client.ConnectionTimeout, OnConnectingAsync, OnDisconnecting, x => );
            
            AppId = appId;

            foreach (MethodInfo method in this.GetAllTypes().Select(t => t.GetTypeInfo()).SelectMany(t => t.DeclaredMethods))
            {
                var attribute = method.GetCustomAttribute<GameCoordinatorReceiverAttribute>();
                if (attribute != null)
                {
                    if (Client.Resolver.TryResolve(method, this, out GameCoordinatorReceiver receiver))
                        Subscribe(attribute.Type, receiver);
                }
            }
        }

        protected virtual async Task OnConnectingAsync()
        {

        }

        protected virtual async Task OnDisconnecting(Exception ex)
        {

        }

        private async Task ConnectInternalAsync()
        {
            await _stateLock.WaitAsync().ConfigureAwait(false);
            try
            {
                await ConnectAsync().ConfigureAwait(false);
            }
            finally
            {
                _stateLock.Release();
            }
        }

        private async Task DisconnectInernalAsync(Exception ex)
        {
            await _stateLock.WaitAsync().ConfigureAwait(false);
            try
            {
                await DisconnectAsync(ex).ConfigureAwait(false);
            }
            finally
            {
                _stateLock.Release();
            }
        }

        protected abstract Task ConnectAsync();

        protected abstract Task DisconnectAsync(Exception ex);

        /// <summary>
        /// Subscribes the specified receiver to messages of the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="receiver"></param>
        public void Subscribe(GameCoordinatorMessageType type, GameCoordinatorReceiver receiver)
        {
            if (!_dispatchers.ContainsKey(type))
                _dispatchers[type] = receiver;
            else
                _dispatchers[type] += receiver;
        }

        /// <summary>
        /// Unsubscribes the specified receiver from messages of the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="receiver"></param>
        public void Unsubscribe(GameCoordinatorMessageType type, GameCoordinatorReceiver receiver)
        {
            if (!_dispatchers.ContainsKey(type))
                _dispatchers[type] -= receiver;
        }

        internal async Task DispatchToReceiver(GameCoordinatorMessage message)
        {
            if (_dispatchers.TryGetValue(message.MessageType, out var value))
            {
                foreach (GameCoordinatorReceiver dispatch in value.GetInvocationList())
                {
                    try
                    {
                        await dispatch(message).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        await _log.ErrorAsync($"A game coordinator message receiver threw an exception: {e}").ConfigureAwait(false);
                    }
                }
            }
            else
            {
                await _log.VerboseAsync($"Received unknown game coordinator message of type {message.MessageType} ({(int)message.MessageType})").ConfigureAwait(false);
            }
        }

        protected internal async Task SendAsync(GameCoordinatorMessageType messageType, bool protobuf, byte[] data)
        {
            await SendAsync(NetworkMessage
                .CreateAppRoutedMessage(MessageType.ClientToGC,
                new CMsgGCClient 
                { 
                    appid = (uint)AppId,
                    msgtype = MessageTypeUtils.MergeMessage((uint)messageType, protobuf),
                    payload = data
                },
                AppId));
        }
        
        /// <summary>
        /// Sends a message to the game coordinator as an async task
        /// </summary>
        /// <returns></returns>
        protected internal async Task SendAsync(GameCoordinatorMessage message)
        {
            await SendAsync(message.MessageType, message.Protobuf, message.Serialize()).ConfigureAwait(false);
        }
        
        /// <summary>
        /// Sends a message to the game coordinator as a job, waits for the job to complete, and returns the response body as an async task
        /// </summary>
        /// <returns></returns>
        protected internal async Task<T> SendJobAsync<T>(GameCoordinatorMessage message)
        {
            if (typeof(T) == typeof(GameCoordinatorMessage))
                return (T)(object)await SendJobAsync(message).ConfigureAwait(false); // ok c#
            
            GameCoordinatorMessage response = await SendJobAsync(message).ConfigureAwait(false);

            return response.Deserialize<T>();
        }

        /// <summary>
        /// Sends a message to the game coordinator as a job, waits for the job to complete, and returns the response message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected internal async Task<GameCoordinatorMessage> SendJobAsync(GameCoordinatorMessage message)
        {
            (var task, var job) = _jobs.AddJob();

            await SendAsync(message.WithJobId(job)).ConfigureAwait(false);
            return await task.ConfigureAwait(false);
        }

        /// <summary>
        /// Informs Steam we're starting the client for this game coordinator
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            await _connectionManager.StartAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Informs Steam we've stopped the client for this game coordinator
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            await _connectionManager.StartAsync().ConfigureAwait(false);
        }
    }
}
