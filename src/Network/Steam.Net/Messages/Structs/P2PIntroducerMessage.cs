using Steam.Net.Messages.Serialization;

namespace Steam.Net.Messages.Structs
{
    internal class P2PIntroducerMessage
    {
        [PacketFieldOrder(0)]
        internal ulong SteamId { get; set; }
        [PacketFieldOrder(1)]
        internal IntroducerRouting RoutingType { get; set; }
        [PacketFieldOrder(2)]
        internal byte[] Data { get; set; } = new byte[1450];
        [PacketFieldOrder(3)]
        internal uint DataLen { get; set; }
    }
}
