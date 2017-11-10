using System.Threading.Tasks;

namespace Steam.Net
{
    public interface ISelfUser
    {
        SteamId Id { get; }
        string PersonaName { get; }

        Task<Result> SetPersonaName(string name);
        Task<Result> SetPersonaState(PersonaState state);
    }
}
