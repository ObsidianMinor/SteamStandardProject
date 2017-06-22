using Steam.KeyValues;

namespace Steam.Local.Apps
{
    /// <summary>
    /// Represents a staged depot
    /// </summary>
    public class StagedDepot
    {
        /// <summary>
        /// The manifest ID of this depot
        /// </summary>
        [KeyValueProperty("manifest")]
        public ulong Manifest { get; set; }

        /// <summary>
        /// The size in bytes of this depot
        /// </summary>
        [KeyValueProperty("size")]
        public ulong Size { get; set; }
    }
}
