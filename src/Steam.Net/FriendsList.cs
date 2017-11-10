using Steam.Net.Messages.Protobufs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            
            var users = new List<(IUser Before, IUser After)>();
            var clans = new List<(IClan Before, IClan After)>();

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
                                users.Add((user, null));
                            }
                            else // otherwise...
                            {
                                IUser before = user;
                                IUser after;
                                if (user is UnknownUser) // if they were previously unknown
                                {
                                    after = new UnknownUser(user.Id, relationship); // make the after user just another unknown user
                                }
                                else
                                {
                                    User realUser = (User)user; // otherwise
                                    after = realUser.WithRelationship(relationship); // clone the existing one and add a relationship
                                }

                                users.Add((before, after)); // tell everyone we're bffs or whatever
                            }
                        }
                        else // or unless we don't have that user
                        {
                            _users[friendId] = new UnknownUser(friendId, relationship); // make a new unknown one and tell everyone that person now exists
                            users.Add((null, _users[friendId]));
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
                                clans.Add((clan, null));
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

                                clans.Add((before, after));
                            }
                        }
                        else
                        {
                            _clans[friendId] = new UnknownClan(friendId, relationship);
                            clans.Add((null, _clans[friendId]));
                        }
                    }
                }
            }
            finally
            {
                _slim.Release();
            }

            // send update notifications outside the lock so other threads can update if possible
            await Task.WhenAll(users.Select(t => InvokeUserUpdated(t.Before, t.After)).Concat(clans.Select(t => InvokeClanUpdated(t.Before, t.After)))).ConfigureAwait(false);
        }

        internal async Task UpdateClan(CMsgClientClanState clan)
        {
            IClan before;
            IClan after;

            await _slim.WaitAsync();
            try
            {
                before = _clans[clan.steamid_clan];

                if (before is UnknownClan)
                {
                    after = Clan.Create(Client, before as UnknownClan, clan);
                }
                else
                {
                    after = (before as Clan).WithState(clan);
                }

                _clans[after.Id] = after;
            }
            finally
            {
                _slim.Release();
            }

            await InvokeClanUpdated(before, after).ConfigureAwait(false);
        }

        internal async Task UpdateFriend(CMsgClientPersonaState persona)
        {
            var users = new List<(IUser Before, IUser After)>();

            ISelfUser currentBefore = null;
            ISelfUser currentAfter = null;

            await _slim.WaitAsync();
            try
            {
                var flag = (ClientPersonaStateFlag)persona.status_flags;
                
                foreach (var friend in persona.friends)
                {
                    if (friend.friendid == Client.SteamId) // that's us!
                    {
                        // todo: update current user
                    }
                    else
                    {
                        IUser before;
                        IUser after;

                        before = _users[friend.friendid];

                        if (before is UnknownUser)
                        {
                            after = User.Create(Client, before as UnknownUser, friend, flag);
                        }
                        else
                        {
                            after = (before as User).WithState(friend, flag);
                        }

                        _users[after.Id] = after;

                        users.Add((before, after));
                    }
                }
            }
            finally
            {
                _slim.Release();
            }

            if (currentBefore != null || currentAfter != null)
                await InvokeCurrentUserUpdated(currentBefore, currentAfter).ConfigureAwait(false);

            await Task.WhenAll(users.Select(t => InvokeUserUpdated(t.Before, t.After))).ConfigureAwait(false);
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
