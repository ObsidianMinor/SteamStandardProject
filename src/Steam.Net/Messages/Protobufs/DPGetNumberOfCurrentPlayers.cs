using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract]
    internal class DPGetNumberOfCurrentPlayers
    {
        [ProtoMember(1, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        internal uint AppId { get; set; }
    }
}
