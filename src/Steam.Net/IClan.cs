namespace Steam.Net
{
    public interface IClan
    {
        SteamId Id { get; }

        ClanRelationship Relationship { get; }
    }
}