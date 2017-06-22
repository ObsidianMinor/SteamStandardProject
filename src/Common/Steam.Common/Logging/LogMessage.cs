using System;

namespace Steam.Common.Logging
{
    /// <summary>
    /// A simple log message
    /// </summary>
    public struct LogMessage
    {
        /// <summary>
        /// The source of this log message
        /// </summary>
        public string Source { get; }
        /// <summary>
        /// The time this message was created
        /// </summary>
        public DateTime Time { get; }
        /// <summary>
        /// The message
        /// </summary>
        public string Message { get; }
        /// <summary>
        /// The log severity
        /// </summary>
        public LogSeverity Level { get; }

        /// <summary>
        /// Creates a new log message
        /// </summary>
        /// <param name="source"></param>
        /// <param name="level"></param>
        /// <param name="message"></param>
        public LogMessage(string source, LogSeverity level, string message)
        {
            Source = source;
            Time = DateTime.Now;
            Level = level;
            Message = message;
        }

        public override string ToString()
        {
            return $"{Time.ToString("HH:mm:ss:fffffff")} - [{Source}] - {Level} - {Message}";
        }
    }
}
