using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract(Name = "CMsgAppRights")]
    internal class AppRights : IExtensible
    {
        [ProtoMember(1)]
        public bool EditInfo { get; set; }
        [ProtoMember(2)]
        public bool Publish { get; set; }
        [ProtoMember(3)]
        public bool ViewErrorData { get; set; }
        [ProtoMember(4)]
        public bool Download { get; set; }
        [ProtoMember(5)]
        public bool UploadCdKeys { get; set; }
        [ProtoMember(6)]
        public bool GenerateCdKeys { get; set; }
        [ProtoMember(7)]
        public bool ViewFinancials { get; set; }
        [ProtoMember(8)]
        public bool ManageCeg { get; set; }
        [ProtoMember(9)]
        public bool ManageSigning { get; set; }
        [ProtoMember(10)]
        public bool ManageCdKeys { get; set; }
        [ProtoMember(11)]
        public bool EditMarketing { get; set; }
        [ProtoMember(12)]
        public bool EconomySupport { get; set; }
        [ProtoMember(13)]
        public bool EconomySupportSupervisor { get; set; }
        [ProtoMember(14)]
        public bool ManagePricing { get; set; }
        [ProtoMember(15)]
        public bool BroadcastLive { get; set; }

        private IExtension _extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        {
            return Extensible.GetExtensionObject(ref _extensionObject, createIfMissing);
        }
    }
}
