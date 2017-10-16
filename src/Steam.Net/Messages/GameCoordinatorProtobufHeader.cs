using Steam.Net.GameCoordinators.Messages.Protobufs;

namespace Steam.Net.Messages
{
    public class GameCoordinatorProtobufHeader : Header
    {
        public SteamId ClientSteamId { get; internal set; }
        public int ClientSessionId { get; internal set; }
        public long SourceAppId { get; internal set; }
        public string TargetJobName { get; internal set; }

        internal GameCoordinatorProtobufHeader(CMsgProtoBufHeader header) : base(header.job_id_target)
        {
            ClientSteamId = header.client_steam_id;
            ClientSessionId = header.client_session_id;
            SourceAppId = header.source_app_id;
            TargetJobName = header.target_job_name;
        }
    }
}
