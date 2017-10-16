using Steam.Logging;
using System;
using System.Threading.Tasks;

namespace Steam
{
    /// <summary>
    /// Represents a class that interacts with the Steam network
    /// </summary>
    public abstract class SteamClient
    {
        private readonly SteamConfig _config;

        /// <summary>
        /// Gets the <see cref="LogManager"/> to 
        /// </summary>
        protected LogManager LogManager { get; }
        private AsyncEvent<Func<LogMessage, Task>> _logEvent = new AsyncEvent<Func<LogMessage, Task>>();

        /// <summary>
        /// Creates a new <see cref="SteamClient"/> with the specified <see cref="SteamConfig"/>
        /// </summary>
        /// <param name="config"></param>
        public SteamClient(SteamConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            _config = config.Clone();
            LogManager = new LogManager(config.LogLevel);
            LogManager.Message += async msg => await _logEvent.InvokeAsync(msg).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the config as the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T GetConfig<T>() where T : SteamConfig => _config as T;
        
        /// <summary>
        /// Occurs when a message of greater severity than the log level is logged
        /// </summary>
        public event Func<LogMessage, Task> Log
        {
            add => _logEvent.Add(value);
            remove => _logEvent.Remove(value);
        }
    }
}
