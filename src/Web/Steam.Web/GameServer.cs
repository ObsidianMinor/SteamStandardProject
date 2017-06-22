using System.Net;

namespace Steam.Web
{
    /// <summary>
    /// Represents a game server at a specified endpoint
    /// </summary>
    public class GameServer
    {
        /// <summary>
        /// Gets the IP address of this game server
        /// </summary>
        public IPEndPoint Address { get; private set; }
        /// <summary>
        /// Gets this game server's index in the game master server list
        /// </summary>
        public int GameMasterServerIndex { get; private set; }
        /// <summary>
        /// Gets the app Id of the game this server is running
        /// </summary>
        public uint AppId { get; private set; }
        /// <summary>
        /// Gets the region this server says it's from
        /// </summary>
        public GameRegionCode Region { get; private set; }
        /// <summary>
        /// Gets whether this server is VAC secured
        /// </summary>
        public bool Secure { get; private set; }
        /// <summary>
        /// Gets whether this server is a local area network server
        /// </summary>
        public bool Lan { get; private set; }
        /// <summary>
        /// Gets the game port for this server
        /// </summary>
        public uint GamePort { get; private set; }
        /// <summary>
        /// Gets the port for a SourceTV server if available
        /// </summary>
        public uint SpectatorPort { get; private set; }
    }
}
