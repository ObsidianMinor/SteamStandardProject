using System.Collections.Generic;
using System.Collections.Immutable;
using Steam.Net.Messages.Protobufs;

namespace Steam.Net
{
    public sealed class Clan : NetEntity<SteamNetworkClient>, IClan
    {
        private SteamId _id;
        private AccountFlags _flags;
        private string _name;
        private string _tag;
        private ImmutableArray<byte> _avatar;
        private ClanRelationship _relationship;
        private uint _online;
        private uint _members;
        private uint _chatting;
        private uint _ingame;

        protected Clan(SteamNetworkClient client, SteamId id) : base(client)
        {
            _id = id;
        }

        public SteamId Id => _id;

        public AccountFlags Flags => _flags;

        public string Name => _name;

        public string Tag => _tag;

        public ImmutableArray<byte> AvatarHash => _avatar;

        public ClanRelationship Relationship => _relationship;

        public long Members => _members;

        public long Online => _online;

        public long Chatting => _chatting;

        public long InGame => _ingame;
        
        internal Clan WithState(CMsgClientClanState state)
        {
            var before = (Clan)MemberwiseClone();

            before._flags = (AccountFlags)state.clan_account_flags;
            if (state.name_info != null)
            {
                before._name = state.name_info.clan_name;
                before._avatar = ImmutableArray.Create(state.name_info.sha_avatar);
            }
            if (state.user_counts != null)
            {
                before._members = state.user_counts.members;
                before._online = state.user_counts.online;
                before._chatting = state.user_counts.chatting;
                before._ingame = state.user_counts.in_game;
            }

            return before;
        }

        internal Clan WithPersonaState(CMsgClientPersonaState.Friend state, ClientPersonaStateFlag flag)
        {
            var before = (Clan)MemberwiseClone();

            if (flag.HasFlag(ClientPersonaStateFlag.PlayerName))
            {
                before._name = state.player_name;
            }

            if (flag.HasFlag(ClientPersonaStateFlag.Presence))
            {
                before._avatar = ImmutableArray.Create(state.avatar_hash);
            }

            if (flag.HasFlag(ClientPersonaStateFlag.ClanTag))
            {
                before._tag = state.clan_tag;
            }

            return before;
        }

        internal Clan WithRelationship(ClanRelationship relationship)
        {
            Clan newClan = (Clan)MemberwiseClone();
            newClan._relationship = relationship;
            return newClan;
        }

        internal static Clan Create(SteamNetworkClient client, ClanRelationship relationship, CMsgClientClanState state)
        {
            return new Clan(client, state.steamid_clan).WithRelationship(relationship).WithState(state);
        }

        internal static Clan Create(SteamNetworkClient client, ClanRelationship relationship, CMsgClientPersonaState.Friend state, ClientPersonaStateFlag flag)
        {
            return new Clan(client, state.friendid).WithRelationship(relationship).WithPersonaState(state, flag);
        }
    }
}
