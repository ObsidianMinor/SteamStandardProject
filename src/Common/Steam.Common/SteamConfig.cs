using Steam.Common.Logging;

namespace Steam.Common
{
    /// <summary>
    /// Provides a base config for Steam types
    /// </summary>
    public abstract class SteamConfig
    {
        /// <summary>
        /// Specifies the log level of this client
        /// </summary>
        public LogSeverity LogLevel { get; set; } = LogSeverity.Info;
    }
}
