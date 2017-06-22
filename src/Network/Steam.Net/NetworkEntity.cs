namespace Steam.Net
{
    /// <summary>
    /// Represents an object that can request and receive data from the Steam client
    /// </summary>
    public abstract class NetworkEntity
    {
        internal SteamNetworkClient Client { get; }

        internal NetworkEntity(SteamNetworkClient client)
        {
            Client = client;
        }
    }
}
