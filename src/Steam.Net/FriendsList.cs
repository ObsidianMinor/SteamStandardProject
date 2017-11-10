using Steam.Net.Messages.Protobufs;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Net
{
    /// <summary>
    /// Represents a Steam friends list
    /// </summary>
    public class FriendsList : NetEntity<SteamNetworkClient>, IReadOnlyDictionary<SteamId, IUser>, IReadOnlyDictionary<SteamId, IClan>, IReadOnlyCollection<IUser>, IReadOnlyCollection<IClan>
    {
        private SemaphoreSlim _slim = new SemaphoreSlim(1, 1);
        private uint _maxFriendsCount;
        private Dictionary<SteamId, IUser> _users = new Dictionary<SteamId, IUser>();
        private Dictionary<SteamId, IClan> _clans = new Dictionary<SteamId, IClan>();
        private ISelfUser _currentUser;

        /// <summary>
        /// Gets the user info for the current logged in user account
        /// </summary>
        public ISelfUser CurrentUser => _currentUser;

        /// <summary>
        /// Invokes when a user's public persona is updated
        /// </summary>
        public event AsyncEventHandler<UserUpdatedEventArgs> UserUpdated;

        /// <summary>
        /// Invokes when a clan's public persona is updated
        /// </summary>
        public event AsyncEventHandler<ClanUpdatedEventArgs> ClanUpdated;

        /// <summary>
        /// Invokes when the current user's public persona is updated
        /// </summary>
        public event AsyncEventHandler<CurrentUserUpdatedEventArgs> CurrentUserUpdated;

        /// <summary>
        /// Gets the number of clans this user is in
        /// </summary>
        public int ClanCount => _clans.Count;

        /// <summary>
        /// Gets the number of friends this user has
        /// </summary>
        public int FriendCount => _users.Count;

        /// <summary>
        /// Gets the max number of friends this user can have
        /// </summary>
        public long MaxFriendsCount => _maxFriendsCount;
        
        internal FriendsList(SteamNetworkClient client) : base(client)
        {
        }
        
        internal async Task UpdateList(CMsgClientFriendsList list)
        {
            await _slim.WaitAsync();
            try
            {
                _maxFriendsCount = list.max_friend_count;

                if (!list.bincremental)
                {
                    _users.Clear();
                    _clans.Clear();
                }

                foreach(var friend in list.friends)
                {
                    SteamId friendId = friend.ulfriendid;

                    if (friendId.IsIndividualAccount)
                    {
                        var relationship = (FriendRelationship)friend.efriendrelationship;
                        if (_users.TryGetValue(friendId, out var user)) // if we have a user already
                        {
                            if (relationship == FriendRelationship.None) // and the new relationship is "None"
                            {
                                _users.Remove(friendId); // remove them and tell everyone they don't exist anymore
                                await InvokeUserUpdated(user, null).ConfigureAwait(false);
                            }
                            else // otherwise...
                            {
                                IUser before = user;
                                IUser after;
                                if (user is UnknownUser) // if we were previously unknown
                                {
                                    after = new UnknownUser(user.Id, relationship); // make the after user just another unknown user
                                }
                                else
                                {
                                    User realUser = (User)user; // otherwise
                                    after = realUser.WithRelationship(relationship); // clone the existing one and add a relationship
                                }

                                await InvokeUserUpdated(before, after).ConfigureAwait(false); // tell everyone we're bffs or whatever
                            }
                        }
                        else // or unless we don't have that user
                        {
                            _users[friendId] = new UnknownUser(friendId, relationship); // make a new unknown one and tell everyone that person now exists
                            await InvokeUserUpdated(null, _users[friendId]).ConfigureAwait(false);
                        }
                    }
                    else if (friendId.IsClan)
                    {
                        var relationship = (ClanRelationship)friend.efriendrelationship;
                        if (_clans.TryGetValue(friendId, out var clan))
                        {
                            if (relationship == ClanRelationship.None)
                            {
                                _users.Remove(friendId);
                                await InvokeClanUpdated(clan, null).ConfigureAwait(false);
                            }
                            else
                            {
                                IClan before = clan;
                                IClan after;
                                if (clan is UnknownClan)
                                {
                                    after = new UnknownClan(clan.Id, relationship);
                                }
                                else
                                {
                                    Clan realClan = (Clan)clan;
                                    after = realClan.WithRelationship(relationship);
                                }

                                await InvokeClanUpdated(before, after).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            _clans[friendId] = new UnknownClan(friendId, relationship);
                            await InvokeUserUpdated(null, _users[friendId]).ConfigureAwait(false);
                        }
                    }
                }
            }
            finally
            {
                _slim.Release();
            }
        }

        internal async Task UpdateClan(CMsgClientClanState clan)
        {
            await _slim.WaitAsync();
            try
            {
                
            }
            finally
            {
                _slim.Release();
            }
        }

        internal async Task UpdateFriend(CMsgClientPersonaState persona)
        {
            await _slim.WaitAsync();
            try
            {
                
            }
            finally
            {
                _slim.Release();
            }
        }

        private async Task InvokeUserUpdated(IUser before, IUser after)
        {
            await UserUpdated.InvokeAsync(this, new UserUpdatedEventArgs(before, after)).ConfigureAwait(false);
        }

        private async Task InvokeClanUpdated(IClan before, IClan after)
        {
            await ClanUpdated.InvokeAsync(this, new ClanUpdatedEventArgs(before, after)).ConfigureAwait(false);
        }

        private async Task InvokeCurrentUserUpdated(ISelfUser before, ISelfUser after)
        {
            await CurrentUserUpdated.InvokeAsync(this, new CurrentUserUpdatedEventArgs(before, after)).ConfigureAwait(false);
        }

        #region IReadOnlyDictionary<SteamId, IUser>

        IUser IReadOnlyDictionary<SteamId, IUser>.this[SteamId key] => _users[key];

        IEnumerable<SteamId> IReadOnlyDictionary<SteamId, IUser>.Keys => _users.Keys;

        IEnumerable<IUser> IReadOnlyDictionary<SteamId, IUser>.Values => _users.Values;

        bool IReadOnlyDictionary<SteamId, IUser>.ContainsKey(SteamId key) => _users.ContainsKey(key);

        bool IReadOnlyDictionary<SteamId, IUser>.TryGetValue(SteamId key, out IUser value) => _users.TryGetValue(key, out value);

        #endregion

        #region IReadOnlyDictionary<SteamId, IClan>

        IClan IReadOnlyDictionary<SteamId, IClan>.this[SteamId key] => _clans[key];

        IEnumerable<SteamId> IReadOnlyDictionary<SteamId, IClan>.Keys => _clans.Keys;

        IEnumerable<IClan> IReadOnlyDictionary<SteamId, IClan>.Values => _clans.Values;

        bool IReadOnlyDictionary<SteamId, IClan>.ContainsKey(SteamId key) => _clans.ContainsKey(key);

        bool IReadOnlyDictionary<SteamId, IClan>.TryGetValue(SteamId key, out IClan value) => _clans.TryGetValue(key, out value);

        #endregion

        #region IReadOnlyCollection

        int IReadOnlyCollection<IUser>.Count => FriendCount;
        
        int IReadOnlyCollection<KeyValuePair<SteamId, IUser>>.Count => FriendCount;

        int IReadOnlyCollection<IClan>.Count => ClanCount;

        int IReadOnlyCollection<KeyValuePair<SteamId, IClan>>.Count => ClanCount;

        #endregion

        #region IEnumerable

        IEnumerator<IUser> IEnumerable<IUser>.GetEnumerator() => _users.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _users.Values.GetEnumerator();

        IEnumerator<IClan> IEnumerable<IClan>.GetEnumerator() => _clans.Values.GetEnumerator();
        
        IEnumerator<KeyValuePair<SteamId, IUser>> IEnumerable<KeyValuePair<SteamId, IUser>>.GetEnumerator() => _users.GetEnumerator();
        
        IEnumerator<KeyValuePair<SteamId, IClan>> IEnumerable<KeyValuePair<SteamId, IClan>>.GetEnumerator() => _clans.GetEnumerator();

        #endregion
    }
}
