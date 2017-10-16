using System;
using System.Text;

namespace Steam
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
        /// The message
        /// </summary>
        public string Message { get; }
        /// <summary>
        /// The log severity
        /// </summary>
        public LogSeverity Severity { get; }
        /// <summary>
        /// The exception
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Creates a new log message
        /// </summary>
        /// <param name="source"></param>
        /// <param name="level"></param>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public LogMessage(LogSeverity level, string source, string message, Exception ex)
        {
            Source = source;
            Severity = level;
            Message = message;
            Exception = ex;
        }

        public override string ToString() => ToString(null);
        public string ToString(StringBuilder builder = null, bool fullException = true, bool prependTimestamp = true, DateTimeKind timestampKind = DateTimeKind.Local, int? padSource = 11)
        {
            string sourceName = Source;
            string message = Message;
            string exMessage = fullException ? Exception?.ToString() : Exception?.Message;

            int maxLength = 1 +
                (prependTimestamp ? 8 : 0) + 1 +
                (padSource ?? sourceName?.Length ?? 0) + 1 +
                (message?.Length ?? 0) +
                (exMessage?.Length ?? 0) + 3;

            if (builder == null)
                builder = new StringBuilder(maxLength);
            else
            {
                builder.Clear();
                builder.EnsureCapacity(maxLength);
            }

            if (prependTimestamp)
            {
                DateTime now;
                if (timestampKind == DateTimeKind.Utc)
                    now = DateTime.UtcNow;
                else
                    now = DateTime.Now;
                if (now.Hour < 10)
                    builder.Append('0');
                builder.Append(now.Hour);
                builder.Append(':');
                if (now.Minute < 10)
                    builder.Append('0');
                builder.Append(now.Minute);
                builder.Append(':');
                if (now.Second < 10)
                    builder.Append('0');
                builder.Append(now.Second);
                builder.Append(' ');
            }
            if (sourceName != null)
            {
                if (padSource.HasValue)
                {
                    if (sourceName.Length < padSource.Value)
                    {
                        builder.Append(sourceName);
                        builder.Append(' ', padSource.Value - sourceName.Length);
                    }
                    else if (sourceName.Length > padSource.Value)
                        builder.Append(sourceName.Substring(0, padSource.Value));
                    else
                        builder.Append(sourceName);
                }
                builder.Append(' ');
            }
            if (!string.IsNullOrEmpty(Message))
            {
                for (int i = 0; i < message.Length; i++)
                {
                    //Strip control chars
                    char c = message[i];
                    if (!char.IsControl(c))
                        builder.Append(c);
                }
            }
            if (exMessage != null)
            {
                if (!string.IsNullOrEmpty(Message))
                {
                    builder.Append(':');
                    builder.AppendLine();
                }
                builder.Append(exMessage);
            }

            return builder.ToString();
        }
    }
}
