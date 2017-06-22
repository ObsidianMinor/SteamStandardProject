using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract(Name = "CMsgClientHeartBeat")]
    internal class Heartbeat : IExtensible
    {
        private IExtension _extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return Extensible.GetExtensionObject(ref _extensionObject, createIfMissing);
        }
    }
}
