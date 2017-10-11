using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract(Name = "AppInfo")]
    internal class AppInfo : IExtensible
    {
        [ProtoMember(1, DataFormat = DataFormat.TwosComplement)]
        internal uint AppId { get; set; }
        [ProtoMember(2, DataFormat = DataFormat.TwosComplement)]
        internal ulong AccessToken { get; set; }
        [ProtoMember(3)]
        internal bool OnlyPublic { get; set; }
        private IExtension extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing) => Extensible.GetExtensionObject(ref extensionObject, createIfMissing);
    }
}
