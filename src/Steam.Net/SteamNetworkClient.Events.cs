namespace Steam.Net
{
    public partial class SteamNetworkClient
    {
        /// <summary>
        /// The client is connected to a connection manager
        /// </summary>
        public event AsyncEventHandler Connected;

        /// <summary>
        /// The connection is ready for a login request
        /// </summary>
        public event AsyncEventHandler Ready;

        /// <summary>
        /// The client has been disconnected from the connection manager
        /// </summary>
        public event AsyncEventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        /// Invoked when the client is logged off
        /// </summary>
        public event AsyncEventHandler<LogOffEventArgs> LoggedOff;

        /// <summary>
        /// Invoked when login was denied
        /// </summary>
        public event AsyncEventHandler<LoginRejectedEventArgs> LoginRejected;

        /// <summary>
        /// Invoked when the client has logged on
        /// </summary>
        public event AsyncEventHandler LoggedOn;

        public event AsyncEventHandler<LoginKeyReceivedEventArgs> LoginKeyReceived;

        public event AsyncEventHandler<CurrentUserUpdatedEventArgs> CurrentUserUpdated;

        public event AsyncEventHandler<WalletUpdatedEventArgs> WalletUpdated;
    }
}
