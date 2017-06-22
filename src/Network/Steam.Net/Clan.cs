using System.Threading.Tasks;

namespace Steam.Net
{
    /// <summary>
    /// Represents a Steam group on the Steam community
    /// </summary>
    public class Clan : Account
    {
        public ClanRelationship Relationship { get; private set; }

        public Clan(SteamNetworkClient client) : base(client)
        {
        }
    }
}
