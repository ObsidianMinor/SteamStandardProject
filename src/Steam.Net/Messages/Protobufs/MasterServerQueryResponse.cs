using ProtoBuf;
using System.Collections.Generic;

namespace Steam.Net.Messages.Protobufs
{
    internal class MasterServerQueryResponse
    {
        [ProtoMember(1)]
        internal List<MSQueryServer> Servers { get; set; }
        [ProtoMember(2, IsRequired = false)]
        internal string Error { get; set; }
    }

    internal class MSQueryServer
    {
        [ProtoMember(1, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        internal uint ServerIp { get; set; }
        [ProtoMember(2, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        internal uint ServerPort { get; set; }
        [ProtoMember(3, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        internal uint AuthPlayers { get; set; }
    }
}
