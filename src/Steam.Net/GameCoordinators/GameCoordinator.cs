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
        private readonly LogManager _gcLog;
        private readonly JobManager<GameCoordinatorMessage> _jobs;
        private readonly Dictionary<GameCoordinatorMessageType, GameCoordinatorReceiver> _dispatchers;

        public int AppId { get; }
        
        protected GameCoordinator(SteamNetworkClient client, int appId)
        {
            if(appId < uint.MinValue)
                throw new ArgumentOutOfRangeException(nameof(appId));

            _dispatchers = new Dictionary<GameCoordinatorMessageType, GameCoordinatorReceiver>();
            AppId = appId;
            _client = client ?? throw new ArgumentNullException(nameof(client), "Can't attach GC to null client");
            _gcLog = client.AttachGC(this);
            _jobs = new JobManager<GameCoordinatorMessage>(_gcLog);
        }
        
        internal async Task DispatchToReceiver(GameCoordinatorMessage message)
        {
            if (_dispatchers.TryGetValue(message.MessageType, out var value))
            {
                foreach (GameCoordinatorReceiver dispatch in value.GetInvocationList())
                {
                    try
                    {
                        await dispatch(message);
                    }
                    catch (Exception e)
                    {
                        LogError("GC", $"A message receiver threw an exception: {e}");
                    }
                }
            }
            else
            {
                LogVerbose("GC", $"Received message of type {message.MessageType} ({(int)message.MessageType})");
            }
        }

        /// <summary>
        /// Sends a message to the game coordinator as an async task
        /// </summary>
        /// <returns></returns>
        protected internal async Task SendAsync(GameCoordinatorMessage message)
        {
            await _client.SendGameCoordinatorMessage(AppId, message);
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
            return await task;
        }

        /// <summary>
        /// Informs Steam we're starting the client for this game coordinator
        /// </summary>
        /// <returns></returns>
        public async virtual Task StartAsync()
        {
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
        
        protected virtual void LogDebug(string source, string message)
        {
            _gcLog.LogDebug(source, message);
        }

        protected virtual void LogVerbose(string source, string message)
        {
            _gcLog.LogVerbose(source, message);
        }

        protected virtual void LogInfo(string source, string message)
        {
            _gcLog.LogInfo(source, message);
        }

        protected virtual void LogWarning(string source, string message)
        {
            _gcLog.LogWarning(source, message);
        }

        protected virtual void LogError(string source, string message)
        {
            _gcLog.LogError(source, message);
        }

        protected virtual void LogCritcal(string source, string message)
        {
            _gcLog.LogCritical(source, message);
        }
    }
}
