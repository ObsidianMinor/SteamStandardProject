using System.Diagnostics;

namespace Steam.Net
{
    /// <summary>
    /// Represents a user on Steam
    /// </summary>
    [DebuggerDisplay("{PlayerName} : {SteamId}")]
    public class User : Account, IUser
    {
        private string _playerName;
        private PersonaState _state;
        private PersonaStateFlag _stateFlags;
        private FriendRelationship _relationship;

        public string PlayerName => _playerName;
        public PersonaState Status => _state;
        public PersonaStateFlag Flags => _stateFlags;

        public FriendRelationship Relationship => _relationship;
        
        internal User(SteamId id, FriendRelationship relationship) : base(id) 
        { 
            _relationship = relationship;
        }

        internal User WithName(string name)
        {
            var before = (User)Clone();
            before._playerName = name;
            return before;
        }

        internal User WithStatus(PersonaState state)
        {
            var before = (User)Clone();
            before._state = state;
            return before;
        }

        internal User WithFlags(PersonaStateFlag flag)
        {
            var before = (User)Clone();
            before._stateFlags = flag;
            return before;
        }

        internal User WithRelationship(FriendRelationship relationship)
        {
            var before = (User)Clone();
            before._relationship = relationship;
            return before;
        }
    }
}