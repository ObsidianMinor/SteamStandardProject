using Steam.Net.Messages.Serialization;

namespace Steam.Net.Messages
{
    internal class MessageHeader
    {
        [PacketFieldOrder(0)]
        internal MessageType MessageType { get; set; }
        [PacketFieldOrder(1)]
        internal ulong TargetJobId { get; set; } = ulong.MaxValue;
        [PacketFieldOrder(2)]
        internal ulong SourceJobId { get; set; } = ulong.MaxValue;
    }
}
