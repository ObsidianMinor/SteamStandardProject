using Steam.Net.Messages.Protobufs;

namespace Steam.Net.Messages
{
    public class GameCoordinatorProtobufHeader : Header
    {
        public SteamId ClientSteamId { get; internal set; }
        public int ClientSessionId { get; internal set; }
        public long SourceAppId { get; internal set; }
        public string TargetJobName { get; internal set; }

        internal GameCoordinatorProtobufHeader(GameCoordinatorHeader header) : base(header.JobIdTarget ?? ulong.MaxValue)
        {
            ClientSteamId = header.ClientSteamId ?? SteamId.Zero;
            ClientSessionId = header.ClientSessionId ?? 0;
            SourceAppId = header.SourceAppId ?? 0;
            TargetJobName = header.TargetJobName;
        }
    }

    public enum MessageSource
    {
        Unspecified,
        System,
        SteamId,
        GameCoordinator,
        ReplySystem
    }
}
