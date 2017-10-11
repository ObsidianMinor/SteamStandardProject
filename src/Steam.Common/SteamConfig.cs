using Steam.Logging;

namespace Steam
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

        /// <summary>
        /// Clones this Steam config using <see cref="object.MemberwiseClone()"/>
        /// </summary>
        /// <returns></returns>
        public virtual SteamConfig Clone()
        {
            return MemberwiseClone() as SteamConfig;
        }
    }
}
