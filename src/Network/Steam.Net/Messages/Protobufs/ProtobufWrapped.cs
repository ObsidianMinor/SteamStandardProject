using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract(Name = "CMsgProtobufWrapped")]
    internal class ProtobufWrapped : IExtensible
    {
        [ProtoMember(2, DataFormat = DataFormat.TwosComplement)]
        public byte[] MessageBody { get; set; }

        private IExtension _extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return Extensible.GetExtensionObject(ref _extensionObject, createIfMissing);
        }
    }
}
