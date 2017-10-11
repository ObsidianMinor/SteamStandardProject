using ProtoBuf;
using Steam.Net.Messages;
using Steam.Net.Messages.Protobufs;
using System;
using System.IO;

namespace Steam.Net.GameCoordinators.Messages
{
    public sealed class GameCoordinatorMessage
    {
        private Body _body;

        public bool Protobuf { get; }

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
            throw new NotImplementedException();
        }

        public static GameCoordinatorMessage CreateMessage(GameCoordinatorMessageType type, object value)
        {
            throw new NotImplementedException();
        }

        public static GameCoordinatorMessage CreateProtobufMessage(GameCoordinatorMessageType type, object value)
        {
            throw new NotImplementedException();
        }

        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }

        public T Deserialize<T>()
        {
            throw new NotImplementedException();
        }

        internal static GameCoordinatorMessage Deserialize(GameCoordinatorMessageType messageType, bool protobuf, byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
                if (protobuf)
                {
                    reader.ReadUInt32(); // read message type we already have
                    GameCoordinatorHeader header;
                    using (MemoryStream protoStream = new MemoryStream(reader.ReadBytes(reader.ReadInt32())))
                        header = Serializer.Deserialize<GameCoordinatorHeader>(protoStream);

                    return new GameCoordinatorMessage(messageType, protobuf, new GameCoordinatorProtobufHeader(header), new ArraySegment<byte>(data, (int)stream.Position, (int)stream.Length - (int)stream.Position));
                }
                else
                {
                    reader.ReadUInt16();reader.ReadUInt64();
                    SteamGid source = reader.ReadUInt64();
                    return new GameCoordinatorMessage(messageType, protobuf, new Header(source), new ArraySegment<byte>(data, (int)stream.Position, (int)stream.Length - (int)stream.Position));
                }
        }
    }
}
