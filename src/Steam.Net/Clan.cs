using System.Collections.Generic;
using System.Collections.Immutable;
using Steam.Net.Messages.Protobufs;

namespace Steam.Net
{
    public class Clan : NetEntity<SteamNetworkClient>, IClan
    {
        private SteamId _id;
        private AccountFlags _flags;
        private string _name;
        private ImmutableArray<byte> _avatar;
        private ClanRelationship _relationship;
        private uint _online;
        private uint _members;
        private uint _chatting;
        private uint _ingame;
        private IReadOnlyCollection<Event> _events;
        private IReadOnlyCollection<Event> _announcements;

        protected Clan(SteamNetworkClient client) : base(client)
        {
        }

        public SteamId Id => _id;

        public AccountFlags Flags => _flags;

        public string Name => _name;

        public ImmutableArray<byte> AvatarHash => _avatar;

        public ClanRelationship Relationship => _relationship;

        public long Members => _members;

        public long Online => _online;

        public long Chatting => _chatting;

        public long InGame => _ingame;

        public IReadOnlyCollection<Event> Events => _events;

        public IReadOnlyCollection<Event> Announcements => _announcements;

        internal Clan WithState(CMsgClientClanState state)
        {
            
        }

        internal Clan WithRelationship(ClanRelationship relationship)
        {
            Clan newClan = (Clan)MemberwiseClone();
            newClan._relationship = relationship;
            return newClan;
        }

        internal static Clan Create(SteamNetworkClient client, UnknownClan unknown, CMsgClientClanState state)
        {
            return new Clan(client).WithRelationship(unknown.Relationship).WithState(state);
        }
    }
}
