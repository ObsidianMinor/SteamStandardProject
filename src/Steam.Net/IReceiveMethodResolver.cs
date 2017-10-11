using Steam.Net.GameCoordinators.Messages;
using Steam.Net.Messages;
using System.Reflection;
using System.Threading.Tasks;

namespace Steam.Net
{
    public delegate Task MessageReceiver(NetworkMessage message);

    public delegate Task GameCoordinatorReceiver(GameCoordinatorMessage message);

    /// <summary>
    /// Resolves receiver methods on a <see cref="SteamNetworkClient"/> or <see cref="GameCoordinator"/>.
    /// </summary>
    public interface IReceiveMethodResolver
    {
        bool TryResolve(MethodInfo method, object target, out MessageReceiver receiver);
        bool TryResolve(MethodInfo method, object target, out GameCoordinatorReceiver receiver);
    }
}
