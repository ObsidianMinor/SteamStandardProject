namespace Steam.Net
{
    public interface IUser
    {
        SteamId Id { get; }

        FriendRelationship Relationship { get; }
    }
}
