using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Steam.Logging
{
    public class LogManager
    {
        public LogSeverity Level { get; }
        private Logger ClientLogger { get; }

        public event Func<LogMessage, Task> Message { add { _messageEvent.Add(value); } remove { _messageEvent.Remove(value); } }
        private readonly AsyncEvent<Func<LogMessage, Task>> _messageEvent = new AsyncEvent<Func<LogMessage, Task>>();

        public LogManager(LogSeverity minSeverity)
        {
            Level = minSeverity;
            ClientLogger = new Logger(this, "Steam");
        }

        public async Task LogAsync(LogSeverity severity, string source, Exception ex)
        {
            try
            {
                var message = new LogMessage(severity, source, null, ex);
                if (severity <= Level)
                    await _messageEvent.InvokeAsync(message).ConfigureAwait(false);

                Debug.WriteLine(message);
            }
            catch { }
        }
        public async Task LogAsync(LogSeverity severity, string source, string message, Exception ex = null)
        {
            try
            {
                var logMessage = new LogMessage(severity, source, message, ex);
                if (severity <= Level)
                    await _messageEvent.InvokeAsync(logMessage).ConfigureAwait(false);

                Debug.WriteLine(message);
            }
            catch { }
        }

        public Task ErrorAsync(string source, Exception ex)
            => LogAsync(LogSeverity.Error, source, ex);
        public Task ErrorAsync(string source, string message, Exception ex = null)
            => LogAsync(LogSeverity.Error, source, message, ex);

        public Task WarningAsync(string source, Exception ex)
            => LogAsync(LogSeverity.Warning, source, ex);
        public Task WarningAsync(string source, string message, Exception ex = null)
            => LogAsync(LogSeverity.Warning, source, message, ex);

        public Task InfoAsync(string source, Exception ex)
            => LogAsync(LogSeverity.Info, source, ex);
        public Task InfoAsync(string source, string message, Exception ex = null)
            => LogAsync(LogSeverity.Info, source, message, ex);

        public Task VerboseAsync(string source, Exception ex)
            => LogAsync(LogSeverity.Verbose, source, ex);
        public Task VerboseAsync(string source, string message, Exception ex = null)
            => LogAsync(LogSeverity.Verbose, source, message, ex);

        public Task DebugAsync(string source, Exception ex)
            => LogAsync(LogSeverity.Debug, source, ex);
        public Task DebugAsync(string source, string message, Exception ex = null)
            => LogAsync(LogSeverity.Debug, source, message, ex);

        public Logger CreateLogger(string name) => new Logger(this, name);

        public async Task WriteInitialLog()
        {
            await ClientLogger.InfoAsync($"The Steam Standard Project v0.9");
        }
    }
}
