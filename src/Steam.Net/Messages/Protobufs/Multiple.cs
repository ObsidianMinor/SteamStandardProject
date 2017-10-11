using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract(Name = "CMsgMulti")]
    internal class Multiple : IExtensible
    {
        private uint? _sizeUnzipped;
        [ProtoMember(1, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        public uint SizeUnzipped
        {
            get => _sizeUnzipped ?? default(uint);
            set => _sizeUnzipped = value;
        }

        public bool SizeUnzippedSpecified
        {
            get => _sizeUnzipped != null;
            set
            {
                if (value == !SizeUnzippedSpecified)
                    _sizeUnzipped = value ? SizeUnzipped : (uint?)null;
            }
        }

        private bool ShouldSerializeSizeUnzipped() => SizeUnzippedSpecified;
        private void ResetSizeUnzipped() => SizeUnzippedSpecified = false;
        
        private byte[] _messageBody;
        [ProtoMember(2, IsRequired = false)]
        public byte[] MessageBody
        {
            get => _messageBody ?? null;
            set => _messageBody = value;
        }

        public bool MessageBodySpecified
        {
            get => _messageBody != null;
            set
            {
                if (value == !MessageBodySpecified)
                    _messageBody = value ? MessageBody : null;
            }
        }
        private bool ShouldSerializeMessageBody() => MessageBodySpecified;
        private void ResetMessageBody() => MessageBodySpecified = false;

        private IExtension extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
            => Extensible.GetExtensionObject(ref extensionObject, createIfMissing);
    }
}
