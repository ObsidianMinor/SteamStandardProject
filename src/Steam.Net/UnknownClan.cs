namespace Steam.Net
{
    public class UnknownClan : IClan
    {
        public SteamId Id { get; }

        public ClanRelationship Relationship { get; }

        internal UnknownClan(SteamId id, ClanRelationship relationship)
        {
            Id = id;
            Relationship = relationship;
        }
    }
}