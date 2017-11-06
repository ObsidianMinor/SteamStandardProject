using System.Threading.Tasks;
using Steam.Net.GameCoordinators.Messages;

namespace Steam.Net.GameCoordinators
{
    /// <summary>
    /// Represents an object that can send and receive data from the game coordinator
    /// </summary>
    public abstract class GameCoordinatorEntity
    {
        protected GameCoordinator Client { get; }

        protected GameCoordinatorEntity(GameCoordinator client)
        {
            Client = client;
        }

        /// <summary>
        /// Sends a message using to Steam on the <see cref="SteamNetworkClient"/>
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <returns>An awaitable task</returns>
        protected async Task SendAsync(GameCoordinatorMessage message)
        {
            await Client.SendAsync(message).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a message to Steam on the <see cref="SteamNetworkClient"/> and waits for a response of the specified type
        /// </summary>
        /// <typeparam name="T">The type of the response</typeparam>
        /// <param name="message">The message to send</param>
        /// <returns>An awaitable task</returns>
        protected async Task<T> SendJobAsync<T>(GameCoordinatorMessage message)
        {
            return await Client.SendJobAsync<T>(message).ConfigureAwait(false);
        }

        protected async Task<GameCoordinatorMessage> SendJobAsync(GameCoordinatorMessage message)
        {
            return await Client.SendJobAsync(message).ConfigureAwait(false);
        }
    }
}
