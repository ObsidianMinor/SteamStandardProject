using Steam.Net.Messages;
using Steam.Net.Messages.Protobufs;
using System.Threading.Tasks;

namespace Steam.Net
{
    public class OfflineSelfUser : NetEntity<SteamNetworkClientBase>, ISelfUser
    {
        public SteamId Id { get; }

        public string PersonaName { get; }

        public PersonaState PersonaState => PersonaState.Offline;

        internal OfflineSelfUser(SteamNetworkClient client, SteamId id, string personaName) : base(client)
        {
            Id = id;
            PersonaName = personaName;
        }

        public async Task<Result> SetPersonaNameAsync(string personaName)
        {
            var response = await SendJobAsync<CMsgPersonaChangeResponse>(
                NetworkMessage.CreateProtobufMessage(MessageType.ClientChangeStatus, new CMsgClientChangeStatus { player_name = personaName })).ConfigureAwait(false);

            return (Result)response.result;
        }

        public async Task<Result> SetPersonaStateAsync(PersonaState state)
        {
            var response = await SendJobAsync<CMsgPersonaChangeResponse>(
                NetworkMessage.CreateProtobufMessage(MessageType.ClientChangeStatus, new CMsgClientChangeStatus { persona_state = (uint)state })).ConfigureAwait(false);

            return (Result)response.result;
        }
    }
}