using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract(Name = "CMsgClientLoggedOff")]
    internal class LoggedOff : IExtensible
    {
        [ProtoMember(1, DataFormat = DataFormat.TwosComplement)]
        public int Result { get; set; } = 2;

        private IExtension extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing) => Extensible.GetExtensionObject(ref extensionObject, createIfMissing);
    }
}
