using Steam.Common;
using Steam.Rest;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Steam.Web
{
    /// <summary>
    /// Represents a user's profile on the Steam community
    /// </summary>
    public class UserProfile : Entity
    {
        private UserProfile(SteamWebClient webClient) : base(webClient)
        {
        }

        /// <summary>
        /// Gets this user's ID
        /// </summary>
        public SteamId Id { get; private set; }
        /// <summary>
        /// Gets whether the current user can view this user's profile
        /// </summary>
        public bool CanViewFullProfile { get; private set; }
        /// <summary>
        /// Gets whether this profile is completed
        /// </summary>
        public bool ProfileCompleted { get; private set; }
        /// <summary>
        /// Gets the persona name of this user
        /// </summary>
        public string PersonaName { get; private set; }
        /// <summary>
        /// Gets the time of the most recent logoff of this user
        /// </summary>
        public DateTimeOffset LastLogoff { get; private set; }
        /// <summary>
        /// Gets whether the current user can comment on this user's profile
        /// </summary>
        public bool CanCommenting { get; private set; }
        public string ProfileUrl { get; private set; }
        public string AvatarUrl { get; private set; }
        public string MediumAvatarUrl { get; private set; }
        public string FullAvatarUrl { get; private set; }
        public bool UsePersona { get; private set; }
        public PersonaState PersonaState { get; private set; }
        public string RealName { get; private set; }
        public SteamId PrimaryGroupId { get; private set; }
        public DateTimeOffset TimeCreated { get; private set; }
        public PersonaStateFlag PersonaStateFlags { get; private set; }
        public string CountryCode { get; private set; }
        public string StateCode { get; private set; }
        public int CityId { get; private set; }
        public IPEndPoint GameServerIp { get; private set; }
        public SteamId GameServerSteamId { get; private set; }
        public string ExtraGameServerInfo { get; private set; }
        public uint GameAppId { get; private set; }

        /// <summary>
        /// Gets the game server this user is on if they're connected to one
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<GameServer> GetGameServerAsync(RequestOptions options = null)
        {
            if (GameServerIp is null)
                return null;

            return await WebClient.GetServerAtAddressAsync(GameServerIp).ConfigureAwait(false);
        }
    }
}