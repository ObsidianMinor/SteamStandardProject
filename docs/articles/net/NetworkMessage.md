# NetworkMessage

`NetworkMessage` is an abstraction over the various types of messages sent to and from Steam. It consists of a header and a body which can be either a protobuf or a struct.

A `NetworkMessage` is immutable and can be created with `CreateMessage`, `CreateClientMessage`, or `CreateProtobufMessage`. App routed messages can also be created with `CreateAppRoutedMessage`. A message can be deserialized from a byte array with `CreateFromByteArray`. An overload for `CreateFromByteArray` can deserialize a message as though it were a server, reading the job ID from the proper position.

"CreateMessage" methods accept `null` bodies and will only have the header serialized.

A job ID can be added to any message with `WithJobId`. Since headers are also immutable, this will create a new `NetworkMessage` with a header with the specified job ID.

Client information can be added to any message with `WithClientInfo`. If the current header is not a `ClientHeader`, `WithClientInfo` returns the message it was called on.

Messages are serialized with the `Serialize` method and create a byte array as a result. An overload is provided to allow the class to serialize the message as though it were a server, positioning the job ID accordingly.

## API Surface

```csharp
public sealed class NetworkMessage
{
    public bool Protobuf { get; }
    public MessageType MessageType { get; }
    public Header Header { get; }
    public object Body { get; }

    public static NetworkMessage CreateMessage(object body);
    public static NetworkMessage CreateMessage(MessageType type, object body);
    public static NetworkMessage CreateClientMessage(MessageType type, object body);
    public static NetworkMessage CreateProtobufMessage(MessageType type, object body);
    public static NetworkMessage CreateAppRoutedMessage(MessageType type, long appId, object body);
    public static NetworkMessage CreateFromByteArray(byte[] data);
    public static NetworkMessage CreateFromByteArray(byte[] data, bool server);

    public NetworkMessage WithJobId(SteamGid job);
    public NetworkMessage WithClientInfo(SteamId id, int sessionId);

    public object Deserialize(Type type);
    public T Deserialize<T>();

    public byte[] Serialize();
    public byte[] Serialize(bool server);
}
```