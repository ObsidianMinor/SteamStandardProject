namespace Steam.Web
{
    public abstract class Entity
    {
        protected SteamWebClient WebClient { get; }

        protected Entity(SteamWebClient webClient)
        {
            WebClient = webClient;
        }
    }
}
