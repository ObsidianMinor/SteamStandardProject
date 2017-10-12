using Xunit;
using System;

namespace Steam.Tests
{
    [Trait("Category", "SteamID")]
    public class SteamIdTests
    {
        private void AssertProperties(SteamId id, uint accountId, uint instanceValue, Universe universe, AccountType type)
        {
            Assert.Equal(accountId, id.AccountId);
            Assert.Equal(instanceValue, id.AccountInstance);
            Assert.Equal(universe, id.AccountUniverse);
            Assert.Equal(type, id.AccountType);
        }

        [Fact(DisplayName = "Verify default ctor values")]
        public void CtorValues()
        {
            SteamId id = new SteamId(46143802);
            AssertProperties(id, 46143802, 1, Universe.Public, AccountType.Individual);
        }

        [Fact(DisplayName = "Steam2 ID"), Trait("Subcategory", "Resolve")]
        public void FromSteam2Id()
        {
            SteamId id = SteamId.FromSteamId("STEAM_0:0:23071901");
            AssertProperties(id, 46143802, 1, Universe.Public, AccountType.Individual);
        }

        [Fact(DisplayName = "Steam2 Public Universe ID"), Trait("Subcategory", "Resolve")]
        public void FromSteam2IdPublicUniverse()
        {
            SteamId id = SteamId.FromSteamId("STEAM_1:1:23071901");
            AssertProperties(id, 46143803, 1, Universe.Public, AccountType.Individual);
        }

        [Fact(DisplayName = "Steam3 User ID"), Trait("Subcategory", "Resolve")]
        public void FromSteam3Id()
        {
            SteamId id = SteamId.FromSteam3Id("[U:1:46143802]");
            AssertProperties(id, 46143802, 1, Universe.Public, AccountType.Individual);
        }

        [Fact(DisplayName = "Steam3 Game Server ID"), Trait("Subcategory", "Resolve")]
        public void FromSteam3IdGameServer()
        {
            SteamId id = SteamId.FromSteam3Id("[G:1:31]");
            AssertProperties(id, 31, 0, Universe.Public, AccountType.GameServer);
        }

        [Fact(DisplayName = "Steam3 Anonymous Server ID"), Trait("Subcategory", "Resolve")]
        public void FromSteam3IdAnonymousServer()
        {
            SteamId id = SteamId.FromSteam3Id("[A:1:46124:11245]");
            AssertProperties(id, 46124, 11245, Universe.Public, AccountType.AnonGameServer);
        }

        [Fact(DisplayName = "Steam3 Lobby ID"), Trait("Subcategory", "Resolve")]
        public void FromSteam3IdLobby()
        {
            SteamId id = SteamId.FromSteam3Id("[L:1:12345]");
            AssertProperties(id, 12345, 0x40000, Universe.Public, AccountType.Chat);
        }

        [Fact(DisplayName = "Steam3 Lobby ID with Instance"), Trait("Subcategory", "Resolve")]
        public void FromSteam3IdLobbyInstance()
        {
            SteamId id = SteamId.FromSteam3Id("[L:1:12345:55]");
            AssertProperties(id, 12345, 0x40000 | 55, Universe.Public, AccountType.Chat);
        }

        [Fact(DisplayName = "Steam Community ID"), Trait("Subcategory", "Resolve")]
        public void FromCommunityId()
        {
            SteamId id = SteamId.FromCommunityId(76561198006409530);
            AssertProperties(id, 46143802, (int)Instance.Desktop, Universe.Public, AccountType.Individual);
        }

        [Fact(DisplayName = "Steam Community group ID"), Trait("Subcategory", "Resolve")]
        public void FromGroupCommunityId()
        {
            SteamId id = SteamId.FromCommunityId(103582791434202956);
            AssertProperties(id, 4681548, (int)Instance.All, Universe.Public, AccountType.Clan);
        }

        [Fact(DisplayName = "Steam Community ID render"), Trait("Subcategory", "Render")]
        public void ToCommunityId()
        {
            ulong expected = 76561198006409530;
            ulong real = new SteamId(46143802).ToCommunityId();

            Assert.Equal(expected, real);
        }

        [Fact(DisplayName = "Steam3 ID Render"), Trait("Subcategory", "Render")]
        public void ToSteam3Id()
        {
            string expected = "[U:1:46143802]";
            string actual = new SteamId(46143802).ToSteam3Id();
            Assert.Equal(expected, actual);
        }

        [Fact(DisplayName = "Steam2 ID Render"), Trait("Subcategory", "Render")]
        public void ToSteam2Id()
        {
            string expected = "STEAM_1:0:23071901";
            SteamId id = new SteamId(46143802);
            Assert.Equal(expected, id.ToSteam2Id());
        }

        [Fact(DisplayName = "Unknown ID Render"), Trait("Subcategory", "Render")]
        public void RenderUnknowwn()
        {
            string expected = "UNKNOWN";
            SteamId id = SteamId.Unknown;
            Assert.Equal(expected, id.ToSteam2Id());
        }

        [Fact(DisplayName = "Pending ID Render"), Trait("Subcategory", "Render")]
        public void RenderPending()
        {
            string expected = "STEAM_ID_PENDING";
            SteamId id = SteamId.Pending;
            Assert.Equal(expected, id.ToSteam2Id());
        }

        [Fact(DisplayName = "Instance value higher than max throws")]
        public void InstanceMax()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SteamId(1, instance: 0xFFFFF + 1));
        }

        [Fact(DisplayName = "Special ID equality")]
        public void SpecialIDEquality()
        {
            Assert.True(SteamId.Pending.Equals(SteamId.Pending));
        }

        [Fact(DisplayName = "Normal ID equality")]
        public void NormalIDEquality()
        {
            Assert.True(SteamId.FromCommunityId(76561198006409530).Equals(SteamId.FromCommunityId(76561198006409530)));
        }

        [Fact(DisplayName = "Back and forth")]
        public void BackForth()
        {
            ulong original = 76561198006409530;
            ulong newVal = SteamId.FromCommunityId(76561198006409530);

            Assert.Equal(original, newVal);
        }

        [Fact(DisplayName = "Game Server Instance is All")]
        public void InstanceOfGameServerIsAll()
        {
            SteamId id = SteamId.FromSteam3Id("[G:1:31]");
            Assert.Equal(Instance.All, id.Instance);
        }
    }
}
