using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract(Name = "PackageInfo")]
    internal class PackageInfo : IExtensible
    {
        [ProtoMember(1, DataFormat = DataFormat.TwosComplement)]
        internal uint PackageId { get; set; }
        [ProtoMember(2, DataFormat = DataFormat.TwosComplement)]
        internal ulong AccessToken { get; set; }

        private IExtension extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing) => Extensible.GetExtensionObject(ref extensionObject, createIfMissing);
    }
}