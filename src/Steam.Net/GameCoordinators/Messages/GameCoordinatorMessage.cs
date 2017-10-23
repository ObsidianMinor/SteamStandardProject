using ProtoBuf;
using Steam.Net.GameCoordinators.Messages.Protobufs;
using Steam.Net.Messages;
using Steam.Net.Utilities;
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

        /// <summary>
        /// Gets the header of this message
        /// </summary>
        public Header Header { get; }

        /// <summary>
        /// Gets the body of this message
        /// </summary>
        public object Body => _body;

        private GameCoordinatorMessage(GameCoordinatorMessageType messageType, bool protobuf, Header header, Body body)
        {
            MessageType = messageType;
            Protobuf = protobuf;
            Header = header;
            _body = body;
        }

        private GameCoordinatorMessage(GameCoordinatorMessageType messageType, bool protobuf, Header header, object body) : this(messageType, protobuf, header, new Body(body)) { }

        private GameCoordinatorMessage(GameCoordinatorMessageType messageType, bool protobuf, Header header, ArraySegment<byte> body) : this(messageType, protobuf, header, new Body(body)) { }

        public GameCoordinatorMessage WithJobId(SteamGid jobId)
        {
            return new GameCoordinatorMessage(MessageType, Protobuf, Header.WithJobId(jobId), _body);
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
                    reader.ReadUInt16(); // version, always 1
                    SteamGid target = reader.ReadUInt64();
                    SteamGid source = reader.ReadUInt64();
                    return new GameCoordinatorMessage(type, protobuf, new Header(source), new ArraySegment<byte>(data, (int)stream.Position, (int)stream.Length - (int)stream.Position));
                }
            }
        }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                if (Protobuf)
                {
                    writer.Write(MessageTypeUtils.MergeMessage((uint)MessageType, Protobuf));
                    using (MemoryStream protoStream = new MemoryStream())
                    {
                        Serializer.Serialize(protoStream, (Header as GameCoordinatorProtobufHeader));
                        writer.Write((uint)protoStream.Length);
                        writer.Write(protoStream.ToArray());
                    }
                }
                else
                {
                    writer.Write((ushort)1);
                    writer.Write(SteamGid.Invalid);
                    writer.Write(Header.JobId);
                }

                writer.Write(_body.Serialize());
                return stream.ToArray();
            }
        }

        public T Deserialize<T>()
        {
            return (T)Deserialize(typeof(T));
        }

        public object Deserialize(Type type)
        {
            return _body.Deserialize(type);
        }
    }
}
