namespace Steam.Net
{
    public partial class SteamNetworkClient
    {
        /// <summary>
        /// The connection is ready for a login request
        /// </summary>
        public event AsyncEventHandler Ready;

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
        
        public event AsyncEventHandler<WalletUpdatedEventArgs> WalletUpdated;
    }
}
