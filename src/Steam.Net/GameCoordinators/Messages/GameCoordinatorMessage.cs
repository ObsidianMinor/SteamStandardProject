using ProtoBuf;
using Steam.Net.GameCoordinators.Messages.Protobufs;
using Steam.Net.Messages;
using System;
using System.IO;

namespace Steam.Net.GameCoordinators.Messages
{
    /// <summary>
    /// Represents a message sent through a game coordinator
    /// </summary>
    public sealed class GameCoordinatorMessage
    {
        private Body _body;

        /// <summary>
        /// Gets whether this message is a protobuf message
        /// </summary>
        public bool Protobuf { get; }

        /// <summary>
        /// Gets the type of this message
        /// </summary>
        public GameCoordinatorMessageType MessageType { get; }

        public Header Header { get; }

        public object Body => _body;

        private GameCoordinatorMessage(GameCoordinatorMessageType messageType, bool protobuf, Header header, object body)
        {
            MessageType = messageType;
            Protobuf = protobuf;
            Header = header;
            _body = new Body(body);
        }

        private GameCoordinatorMessage(GameCoordinatorMessageType messageType, bool protobuf, Header header, ArraySegment<byte> body)
        {
            MessageType = messageType;
            Protobuf = protobuf;
            Header = header;
            _body = new Body(body);
        }

        public static GameCoordinatorMessage CreateMessage(object value)
        {
            return new GameCoordinatorMessage(0, false, new Header(SteamGid.Invalid), value);
        }

        public static GameCoordinatorMessage CreateMessage(GameCoordinatorMessageType type, object value)
        {
            return new GameCoordinatorMessage(type, false, new Header(SteamGid.Invalid), value);
        }

        public static GameCoordinatorMessage CreateProtobufMessage(GameCoordinatorMessageType type, object value)
        {
            return new GameCoordinatorMessage(type, true, new GameCoordinatorProtobufHeader(new CMsgProtoBufHeader()), value);
        }

        public static GameCoordinatorMessage CreateFromByteArray(GameCoordinatorMessageType type, bool protobuf, byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                if (protobuf)
                {
                    reader.ReadUInt32(); // read message type we already have
                    CMsgProtoBufHeader header;
                    using (MemoryStream protoStream = new MemoryStream(reader.ReadBytes(reader.ReadInt32())))
                        header = Serializer.Deserialize<CMsgProtoBufHeader>(protoStream);

                    return new GameCoordinatorMessage(type, protobuf, new GameCoordinatorProtobufHeader(header), new ArraySegment<byte>(data, (int)stream.Position, (int)stream.Length - (int)stream.Position));
                }
                else
                {
                    reader.ReadUInt16(); // version
                    reader.ReadUInt64(); // target
                    SteamGid source = reader.ReadUInt64();
                    return new GameCoordinatorMessage(type, protobuf, new Header(source), new ArraySegment<byte>(data, (int)stream.Position, (int)stream.Length - (int)stream.Position));
                }
            }
        }

        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }

        public T Deserialize<T>()
        {
            throw new NotImplementedException();
        }
    }
}
