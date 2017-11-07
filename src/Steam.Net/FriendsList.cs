using Steam.Net.Messages.Protobufs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steam.Net
{
    /// <summary>
    /// Represents a Steam friends list
    /// </summary>
    public class FriendsList : NetEntity<SteamNetworkClient>, IReadOnlyCollection<IUser>, IReadOnlyCollection<IClan>,
    {
        private List<IUser> _users;
        private List<IClan> _clans;

        public event AsyncEventHandler<UserUpdatedEventArgs> UserUpdated;

        public event AsyncEventHandler<ClanUpdatedEventArgs> ClanUpdated;

        public int ClanCount => _clans.Count;

        public int FriendCount => _users.Count;

        internal FriendsList(SteamNetworkClient client) : base(client)
        {
            _users = new List<IUser>();
            _clans = new List<IClan>();
        }

        public async Task AddFriend(IUser user)
        {
            
        }

        public async Task RemoveFriend(IUser user)
        {

        }
    }
}
