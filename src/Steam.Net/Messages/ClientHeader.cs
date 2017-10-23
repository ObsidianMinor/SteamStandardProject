namespace Steam.Net.Messages
{
    /// <summary>
    /// Represents a client header with a Steam ID, session ID, and job IDs
    /// </summary>
    public class ClientHeader : Header
    {
        private SteamId _steamId;
        private int _sessionId;

        /// <summary>
        /// The Steam ID of the client this message was sent from or to
        /// </summary>
        public SteamId SteamId => _steamId;

        /// <summary>
        /// The session ID of the client this message was sent from or to
        /// </summary>
        public int SessionId => _sessionId;

        internal ClientHeader(SteamGid job, SteamId id, int sessionId) : base(job)
        {
            _steamId = id;
            _sessionId = sessionId;
        }

        /// <summary>
        /// Clones this client header with the specified Steam ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ClientHeader WithSteamId(SteamId id)
        {
            ClientHeader header = (ClientHeader)Clone();
            header._steamId = id;
            return header;
        }

        /// <summary>
        /// Clones this client header with the specified session ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ClientHeader WithSessionId(int id)
        {
            ClientHeader header = (ClientHeader)Clone();
            header._sessionId = id;
            return header;
        }

        /// <summary>
        /// Clones the current <see cref="ClientHeader"/>
        /// </summary>
        /// <returns></returns>
        protected override Header Clone()
        {
            return new ClientHeader(JobId, SteamId, SessionId);
        }
    }
}
