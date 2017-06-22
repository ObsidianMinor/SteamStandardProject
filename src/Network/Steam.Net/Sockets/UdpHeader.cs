using System.IO;

namespace Steam.Net.Sockets
{
    internal class UdpHeader
    {
        public const int Magic = 0x31305356;
        public ushort PayloadSize { get; set; }
        public UdpPacketType PacketType { get; set; } = UdpPacketType.Invalid;
        public byte Flags { get; set; }
        public uint SourceConnId { get; set; } = 512;
        public uint DestinationConnId { get; set; }
        public uint SequenceThis { get; set; }
        public uint SequenceAck { get; set; }
        public uint PacketsInMessage { get; set; }
        public uint MessageStartSequence { get; set; }
        public uint MessageSize { get; set; }

        public void Deserialize(Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Magic);
                writer.Write(PayloadSize);
                writer.Write((byte)PacketType);
                writer.Write(Flags);
                writer.Write(SourceConnId);
                writer.Write(DestinationConnId);
                writer.Write(SequenceThis);
                writer.Write(SequenceAck);
                writer.Write(PacketsInMessage);
                writer.Write(MessageStartSequence);
                writer.Write(MessageSize);
            }
        }

        public void Serialize(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                reader.ReadUInt32(); // read and discard the magic number
                PayloadSize = reader.ReadUInt16();
                PacketType = (UdpPacketType)reader.ReadByte();
                Flags = reader.ReadByte();
                SourceConnId = reader.ReadUInt32();
                DestinationConnId = reader.ReadUInt32();
                SequenceThis = reader.ReadUInt32();
                SequenceAck = reader.ReadUInt32();
                PacketsInMessage = reader.ReadUInt32();
                MessageStartSequence = reader.ReadUInt32();
                MessageSize = reader.ReadUInt32();
            }
        }
    }
}
