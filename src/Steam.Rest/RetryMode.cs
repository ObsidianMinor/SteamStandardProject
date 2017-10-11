using System;

namespace Steam.Rest
{
    /// <summary>
    /// Specifies retry modes to use for the <see cref="SteamRestClient"/>
    /// </summary>
    [Flags]
    public enum RetryMode
    {
        /// <summary>
        /// Uses the default value defined by a <see cref="SteamRestConfig"/>
        /// </summary>
        UseDefault = -1,
        /// <summary>
        /// Always fail for timeouts and bad gateways
        /// </summary>
        AlwaysFail,
        /// <summary>
        /// Retry timeouts
        /// </summary>
        RetryTimeouts,
        /// <summary>
        /// Retry error 502 responses
        /// </summary>
        RetryBadGateway = 4,
        /// <summary>
        /// Retry timeouts and bad gateway results
        /// </summary>
        AlwaysRetry = RetryTimeouts | RetryBadGateway
    }
}
