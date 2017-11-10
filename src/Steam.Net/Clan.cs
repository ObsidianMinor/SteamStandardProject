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

        public SteamId Id { get; }

        public AccountFlags Flags { get; }

        public string Name { get; }

        public ImmutableArray<byte> AvatarHash { get; }

        public ClanRelationship Relationship { get; }

        public long Members { get; }

        public long Online { get; }

        public long Chatting { get; }

        public long InGame { get; }

        public IReadOnlyCollection<Event> Events { get; }

        public IReadOnlyCollection<Event> Announcements { get; }

        internal Clan WithState(CMsgClientClanState state)
        {

        }

        internal Clan WithRelationship(ClanRelationship relationship)
        {
            Clan newClan = (Clan)MemberwiseClone();
            newClan._relationship = relationship;
            return newClan;
        }
    }
}
