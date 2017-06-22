using Steam.Net.Messages.Serialization;

namespace Steam.Net.Messages
{
    internal class ExtendedClientHeader
    {
        [PacketFieldOrder(0)]
        internal MessageType MessageType { get; set; } = MessageType.Invalid;
        [PacketFieldOrder(1)]
        internal byte HeaderSize { get; set; } = 36;
        [PacketFieldOrder(2)]
        internal ushort HeaderVersion { get; set; } = 2;
        [PacketFieldOrder(3)]
        internal ulong TargetJobId { get; set; } = ulong.MaxValue;
        [PacketFieldOrder(4)]
        internal ulong SourceJobId { get; set; } = ulong.MaxValue;
        [PacketFieldOrder(5)]
        internal byte HeaderCanary { get; set; } = 239;
        [PacketFieldOrder(6)]
        internal ulong SteamId { get; set; } = 0;
        [PacketFieldOrder(7)]
        internal int SessionId { get; set; } = 0;
    }
}
