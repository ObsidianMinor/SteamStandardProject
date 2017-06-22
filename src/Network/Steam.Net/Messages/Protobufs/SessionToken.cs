using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract]
    internal class SessionToken : IExtensible
    {
        [ProtoMember(1, DataFormat = DataFormat.TwosComplement)]
        internal ulong Token { get; set; }

        IExtension extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref extensionObject, createIfMissing);
    }
}
