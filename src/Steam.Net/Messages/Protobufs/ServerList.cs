using ProtoBuf;
using System.Collections.Generic;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract]
    internal class ServerList : IExtensible
    {
        private readonly List<Server> _servers = new List<Server>();
        [ProtoMember(1, IsRequired = false)]
        public List<Server> Servers => _servers;

        IExtension extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref extensionObject, createIfMissing);
    }

    [ProtoContract]
    internal class Server : IExtensible
    {
        private uint? _serverType;
        [ProtoMember(1, IsRequired = false)]
        public uint ServerType
        {
            get => _serverType ?? 0;
            set => _serverType = value;
        }

        public bool ServerTypeSpecified
        {
            get => _serverType != null;
            set
            {
                if (value == !ServerTypeSpecified)
                    _serverType = value ? ServerType : (uint?)null;
            }
        }

        private bool ShouldSerializeServerType() => ServerTypeSpecified;
        private void ResetServerType() => ServerTypeSpecified = false;

        private uint? _serverIp;
        [ProtoMember(2, IsRequired = false)]
        public uint ServerIp
        {
            get => _serverIp ?? 0;
            set => _serverIp = value;
        }

        public bool ServerIpSpecified
        {
            get => _serverIp != null;
            set
            {
                if (value == !ServerIpSpecified)
                    _serverIp = value ? ServerIp : (uint?)null;
            }
        }

        private bool ShouldSerializeServerIp() => ServerIpSpecified;
        private void ResetServerIp() => ServerIpSpecified = false;

        private uint? _serverPort;
        [ProtoMember(3, IsRequired = false)]
        public uint ServerPort
        {
            get => _serverPort ?? 0;
            set => _serverPort = value;
        }

        public bool ServerPortSpecified
        {
            get => _serverIp != null;
            set
            {
                if (value == !ServerPortSpecified)
                    _serverIp = value ? ServerPort : (uint?)null;
            }
        }

        private bool ShouldSerializeServerPort() => ServerPortSpecified;
        private void ResetServerPort() => ServerPortSpecified = false;

        IExtension extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref extensionObject, createIfMissing);
    }
}
