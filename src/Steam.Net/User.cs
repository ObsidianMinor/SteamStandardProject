using System.Diagnostics;
using Steam.Net.Messages.Protobufs;

namespace Steam.Net
{
    /// <summary>
    /// Represents a user on Steam
    /// </summary>
    [DebuggerDisplay("{PlayerName} : {SteamId}")]
    public class User : NetEntity<SteamNetworkClientBase>, IUser
    {
        private SteamId _steamId;
        private string _playerName;
        private PersonaState _state;
        private PersonaStateFlag _stateFlags;
        private FriendRelationship _relationship;
        
        public SteamId Id => _steamId;
        public string PlayerName => _playerName;
        public PersonaState Status => _state;
        public PersonaStateFlag Flags => _stateFlags;

        public FriendRelationship Relationship => _relationship;
        
        internal User(SteamNetworkClient client, SteamId id, FriendRelationship relationship) : base(client)
        {
            _steamId = id;
            _relationship = relationship;
        }

        internal User WithState(CMsgClientPersonaState.Friend state, ClientPersonaStateFlag flag)
        {
            
        }
        
        internal User WithRelationship(FriendRelationship relationship)
        {
            var before = (User)MemberwiseClone();
            before._relationship = relationship;
            return before;
        }

        internal static User Create(SteamNetworkClient client, UnknownUser before, CMsgClientPersonaState.Friend state, ClientPersonaStateFlag flag)
        {
            return new User(client, before.Id, before.Relationship).WithState(state, flag);
        }
    }
}