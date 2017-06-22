using Steam.Common;
using Steam.Net.Messages.Serialization;

namespace Steam.Net.Messages.Structs
{
    internal class ChannelEncryptRequest
    {
        [PacketFieldOrder(0)]
        internal uint ProtocolVersion { get; set; } = 1;
        [PacketFieldOrder(1)]
        internal Universe Universe { get; set; } = 0;
    }
}
