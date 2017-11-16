using System.Threading.Tasks;

namespace Steam.Net
{
    public interface ISelfUser
    {
        SteamId Id { get; }
        string PersonaName { get; }
        PersonaState Status { get; }

        Task<Result> SetPersonaNameAsync(string name);
        Task<Result> SetPersonaStateAsync(PersonaState state);
    }
}
