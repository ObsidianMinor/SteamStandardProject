namespace Steam.Net
{
    public class SelfUser : User
    {
        public AccountFlags Flags { get; protected set; }

        public string AccountName { get; }

        protected SelfUser(SteamId id) : base(id)
        {
            
        }

        internal static SelfUser CreateAnonymousUser(SteamId id)
        {
            return new SelfUser(id);
        }
    }
}