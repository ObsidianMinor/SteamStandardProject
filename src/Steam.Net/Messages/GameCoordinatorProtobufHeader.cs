using Steam.Net.GameCoordinators.Messages.Protobufs;

namespace Steam.Net.Messages
{
    public class GameCoordinatorProtobufHeader : Header // nothing in the protobuf header is actually used
    {
        internal GameCoordinatorProtobufHeader(SteamGid gid) : base(gid) { }

        internal GameCoordinatorProtobufHeader(CMsgProtoBufHeader header) : this(header.job_id_source) { }

        protected override Header Clone()
        {
            return new GameCoordinatorProtobufHeader(JobId);
        }
    }
}
