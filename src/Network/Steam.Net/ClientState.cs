using Steam.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace Steam.Net
{
    /// <summary>
    /// Caches data about the current client
    /// </summary>
    [DebuggerDisplay("Session ID = {SessionId}")]
    internal class ClientState
    {
        ConcurrentBag<License> _licenses = new ConcurrentBag<License>();
        ConcurrentDictionary<SteamId, Account> _friends = new ConcurrentDictionary<SteamId, Account>();
        ConcurrentDictionary<SteamId, Clan> _clans = new ConcurrentDictionary<SteamId, Clan>();
        ConcurrentDictionary<SteamId, ChatRoom> _openChatrooms = new ConcurrentDictionary<SteamId, ChatRoom>();
        ConcurrentDictionary<ServerType, HashSet<IPEndPoint>> _availableServers = new ConcurrentDictionary<ServerType, HashSet<IPEndPoint>>();
        ConcurrentBag<Uri> _availableWebSockets = new ConcurrentBag<Uri>();
        internal SelfUser CurrentUser { get; set; }

        internal SteamId SteamId
        {
            get => CurrentUser.Id;
            set => CurrentUser.Id = value;
        }
        internal int SessionId { get; private set; }
        internal ulong SessionToken { get; set; }
        internal uint CellId { get; private set; }

        internal ClientState() { }

        internal void SetSessionInfo(uint cellId, int sessionId, SteamId id, AccountFlags flags)
        {
            CellId = cellId;
            SessionId = sessionId;
            CurrentUser.Id = id;
            CurrentUser.Flags = flags;
        }

        internal void SetServers(ServerType type, IEnumerable<IPEndPoint> newValues)
        {
            HashSet<IPEndPoint> newEndpoints = new HashSet<IPEndPoint>(newValues);
            HashSet<IPEndPoint> existingEndpoints = _availableServers.GetOrAdd(type, newEndpoints);

            if (existingEndpoints != newEndpoints)
                foreach (IPEndPoint endpoint in newEndpoints)
                    existingEndpoints.Add(endpoint);
        }

        internal void SetWebSockets(IEnumerable<Uri> newValues)
        {
            foreach(Uri webSocket in newValues)
            {
                if (!_availableWebSockets.Contains(webSocket))
                    _availableWebSockets.Add(webSocket);
            }
        }

        internal void RemoveSessionInfo()
        {
            SessionId = 0;
            SteamId = default(SteamId);
            CellId = 0;
        }

        internal void SetLoginContinuation()
        {

        }
        
        internal void UpdateFriend(SteamId id, Account friend)
        {
            
        }

        internal void UpdateClan(SteamId id)
        {

        }
    }
}
