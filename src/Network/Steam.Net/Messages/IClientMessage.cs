using Steam.Common;

namespace Steam.Net.Messages
{
    internal interface IClientMessage : IMessage
    {
        SteamId SteamId { get; set; }

        int SessionId { get; set; }
    }
}
