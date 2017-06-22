using Steam.KeyValues;

namespace Steam.Web.API.Responses
{
    internal class ServerTimeResponse : Response
    {
        [KeyValueProperty("server_time")]
        internal long ServerTime { get; set; }
        [KeyValueProperty("skew_tolerance_seconds")]
        internal int SkewTolerance { get; set; }
        [KeyValueProperty("large_time_jink")]
        internal int LargeTimeJink { get; set; }
        [KeyValueProperty("probe_frequency_sounds")]
        internal int ProbeFrequencySeconds { get; set; }
        [KeyValueProperty("adjusted_time_probe_frequency_seconds")]
        internal int AdjustedTimeProbeFrequencySeconds { get; set; }
        [KeyValueProperty("hint_probe_frequency_seconds")]
        internal int HintProbeFrequencySeconds { get; set; }
        [KeyValueProperty("sync_timeout")]
        internal int SyncTimeout { get; set; }
        [KeyValueProperty("try_again_seconds")]
        internal int TryAgainSeconds { get; set; }
        [KeyValueProperty("max_attempts")]
        internal int MaxAttempts { get; set; }
    }
}
