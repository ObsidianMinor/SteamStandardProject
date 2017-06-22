using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Steam.Common.Logging
{
    /// <summary>
    /// A log manager
    /// </summary>
    public class LogManager
    {
        /// <summary>
        /// The source of this log manager
        /// </summary>
        public string Source { get; }

        private readonly LogSeverity _severity;

        public event EventHandler<LogMessage> Log;

        public LogManager(string source, LogSeverity severity)
        {
            Source = source;
            _severity = severity;
        }

        public async Task LogCriticalAsync(string message)
        {
            await LogAsync(message, LogSeverity.Critical).ConfigureAwait(false);
        }

        public async Task LogErrorAsync(string message)
        {
            await LogAsync(message, LogSeverity.Error).ConfigureAwait(false);
        }

        public async Task LogWarningAsync(string message)
        {
            await LogAsync(message, LogSeverity.Warning).ConfigureAwait(false);
        }

        public async Task LogInfoAsync(string message)
        {
            await LogAsync(message, LogSeverity.Info).ConfigureAwait(false);
        }

        public async Task LogVerboseAsync(string message)
        {
            await LogAsync(message, LogSeverity.Verbose).ConfigureAwait(false);
        }

        public async Task LogDebugAsync(string message)
        {
            await LogAsync(message, LogSeverity.Debug).ConfigureAwait(false);
        }

        public async Task LogAsync(string message, LogSeverity severity)
        {
            LogMessage logMessage = new LogMessage(Source, severity, message);
            if (severity <= _severity)
                Log?.Invoke(this, logMessage);

            Debug.WriteLine(logMessage);
        }
        
        public LogManager CreateLinkedManager(string source)
        {
            LogManager newManager = new LogManager(source, _severity);
            newManager.Log += PassLogMessage;
            return newManager;
        }

        private void PassLogMessage(object sender, LogMessage message)
        {
            Log?.Invoke(this, message);
        }
    }
}
