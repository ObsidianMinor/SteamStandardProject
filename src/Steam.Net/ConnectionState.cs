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
        /// The client is connected to a connection manager and encryption should be set up soon
        /// </summary>
        Connected,
        /// <summary>
        /// The connection is encrypted and messages can now be sent and received
        /// </summary>
        Encrypted,
        /// <summary>
        /// The client is disconnecting from the network
        /// </summary>
        Disconnecting,
    }
}