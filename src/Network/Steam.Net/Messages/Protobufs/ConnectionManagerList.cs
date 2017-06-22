using ProtoBuf;
using System.Collections.Generic;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract]
    internal class ConnectionManagerList : IExtensible
    {
        private readonly List<uint> _connectionManagerAddresses = new List<uint>();
        [ProtoMember(1, DataFormat = DataFormat.TwosComplement, IsRequired = false)]
        public List<uint> ConnectionManagerAddresses => _connectionManagerAddresses;

        private readonly List<uint> _connectionManagerPorts = new List<uint>();
        [ProtoMember(2, DataFormat = DataFormat.TwosComplement, IsRequired = false)]
        public List<uint> ConnectionManagerPorts => _connectionManagerPorts;

        private readonly List<string> _webSocketAddresses = new List<string>();
        [ProtoMember(3, IsRequired = false)]
        public List<string> WebSocketAddresses => _webSocketAddresses;

        private uint? _percentDefaultToWebSocket;
        [ProtoMember(4)]
        public uint PercentDefaultToWebSocket
        {
            get => _percentDefaultToWebSocket ?? 0;
            set => _percentDefaultToWebSocket = value;
        }

        public bool PercentDefaultToWebSocketSpecified
        {
            get => _percentDefaultToWebSocket != null;
            set
            {
                if(value == !PercentDefaultToWebSocketSpecified)
                    _percentDefaultToWebSocket = _percentDefaultToWebSocket = value ? PercentDefaultToWebSocket : (uint?)null;
            }
        }

        private bool ShouldSerializePercentDefaultToWebSocket() => PercentDefaultToWebSocketSpecified;
        private void ResetPercentDefaultToWebSocket() => PercentDefaultToWebSocketSpecified = false;

        private IExtension extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing) 
            => Extensible.GetExtensionObject(ref extensionObject, createIfMissing);
    }
}
