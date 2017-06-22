using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract]
    internal class SimpleRequest : IExtensible // represents a reusable protobuf with no information
    {
        IExtension _extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref _extensionObject, createIfMissing);
    }
}
