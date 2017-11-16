using System.Threading.Tasks;

namespace Steam.Net
{
    public class OfflineSelfUser : NetEntity<SteamNetworkClient>, ISelfUser
    {
        public SteamId Id { get; }

        public string PersonaName { get; }

        public PersonaState Status => PersonaState.Offline;

        internal OfflineSelfUser(SteamNetworkClient client, SteamId id, string personaName) : base(client)
        {
            Id = id;
            PersonaName = personaName;
        }

        public Task<Result> SetPersonaNameAsync(string personaName) => Client.SetPersonaNameAsync(personaName);

        public Task<Result> SetPersonaStateAsync(PersonaState state) => Client.SetPersonaStateAsync(state);
    }
}