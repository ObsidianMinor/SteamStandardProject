using Steam.Common;

namespace Steam.Net.Messages
{
    internal class StructMessage<T> : IMessage, IHeader<MessageHeader>, IStructBody<T> where T : new()
    {
        public bool IsProtobuf => false;

        public MessageType Type => Header.MessageType;

        public SteamGuid TargetJobId
        {
            get => Header.TargetJobId;
            set => Header.TargetJobId = value;
        }

        public SteamGuid SourceJobId
        {
            get => Header.SourceJobId;
            set => Header.SourceJobId = value;
        }

        public MessageHeader Header { get; set; } = new MessageHeader();

        public T Body { get; set; } = new T();

        public byte[] Payload { get; set; }

        public StructMessage() { }

        public StructMessage(MessageType type)
        {
            Header.MessageType = type;
        }
    }
}
