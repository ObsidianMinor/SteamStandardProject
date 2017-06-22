using ProtoBuf;

namespace Steam.Net.Messages
{
    internal class ClientProtobufMessage<T> : ClientProtobufMessageHeader, IProtobufBody<T> where T : IExtensible, new()
    {
        public T Body { get; set; } = new T();

        public ClientProtobufMessage() { }

        public ClientProtobufMessage(MessageType type)
        {
            Header.Type = type;
        }
    }
}
