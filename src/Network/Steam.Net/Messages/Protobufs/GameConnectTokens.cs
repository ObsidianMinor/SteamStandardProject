using ProtoBuf;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract(Name = "CMsgClientGameConnectTokens")]
    public class GameConnectTokens : IExtensible
    {
        [ProtoMember(1, DataFormat = DataFormat.TwosComplement)]
        public uint MaxTokensToKeep { get; set; } = 10;

        private readonly List<byte[]> _tokens = new List<byte[]>();
        public List<byte[]> Tokens => _tokens;
        
        private IExtension _extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return Extensible.GetExtensionObject(ref _extensionObject, createIfMissing);
        }
    }
}
