namespace Steam.Net
{
    public class User : Account
    {
        public string PlayerName { get; }
        public PersonaState Status { get; }

        public int GameQueryPort { get; }
        public GameId SourceId { get; }

        protected User(SteamId id) : base(id)
        {
            
        }
    }
}