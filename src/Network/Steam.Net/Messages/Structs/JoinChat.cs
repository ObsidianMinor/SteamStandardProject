using Steam.Net.Messages.Serialization;

namespace Steam.Net.Messages.Structs
{
    internal class JoinChat
    {
        [PacketFieldOrder(0)]
        internal ulong ChatId { get; set; }
        [PacketFieldOrder(1)]
        internal byte IsVoiceSpeaker { get; set; }
    }
}
