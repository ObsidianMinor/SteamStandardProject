using Steam.Common;
using Steam.Net.Messages.Serialization;

namespace Steam.Net.Messages.Structs
{
    internal class ChannelEncryptResult
    {
        [PacketFieldOrder(0)]
        internal Result Result { get; set; }
    }
}
