using Steam.Web;

namespace Steam.Net
{
    public abstract class NetEntity<T> : WebEntity<T> where T : SteamNetworkClient
    {
        protected NetEntity(T client) : base(client)
        {
        }
    }
}
