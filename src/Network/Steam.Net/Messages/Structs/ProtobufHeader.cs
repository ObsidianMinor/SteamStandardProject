using Steam.Net.Messages.Serialization;

namespace Steam.Net.Messages
{
    internal class ProtobufHeader
    {
        [PacketFieldOrder(0)]
        public MessageType Type { get; set; }
        [PacketFieldOrder(1)]
        internal int HeaderLength { get; set; }
        [PacketFieldOrder(2)]
        internal Protobufs.ProtobufHeader Protobuf { get; set; } = new Protobufs.ProtobufHeader();
    }
}
