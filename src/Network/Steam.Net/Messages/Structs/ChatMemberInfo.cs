using Steam.Net.Messages.Serialization;

namespace Steam.Net.Messages.Structs
{
    internal class ChatMemberInfo
    {
        [PacketFieldOrder(0)]
        internal ulong ChatId { get; set; }
        [PacketFieldOrder(1)]
        internal ChatInfoType Type { get; set; }
    }
}
