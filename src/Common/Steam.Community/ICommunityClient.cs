using Steam.Common;
using System.Threading.Tasks;

namespace Steam.Community
{
    /// <summary>
    /// Represents a Steam client that can access data from the Steam community
    /// </summary>
    public interface ICommunityClient
    {
        Task<IUser> GetUserAsync(SteamId id);
    }
}
