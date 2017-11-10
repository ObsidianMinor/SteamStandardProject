namespace Steam.Net
{
    /// <summary>
    /// Represents a user with no information except it's ID and relationship status
    /// </summary>
    public class UnknownUser : IUser
    {
        public SteamId Id { get; }

        public FriendRelationship Relationship { get; }

        internal UnknownUser(SteamId id, FriendRelationship relationship)
        {
            Id = id;
            Relationship = relationship;
        }
    }
}
