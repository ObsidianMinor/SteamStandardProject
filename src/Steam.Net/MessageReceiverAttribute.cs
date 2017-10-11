using System;

namespace Steam.Net
{
    /// <summary>
    /// Informs the client that this method listens for the specified message type
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MessageReceiverAttribute : Attribute
    {
        public MessageType Type { get; }

        public MessageReceiverAttribute(MessageType type)
        {
            Type = type;
        }
    }
}
