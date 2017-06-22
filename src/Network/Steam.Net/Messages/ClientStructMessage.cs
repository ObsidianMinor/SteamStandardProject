namespace Steam.Net.Messages
{
    internal class ClientStructMessage<T> : ClientProtobufMessageHeader, IClientMessage, IPayload where T : new()
    {
        public T Body { get; set; }

        public ClientStructMessage() { }

        public ClientStructMessage(MessageType type)
        {
            Header.Type = type;
        }
    }
}
