using Steam.Common;
using Steam.Rest;
using Steam.Web;
using System.Linq;
using System.Threading.Tasks;

namespace Steam.Net
{
    /// <summary>
    /// Represents a user on the Steam network
    /// </summary>
    public class User : Account
    {
        /// <summary>
        /// Gets the current persona state of this user
        /// </summary>
        public PersonaState PersonaState { get; protected set; }
        /// <summary>
        /// Gets the state flag of this user
        /// </summary>
        public PersonaStateFlag PersonaStateFlag { get; protected set; }

        /// <summary>
        /// Gets the ID of the app this user is playing
        /// </summary>
        public uint AppId { get; protected set; }
        /// <summary>
        /// Gets the ID of the game this user is playing
        /// </summary>
        public GameId GameId { get; protected set; }
        /// <summary>
        /// Gets the name of the game this user is playing
        /// </summary>
        public string GameName { get; protected set; }

        public User(SteamNetworkClient client) : base(client)
        {
        }

        /// <summary>
        /// Returns this user's Steam community profile
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<UserProfile> GetProfileAsync(RequestOptions options = null)
        {
            var profiles = await Client.GetPlayerSummariesAsync(new[] { Id }, options);
            return profiles.FirstOrDefault();
        }
    }
}
