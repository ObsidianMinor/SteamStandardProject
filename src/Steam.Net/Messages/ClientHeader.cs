namespace Steam.Net.Messages
{
    /// <summary>
    /// Represents a client header with a Steam ID, session ID, and job IDs
    /// </summary>
    public class ClientHeader : Header
    {
        /// <summary>
        /// The Steam ID of the client this message was sent from or to
        /// </summary>
        public SteamId SteamId { get; internal set; }

        /// <summary>
        /// The session ID of the client this message was sent from or to
        /// </summary>
        public int SessionId { get; internal set; }

        internal ClientHeader(SteamGid job, SteamId id, int sessionId) : base(job)
        {
            SteamId = id;
            SessionId = sessionId;
        }
    }
}
