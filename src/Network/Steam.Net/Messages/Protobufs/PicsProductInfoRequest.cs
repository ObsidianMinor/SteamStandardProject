using ProtoBuf;
using System.Collections.Generic;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract(Name = "CMsgClientPICSProductInfoRequest")]
    internal class PicsProductInfoRequest : IExtensible
    {
        [ProtoMember(1)]
        internal List<PackageInfo> Packages { get; set; }
        [ProtoMember(2)]
        internal List<AppInfo> Apps { get; set; }
        [ProtoMember(3)]
        internal bool MetadataOnly { get; set; }
        [ProtoMember(4, DataFormat = DataFormat.TwosComplement)]
        internal uint NumberPreviouslyFailed { get; set; }

        private IExtension extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing) => Extensible.GetExtensionObject(ref extensionObject, createIfMissing);
    }
}
