using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steam.Logging;
using Steam.Net.GameCoordinators.Messages;

namespace Steam.Net.GameCoordinators
{
    /// <summary>
    /// Represents an abstract game coordinator client
    /// </summary>
    public abstract class GameCoordinator
    {
        private readonly SteamNetworkClient _client;
        protected Logger Log { get; }
        private readonly JobManager<GameCoordinatorMessage> _jobs;
        private readonly Dictionary<GameCoordinatorMessageType, GameCoordinatorReceiver> _dispatchers;

        public int AppId { get; }
        
        protected GameCoordinator(SteamNetworkClient client, int appId)
        {
            if(appId < uint.MinValue)
                throw new ArgumentOutOfRangeException(nameof(appId));

            _client = client ?? throw new ArgumentNullException(nameof(client), "Can't attach GC to null client");

            _dispatchers = new Dictionary<GameCoordinatorMessageType, GameCoordinatorReceiver>();
            AppId = appId;
            (Log, _jobs) = client.AttachGC(this, "GC");
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
                        await Log.ErrorAsync($"A message receiver threw an exception: {e}").ConfigureAwait(false);
                    }
                }
            }
            else
            {
                await Log.VerboseAsync($"Received message of type {message.MessageType} ({(int)message.MessageType})").ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sends a message to the game coordinator as an async task
        /// </summary>
        /// <returns></returns>
        protected internal async Task SendAsync(GameCoordinatorMessage message)
        {
            await _client.SendGameCoordinatorMessage(AppId, message).ConfigureAwait(false);
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

        protected internal async Task<GameCoordinatorMessage> SendJobAsync(GameCoordinatorMessage message)
        {
            (var task, var job) = _jobs.AddJob();
            message.Header.JobId = job;

            await SendAsync(message).ConfigureAwait(false);
            return await task.ConfigureAwait(false);
        }

        /// <summary>
        /// Informs Steam we're starting the client for this game coordinator
        /// </summary>
        /// <returns></returns>
        public async virtual Task StartAsync()
        {
            if (_client.ConnectionState != ConnectionState.Connected)
                throw new InvalidOperationException("Could not start game coordinator: The connected client is not connected to Steam");

            await _client.SetPlayingGameAsync(AppId).ConfigureAwait(false);
        }

        /// <summary>
        /// Informs Steam we've stopped the client for this game coordinator
        /// </summary>
        /// <returns></returns>
        public async virtual Task StopAsync()
        {
            await _client.SetPlayingGameAsync(0).ConfigureAwait(false);
        }
    }
}
