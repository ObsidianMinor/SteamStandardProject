using Steam.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steam.Net
{
    /// <summary>
    /// Represents a Steam chat room between a friend or a clan
    /// </summary>
    public class ChatRoom : NetworkEntity
    {
        private ChatRoom(SteamNetworkClient client) : base(client)
        {
        }

        /// <summary>
        /// Gets the Steam ID of this chat room
        /// </summary>
        public SteamId Id { get; private set; }
        /// <summary>
        /// Gets a collection of all users currently in this chat
        /// </summary>
        public IReadOnlyCollection<User> Members { get; private set; }
        /// <summary>
        /// Gets a collection of all messages received so far
        /// </summary>
        public IReadOnlyCollection<ChatMessage> Messages { get; private set; }

        public async Task SendMessageAsync(string message)
        {
            await Client.ApiClient.SendChatMessageAsync(Id, message, ChatEntryType.ChatMessage).ConfigureAwait(false);
        }

        public async Task EnterTypingStateAsync()
        {
            await Client.ApiClient.SendChatMessageAsync(Id, null, ChatEntryType.Typing).ConfigureAwait(false);
        }
    }
}
