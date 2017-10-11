using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract(Name = "CMsgAuthTicket")]
    internal class AuthTicket : IExtensible
    {
        [ProtoMember(1, DataFormat = DataFormat.TwosComplement)]
        public uint State { get; set; }

        [ProtoMember(2, DataFormat = DataFormat.TwosComplement)]
        public uint Result { get; set; }

        [ProtoMember(3, DataFormat = DataFormat.FixedSize)]
        public ulong SteamId { get; set; }

        [ProtoMember(4, DataFormat = DataFormat.FixedSize)]
        public ulong GameId { get; set; }

        [ProtoMember(5, DataFormat = DataFormat.TwosComplement)]
        public uint SteamPipe { get; set; }

        [ProtoMember(6, DataFormat = DataFormat.TwosComplement)]
        public uint TicketCrc { get; set; }

        [ProtoMember(7, DataFormat = DataFormat.Default)]
        public byte[] Ticket { get; set; }

        private IExtension _extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return Extensible.GetExtensionObject(ref _extensionObject, createIfMissing);
        }
    }
}
