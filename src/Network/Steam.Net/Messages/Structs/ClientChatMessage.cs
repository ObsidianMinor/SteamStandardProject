using Steam.Net.Messages.Serialization;

namespace Steam.Net.Messages.Structs
{
    internal class ClientChatMessage
    {
        [PacketFieldOrder(0)]
        internal ulong Author { get; set; }
        [PacketFieldOrder(1)]
        internal ulong Room { get; set; }
        [PacketFieldOrder(2)]
        internal ChatEntryType Type { get; set; }
    }
}
