using Steam.Common;

namespace Steam.Net.Messages
{
    internal class ClientProtobufMessageHeader : IClientMessage, IHeader<ProtobufHeader>
    {
        public SteamId SteamId
        {
            get => Header.Protobuf.steamid;
            set => Header.Protobuf.steamid = value;
        }
        public int SessionId
        {
            get => Header.Protobuf.client_sessionid;
            set => Header.Protobuf.client_sessionid = value;
        }

        public bool IsProtobuf => true;

        public MessageType Type => Header.Type;

        public SteamGuid TargetJobId
        {
            get => Header.Protobuf.jobid_target;
            set => Header.Protobuf.jobid_target = value;
        }

        public SteamGuid SourceJobId
        {
            get => Header.Protobuf.jobid_source;
            set => Header.Protobuf.jobid_source = value;
        }

        public ProtobufHeader Header { get; set; } = new ProtobufHeader();
        public byte[] Payload { get; set; }
    }
}
