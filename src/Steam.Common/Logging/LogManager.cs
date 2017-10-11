using System;
using System.Diagnostics;

namespace Steam.Logging
{
    /// <summary>
    /// A simple log manager
    /// </summary>
    public class LogManager
    {
        private readonly LogSeverity _severity;

        /// <summary>
        /// Occurs when a message of greater severity than the log level is logged
        /// </summary>
        public event EventHandler<LogMessage> Log;

        /// <summary>
        /// Creates a new log manager with the specified source string and severity level
        /// </summary>
        /// <param name="severity"></param>
        public LogManager(LogSeverity severity)
        {
            _severity = severity;
        }
        
        /// <summary>
        /// Logs a critical message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="source"></param>
        public void LogCritical(string source, string message)
        {
            LogMessage(source, message, LogSeverity.Critical);
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="source"></param>
        public void LogError(string source, string message)
        {
            LogMessage(source, message, LogSeverity.Error);
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="source"></param>
        public void LogWarning(string source, string message)
        {
            LogMessage(source, message, LogSeverity.Warning);
        }

        /// <summary>
        /// Logs an info message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="source"></param>
        public void LogInfo(string source, string message)
        {
            LogMessage(source, message, LogSeverity.Info);
        }

        /// <summary>
        /// Logs a verbose message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="source"></param>
        public void LogVerbose(string source, string message)
        {
            LogMessage(source, message, LogSeverity.Verbose);
        }

        /// <summary>
        /// Logs a debug message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="source"></param>
        public void LogDebug(string source, string message)
        {
            LogMessage(source, message, LogSeverity.Debug);
        }
        
        private void LogMessage(string source, string message, LogSeverity severity)
        {
            LogMessage logMessage = new LogMessage(source, severity, message);
            if (severity <= _severity)
                Log?.Invoke(this, logMessage);

            Debug.WriteLine(logMessage);
        }
    }
}
