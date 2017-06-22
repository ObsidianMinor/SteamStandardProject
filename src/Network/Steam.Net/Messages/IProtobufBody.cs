using ProtoBuf;

namespace Steam.Net.Messages
{
    internal interface IProtobufBody<T> where T : IExtensible, new()
    {
        T Body { get; set; }
    }
}
