using System.Threading.Tasks;

namespace Steam.Net
{
    public interface ISelfUser
    {
        SteamId Id { get; }
        string PersonaName { get; }
        PersonaState Status { get; }

        Task SetPersonaNameAsync(string name);
        Task SetPersonaStateAsync(PersonaState state);
    }
}
