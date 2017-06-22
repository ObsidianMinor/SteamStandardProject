using System;

namespace Steam.Net
{
    public class ChatMessage
    {
        public User Author { get; private set; }

        public DateTimeOffset Time { get; private set; }

        public string Message { get; private set; }
    }
}
