using System;

namespace Steam.Net
{
    /// <summary>
    /// Represents a chat message on Steam
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Gets the user that created this message
        /// </summary>
        public User Author { get; private set; }
        /// <summary>
        /// Gets the time this message was created
        /// </summary>
        public DateTimeOffset Time { get; private set; }
        /// <summary>
        /// Gets the content of this message
        /// </summary>
        public string Message { get; private set; }
    }
}
