using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract]
    internal class LoginKey
    {
        [ProtoMember(1, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        public uint UniqueId { get; set; }
        [ProtoMember(2, IsRequired = false)]
        public string Key { get; set; }
    }
}
