using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steam.Net
{
    /// <summary>
    /// Represents a basic chat room on Steam
    /// </summary>
    public interface IChatRoom
    {
        /// <summary>
        /// Gets the chat history of this room
        /// </summary>
        IReadOnlyCollection<ChatMessage> Messages { get; }
        /// <summary>
        /// Sends a message to this room
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SendMessageAsync(string message);
        /// <summary>
        /// Enters a typing state in this room
        /// </summary>
        /// <returns></returns>
        Task EnterTypingStateAsync();
    }
}
