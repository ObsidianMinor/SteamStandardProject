using Steam.Net.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steam.Net
{
    public class FriendChat : NetworkEntity, IChatRoom
    {
        private FriendChat(SteamNetworkClient client) : base(client)
        {
        }

        public User Friend { get; private set; }

        public IReadOnlyCollection<ChatMessage> Messages { get; private set; }

        public Task EnterTypingStateAsync()
        {
            throw new NotImplementedException();
        }

        public Task SendMessageAsync(string message)
        {
            throw new NotImplementedException();
        }

        internal static FriendChat Create(SteamNetworkClient client, User friend, IEnumerable<ChatMessage> history)
        {
            return new FriendChat(client)
            {
                Friend = friend,
                Messages = history.ToReadOnlyCollection(),
            };
        }
    }
}
