namespace Steam.Net
{
    public enum ConnectionState
    {
        /// <summary>
        /// The client is disconnected from a connection manager
        /// </summary>
        Disconnected,
        /// <summary>
        /// The client is connecting to a connection manager
        /// </summary>
        Connecting,
        /// <summary>
        /// The client is connected to a connection manager and encryption is set up
        /// </summary>
        Connected,
        /// <summary>
        /// The client is disconnecting from the network
        /// </summary>
        Disconnecting,
    }
}