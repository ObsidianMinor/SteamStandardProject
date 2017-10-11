using Steam.Logging;
using System;

namespace Steam
{
    /// <summary>
    /// Represents a class that interacts with the Steam network
    /// </summary>
    public abstract class SteamClient
    {
        private readonly SteamConfig _config;
        private readonly LogManager _log;

        /// <summary>
        /// Creates a new <see cref="SteamClient"/> with the specified <see cref="SteamConfig"/>
        /// </summary>
        /// <param name="config"></param>
        public SteamClient(SteamConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            _config = config.Clone();
            _log = new LogManager(config.LogLevel);
        }

        /// <summary>
        /// Gets the config as the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T GetConfig<T>() where T : SteamConfig => _config as T;

        /// <summary>
        /// Gets the log manager for this client
        /// </summary>
        protected LogManager LogManager => _log;

        /// <summary>
        /// Occurs when a message of greater severity than the log level is logged
        /// </summary>
        public event EventHandler<LogMessage> Log
        {
            add => _log.Log += value;
            remove => _log.Log -= value;
        }

        /// <summary>
        /// Logs a debug message as the specified source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="message"></param>
        protected virtual void LogDebug(string source, string message)
        {
            _log.LogDebug(source, message);
        }

        /// <summary>
        /// Logs a verbose message as the specified source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="message"></param>
        protected virtual void LogVerbose(string source, string message)
        {
            _log.LogVerbose(source, message);
        }

        /// <summary>
        /// Logs an info message as the specified source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="message"></param>
        protected virtual void LogInfo(string source, string message)
        {
            _log.LogInfo(source, message);
        }

        /// <summary>
        /// Logs a warning message as the specified source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="message"></param>
        protected virtual void LogWarning(string source, string message)
        {
            _log.LogWarning(source, message);
        }

        /// <summary>
        /// Logs an error message as the specified source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="message"></param>
        protected virtual void LogError(string source, string message)
        {
            _log.LogError(source, message);
        }

        /// <summary>
        /// Logs a critical error message as the specified source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="message"></param>
        protected virtual void LogCritical(string source, string message)
        {
            _log.LogCritical(source, message);
        }
    }
}
