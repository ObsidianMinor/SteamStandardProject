namespace Steam.Net.Messages
{
    /// <summary>
    /// Represents a basic Steam header with a target or source job ID
    /// </summary>
    public class Header
    {
        /// <summary>
        /// Gets the job ID of this message
        /// </summary>
        /// <remarks>
        /// When this message is serialized this becomes the target job ID. If it's deserialize it's the source job ID
        /// </remarks>
        public SteamGid JobId { get; internal set; }

        internal Header(SteamGid job)
        {
            JobId = job;
        }
    }
}
