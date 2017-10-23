namespace Steam.Net.Messages
{
    /// <summary>
    /// Represents a basic Steam header with a target or source job ID
    /// </summary>
    public class Header
    {
        private SteamGid _jobId;

        /// <summary>
        /// Gets the job ID of this message
        /// </summary>
        /// <remarks>
        /// When this message is serialized this becomes the target job ID. If it's deserialize it's the source job ID
        /// </remarks>
        public SteamGid JobId => _jobId;

        internal Header(SteamGid gid)
        {
            _jobId = gid;
        }

        /// <summary>
        /// Clones this header and adds the specified job ID
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public Header WithJobId(SteamGid jobId)
        {
            Header header = Clone();
            header._jobId = jobId;
            return header;
        }

        /// <summary>
        /// Clones the current <see cref="Header"/>
        /// </summary>
        /// <returns></returns>
        protected virtual Header Clone()
        {
            return new Header(_jobId);
        }
    }
}
