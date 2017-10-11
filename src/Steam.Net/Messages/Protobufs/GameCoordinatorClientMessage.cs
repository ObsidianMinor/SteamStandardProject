using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract]
    internal class GameCoordinatorClientMessage : IExtensible
    {
        private uint? _appId;
        [ProtoMember(1, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        public uint AppId
        {
            get => _appId ?? default;
            set => _appId = value;
        }

        public bool AppIdSpecified
        {
            get => _appId != null;
            set
            {
                if (value == !AppIdSpecified)
                    _appId = value ? AppId : default;
            }
        }

        private bool ShouldSerializeAppId() => AppIdSpecified;
        private void ResetAppId() => AppIdSpecified = false;

        private uint? _messageType;
        [ProtoMember(2, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        public uint MessageType
        {
            get => _messageType ?? default;
            set => _messageType = value;
        }

        public bool MessageTypeSpecified
        {
            get => _messageType != null;
            set
            {
                if (value == !MessageTypeSpecified)
                    _messageType = value ? MessageType : default;
            }
        }

        private bool ShouldSerializeMessageType() => MessageTypeSpecified;
        private void ResetMessageType() => MessageTypeSpecified = false;

        private byte[] _payload;
        [ProtoMember(3, IsRequired = false)]
        public byte[] Payload { get; set; }

        public bool PayloadSpecified
        {
            get => _payload != null;
            set
            {
                if (value == !PayloadSpecified)
                    _payload = value ? Payload : default;
            }
        }

        private bool ShouldSerializePayload() => PayloadSpecified;
        private void ResetPayload() => PayloadSpecified = false;

        private ulong? _steamId;
        [ProtoMember(4, IsRequired = false, DataFormat = DataFormat.FixedSize)]
        public ulong SteamId
        {
            get => _steamId ?? 0;
            set => _steamId = value;
        }

        public bool SteamIdSpecified
        {
            get => _steamId != null;
            set
            {
                if (value == !SteamIdSpecified)
                    _steamId = value ? SteamId : default;
            }
        }

        private bool ShouldSerializeSteamId() => SteamIdSpecified;
        private void ResetSteamId() => SteamIdSpecified = false;

        private string _name;
        [ProtoMember(5, IsRequired = false)]
        public string Name
        {
            get => _name ?? "";
            set => _name = value;
        }

        public bool NameSpecified
        {
            get => _name != null;
            set
            {
                if (value == !NameSpecified)
                    _name = value ? Name : default;
            }
        }

        private bool ShouldSerializeName() => NameSpecified;
        private void ResetName() => NameSpecified = false;

        private IExtension _extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return Extensible.GetExtensionObject(ref _extensionObject, createIfMissing);
        }
    }
}