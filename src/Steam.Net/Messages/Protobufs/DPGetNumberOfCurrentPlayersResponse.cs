using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract]
    internal class DPGetNumberOfCurrentPlayersResponse
    {
        [ProtoMember(1, DataFormat = DataFormat.TwosComplement)]
        internal int Result { get; set; }
        [ProtoMember(2, DataFormat = DataFormat.TwosComplement)]
        internal int PlayerCount { get; set; }
    }
}
