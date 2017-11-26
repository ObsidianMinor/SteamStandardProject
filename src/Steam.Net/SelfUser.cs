using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Steam.Net.Messages.Protobufs;

namespace Steam.Net
{
    public sealed class SelfUser : NetEntity<SteamNetworkClient>, ISelfUser
    {
        private SteamId _steamId;
        private string _playerName;
        private PersonaState _state;
        private PersonaStateFlag _stateFlags;
        private GameInfo _game;
        private DateTimeOffset _lastLogoff;
        private DateTimeOffset _lastLogon;
        private uint _onlineSessionInstances;
        private uint _publishedInstanceId;
        private ImmutableArray<byte> _avatarHash;

        /// <summary>
        /// Gets this user's Steam ID
        /// </summary>
        public SteamId Id => _steamId;

        /// <summary>
        /// Gets this user's persona name
        /// </summary>
        public string PersonaName => _playerName;

        /// <summary>
        /// Gets the persona state of this user
        /// </summary>
        public PersonaState Status => _state;

        /// <summary>
        /// Get the persona state flags of this user
        /// </summary>
        public PersonaStateFlag Flags => _stateFlags;

        /// <summary>
        /// Gets the relationship this user has with the current user
        /// </summary>
        public FriendRelationship Relationship => FriendRelationship.None;

        /// <summary>
        /// Gets information about the current game this user is in
        /// </summary>
        public GameInfo GameInfo => _game;

        /// <summary>
        /// Gets the last time this user logged off
        /// </summary>
        public DateTimeOffset LastLogoff => _lastLogoff;

        /// <summary>
        /// Gets the last time this user logged on
        /// </summary>
        public DateTimeOffset LastLogon => _lastLogon;

        public long OnlineSessionInstances => _onlineSessionInstances;

        public long PublishedInstanceId => _publishedInstanceId;
        
        private SelfUser(SteamNetworkClient client, SteamId id) : base(client)
        {
            _steamId = id;
        }
        
        internal SelfUser WithState(CMsgClientPersonaState.Friend state, ClientPersonaStateFlag flag)
        {
            var before = (SelfUser)MemberwiseClone();

            if (flag.HasFlag(ClientPersonaStateFlag.Presence))
            {
                before._avatarHash = ImmutableArray.Create(state.avatar_hash);
                before._state = (PersonaState)state.persona_state;
                before._stateFlags = (PersonaStateFlag)state.persona_state_flags;
            }

            if (flag.HasFlag(ClientPersonaStateFlag.PlayerName))
            {
                before._playerName = state.player_name;
            }

            if (flag.HasFlag(ClientPersonaStateFlag.LastSeen))
            {
                before._lastLogoff = DateTimeOffset.FromUnixTimeSeconds(state.last_logoff);
                before._lastLogon = DateTimeOffset.FromUnixTimeSeconds(state.last_logon);
            }

            if (flag.HasFlag(ClientPersonaStateFlag.Status))
            {
                before._onlineSessionInstances = state.online_session_instances;
                before._publishedInstanceId = state.published_instance_id;
            }

            if (flag.HasFlag(ClientPersonaStateFlag.QueryPort | ClientPersonaStateFlag.SourceID | ClientPersonaStateFlag.GameDataBlob | ClientPersonaStateFlag.GameExtraInfo))
            {
                before._game = GameInfo.Create(state);
            }
            
            return before;
        }

        public Task SetPersonaNameAsync(string personaName) => Client.SetPersonaNameAsync(personaName);

        public Task SetPersonaStateAsync(PersonaState state) => Client.SetPersonaStateAsync(state);

        internal static SelfUser Create(SteamNetworkClient client, CMsgClientPersonaState.Friend state, ClientPersonaStateFlag flag)
        {
            return new SelfUser(client, state.friendid).WithState(state, flag);
        }
    }
}
