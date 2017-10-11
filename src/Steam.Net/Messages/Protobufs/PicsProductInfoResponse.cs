using ProtoBuf;
using System.Collections.Generic;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract]
    internal class PicsProductInfoResponse : IExtensible
    {
        [ProtoMember(1)]
        public List<AppInfo> Apps { get; set; }
        [ProtoMember(2, DataFormat = DataFormat.TwosComplement)]
        public List<uint> UnknownAppIds { get; set; }
        [ProtoMember(3)]
        public List<PackageInfo> Packages { get; set; }
        [ProtoMember(4, DataFormat = DataFormat.TwosComplement)]
        public List<uint> UnknownPackageIds { get; set; }
        [ProtoMember(5)]
        public bool MetadataOnly { get; set; }
        [ProtoMember(6)]
        public bool ResponsePending { get; set; }
        [ProtoMember(7, DataFormat = DataFormat.TwosComplement)]
        public uint HttpMinSize { get; set; }
        [ProtoMember(8)]
        public string HttpHost { get; set; } = "";

        IExtension extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref extensionObject, createIfMissing);
    }
}
