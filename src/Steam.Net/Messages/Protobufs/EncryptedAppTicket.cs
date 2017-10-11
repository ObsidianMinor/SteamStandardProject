using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract(Name = "EncryptedAppTicket")]
    internal class EncryptedAppTicket : IExtensible
    {
        [ProtoMember(1, DataFormat = DataFormat.TwosComplement)]
        public uint TicketVersion { get; set; }

        [ProtoMember(2, DataFormat = DataFormat.TwosComplement)]
        public uint CrcEncryptedTicket { get; set; }

        [ProtoMember(3, DataFormat = DataFormat.TwosComplement)]
        public uint CbEncryptedUserData { get; set; }

        [ProtoMember(4, DataFormat = DataFormat.TwosComplement)]
        public uint CbEncryptedAppOwnershipTicket { get; set; }

        [ProtoMember(5, DataFormat = DataFormat.Default)]
        public byte[] EncryptedTicket { get; set; }

        private IExtension _extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return Extensible.GetExtensionObject(ref _extensionObject, createIfMissing);
        }
    }
}
