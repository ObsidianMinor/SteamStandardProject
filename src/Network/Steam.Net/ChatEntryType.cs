namespace Steam.Common
{
    /// <summary>
    /// Chat event types
    /// </summary>
    public enum ChatEntryType
    {
        /// <summary>
        /// Invalid
        /// </summary>
        Invalid = 0,
        /// <summary>
        /// Chat message
        /// </summary>
        ChatMessage = 1,
        /// <summary>
        /// A user is typing
        /// </summary>
        Typing = 2,
        /// <summary>
        /// Game invite
        /// </summary>
        InviteGame = 3,
        /// <summary>
        /// User left conversation
        /// </summary>
        LeftConversation = 6,
        /// <summary>
        /// User entered
        /// </summary>
        Entered = 7,
        /// <summary>
        /// User was kicked
        /// </summary>
        WasKicked = 8,
        /// <summary>
        /// User was banned
        /// </summary>
        WasBanned = 9,
        /// <summary>
        /// User disconnected
        /// </summary>
        Disconnected = 10,
        /// <summary>
        /// Historical chat message
        /// </summary>
        HistoricalChat = 11,
        /// <summary>
        /// Reserved value
        /// </summary>
        Reserved1 = 12,
        /// <summary>
        /// Reserved value
        /// </summary>
        Reserved2 = 13,
        /// <summary>
        /// Hyperlink blocked
        /// </summary>
        LinkBlocked = 14,
    }
}
