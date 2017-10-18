using Steam.Net.Utilities;
using System;
using System.IO;
using ProtoBuf;
using Steam.Net.Messages.Protobufs;

namespace Steam.Net.Messages
{
    /// <summary>
    /// Represents an immutable network message
    /// </summary>
    public sealed class NetworkMessage
    {
        private readonly Body _body;

        public bool Protobuf => Header is ProtobufClientHeader;

        public MessageType MessageType { get; }
        
        /// <summary>
        /// The header of the message
        /// </summary>
        public Header Header { get; }

        /// <summary>
        /// The body of the message
        /// </summary>
        public object Body => _body;

        internal NetworkMessage(MessageType type, Header header, ArraySegment<byte> body)
        {
            MessageType = type;
            _body = new Body(body);
            Header = header;
        }

        internal NetworkMessage(MessageType type, Header header, object body)
        {
            MessageType = type;
            _body = new Body(body);
            Header = header;
        }

        /// <summary>
        /// Creates a struct message without client information in the header
        /// </summary>
        /// <param name="body">The body</param>
        /// <returns>A network message</returns>
        public static NetworkMessage CreateMessage(object body)
        {
            var head = new Header(ulong.MaxValue);
            return new NetworkMessage(0, head, body);
        }

        /// <summary>
        /// Creates a struct message with the specified message ID and without client information in the header
        /// </summary>
        /// <param name="type"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static NetworkMessage CreateMessage(MessageType type, object body)
        {
            var head = new Header(ulong.MaxValue);
            return new NetworkMessage(type, head, body);
        }

        /// <summary>
        /// Creates a struct message with the specified message ID and client information in the header
        /// </summary>
        /// <param name="type"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static NetworkMessage CreateClientMessage(MessageType type, object body)
        {
            var head = new ClientHeader(ulong.MaxValue, SteamId.Zero, 0);
            return new NetworkMessage(type, head, body);
        }

        /// <summary>
        /// Creates a protobuf message with a protobuf header
        /// </summary>
        /// <param name="type"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static NetworkMessage CreateProtobufMessage(MessageType type, object body)
        {
            var head = new ProtobufClientHeader();
            return new NetworkMessage(type, head, body);
        }

        /// <summary>
        /// Creates a protobuf message with a protobuf header with the specified routing app ID
        /// </summary>
        /// <param name="type"></param>
        /// <param name="body"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static NetworkMessage CreateAppRoutedMessage(MessageType type, object body, long appId)
        {
            if (appId < uint.MinValue || appId > uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(appId));

            NetworkMessage message = CreateProtobufMessage(type, body);
            (message.Header as ProtobufClientHeader).RoutingAppId = appId;
            return message;
        }

        public static NetworkMessage CreateFromByteArray(byte[] data) => CreateFromByteArray(data, false);

        public static NetworkMessage CreateFromByteArray(byte[] data, bool server)
        {
            using (MemoryStream stream = new MemoryStream(data, false))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                (MessageType type, bool protobuf) = MessageTypeUtils.SplitMessage(reader.ReadUInt32());

                Header header;

                void ReadHeader()
                {
                    ulong targetJob = reader.ReadUInt64();
                    ulong sourceJob = reader.ReadUInt64();
                    header = new Header(sourceJob);
                }

                if (protobuf)
                {
                    int length = reader.ReadInt32();
                    CMsgProtoBufHeader protobufHeader;
                    using (MemoryStream protoStream = new MemoryStream(reader.ReadBytes(length)))
                        protobufHeader = Serializer.Deserialize<CMsgProtoBufHeader>(protoStream);

                    header = new ProtobufClientHeader(protobufHeader, server);
                }
                else if (stream.Length > 36 && reader.ReadByte() == 36 && reader.ReadUInt16() == 2)
                {
                    // please never fail
                    ulong target = reader.ReadUInt64();
                    ulong source = reader.ReadUInt64();

                    if (reader.ReadByte() != 239)
                    {
                        stream.Seek(4, SeekOrigin.Begin);
                        ReadHeader();
                    }
                    else
                    {
                        ulong steamId = reader.ReadUInt64();
                        int sessionId = reader.ReadInt32();

                        header = new ClientHeader(source, steamId, sessionId); // if we somehow get here and it turns out we're not in the header anymore (and it was a normal header),
                                                                               // tell me so I can shoot myself
                    }
                }
                else
                {
                    stream.Seek(4, SeekOrigin.Begin);
                    ReadHeader();
                }

                return new NetworkMessage(type, header, new ArraySegment<byte>(data, (int)stream.Position, (int)(stream.Length - stream.Position)));
            }
        }
        
        /// <summary>
        /// Deserializes the body as the specified type
        /// </summary>
        /// <typeparam name="T">The type to deserialize as</typeparam>
        /// <returns>The type deserialized</returns>
        public T Deserialize<T>()
        {
            return (T) Deserialize(typeof(T));
        }

        /// <summary>
        /// Deserializes the body as the specified type
        /// </summary>
        /// <param name="type">The type to deserialize as</param>
        /// <returns>An object of the specified type</returns>
        public object Deserialize(Type type)
        {
            return _body.Deserialize(type);
        }

        /// <summary>
        /// Serializes this message as a byte array
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize() => Serialize(false);

        /// <summary>
        /// Serializes this message as a byte array and optionally positions the job ID in the header as though we are a server
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public byte[] Serialize(bool server)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(MessageTypeUtils.MergeMessage(MessageType, Protobuf));
                if (Header is ProtobufClientHeader head)
                {
                    using (MemoryStream protoStream = new MemoryStream())
                    {
                        Serializer.Serialize(protoStream, (Header as ProtobufClientHeader).CreateProtobuf(server));
                        byte[] content = protoStream.ToArray();
                        writer.Write(content.Length);
                        writer.Write(content);
                    }
                }
                else if (Header is ClientHeader extended)
                {
                    writer.Write((byte)36);
                    writer.Write((ushort)2);
                    writer.Write(server ? SteamGid.Invalid : extended.JobId);
                    writer.Write(server ? extended.JobId : SteamGid.Invalid);
                    writer.Write((byte)239);
                    writer.Write(extended.SteamId);
                    writer.Write(extended.SessionId);
                }
                else
                {
                    writer.Write(server ? SteamGid.Invalid : Header.JobId);
                    writer.Write(server ? Header.JobId : SteamGid.Invalid);
                }
                writer.Write(_body.Serialize());
                return stream.ToArray();
            }
        }
    }
}
