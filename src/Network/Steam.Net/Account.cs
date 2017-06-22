using Steam.Common;
using Steam.Web;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Steam.Net
{
    public abstract class Account : NetworkEntity
    {
        internal Account(SteamNetworkClient client) : base(client)
        {
        }

        public SteamId Id { get; internal set; }

        public string Name { get; internal set; }

        public ImmutableArray<byte> AvatarHash { get; internal set; }

        public Uri GetAvatarUri(ImageSize size)
        {
            return SteamWebClient.GetAvatarImageUrl(string.Join("", AvatarHash.Select(b => b.ToString("X2"))), size);
        }
    }
}
