using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract]
    internal class LoginKeyAccepted
    {
        [ProtoMember(1, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        internal uint UniqueId { get; set; }
    }
}
