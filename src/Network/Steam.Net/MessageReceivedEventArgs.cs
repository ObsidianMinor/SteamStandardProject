using System;

namespace Steam.Net
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public ChatMessage Message { get; }
        public IChatRoom Room { get; }
    }
}
