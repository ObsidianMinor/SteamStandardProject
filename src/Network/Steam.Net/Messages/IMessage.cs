using Steam.Common;

namespace Steam.Net.Messages
{
    internal interface IMessage : IPayload
    {
        bool IsProtobuf { get; }

        MessageType Type { get; }

        SteamGuid TargetJobId { get; set; }

        SteamGuid SourceJobId { get; set; }
    }
}
