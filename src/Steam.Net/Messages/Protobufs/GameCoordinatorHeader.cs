using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract]
    internal class GameCoordinatorHeader : IExtensible
    {
        [ProtoMember(1, IsRequired = false, DataFormat = DataFormat.FixedSize)]
        public ulong? ClientSteamId { get; set; }

        [ProtoMember(2, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        public int? ClientSessionId { get; set; }

        [ProtoMember(3, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        public uint? SourceAppId { get; set; }

        [ProtoMember(10, IsRequired = false, DataFormat = DataFormat.FixedSize)]
        public ulong? JobIdSource { get; set; }

        [ProtoMember(11, IsRequired = false, DataFormat = DataFormat.FixedSize)]
        public ulong? JobIdTarget { get; set; }

        [ProtoMember(12, IsRequired = false)]
        public string TargetJobName { get; set; }
        
        [ProtoMember(13, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        public int? EResult { get; set; }

        [ProtoMember(14, IsRequired = false)]
        public string ErrorMessage { get; set; }

        [ProtoMember(200, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        public MessageSource? Source { get; set; }

        [ProtoMember(201, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        public uint? IndexSource { get; set; }

        private IExtension _extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing) 
            => Extensible.GetExtensionObject(ref _extensionObject, createIfMissing);
    }
}
