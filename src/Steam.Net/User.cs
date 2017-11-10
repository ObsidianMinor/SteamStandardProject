using System.Diagnostics;
using Steam.Net.Messages.Protobufs;

namespace Steam.Net
{
    /// <summary>
    /// Represents a user on Steam
    /// </summary>
    [DebuggerDisplay("{PlayerName} : {SteamId}")]
    public class User : IUser
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
        
        internal User(SteamId id, FriendRelationship relationship)
        {
            _steamId = id;
            _relationship = relationship;
        }

        internal User WithState(CMsgClientPersonaState.Friend state)
        {

        }
        
        internal User WithRelationship(FriendRelationship relationship)
        {
            var before = Clone<User>();
            before._relationship = relationship;
            return before;
        }

        private protected T Clone<T>() where T : User
        {
            return (T)MemberwiseClone();
        }
    }
}