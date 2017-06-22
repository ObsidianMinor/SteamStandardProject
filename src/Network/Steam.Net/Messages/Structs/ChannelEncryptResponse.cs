using Steam.Net.Messages.Serialization;

namespace Steam.Net.Messages.Structs
{
    internal class ChannelEncryptResponse
    {
        [PacketFieldOrder(0)]
        public uint ProtocolVersion { get; set; } = 1;
        [PacketFieldOrder(1)]
        public uint KeySize { get; set; } = 128;
    }
}
