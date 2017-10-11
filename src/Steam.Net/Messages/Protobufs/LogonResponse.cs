using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract(Name = @"CMsgClientLogonResponse")]
    internal partial class LogonResponse : IExtensible
    {
        [ProtoMember(1, DataFormat = DataFormat.TwosComplement)]
        public int Result { get; set; } = 2;
        
        [ProtoMember(2, DataFormat = DataFormat.TwosComplement)]
        public int OutOfGameHeartbeatSeconds { get; set; }
        
        [ProtoMember(3, DataFormat = DataFormat.TwosComplement)]
        public int InGameHeartbeatSeconds { get; set; }
        
        [ProtoMember(4, DataFormat = DataFormat.TwosComplement)]
        public uint PublicIp { get; set; }
        
        [ProtoMember(5, DataFormat = DataFormat.FixedSize)]
        public uint Time32ServerTime { get; set; }
        
        [ProtoMember(6, DataFormat = DataFormat.TwosComplement)]
        public uint AccountFlags { get; set; }
        
        [ProtoMember(7, DataFormat = DataFormat.TwosComplement)]
        public uint CellId { get; set; }

        [ProtoMember(8)]
        public string EmailDomain { get; set; } = "";
        
        [ProtoMember(9)]
        public byte[] Steam2Ticket { get; set; }
        
        [ProtoMember(10, DataFormat = DataFormat.TwosComplement)]
        public int ResultExtended { get; set; }
        
        [ProtoMember(11)]
        public string WebApiAuthenticateUserNOnce { get; set; } = "";

        [ProtoMember(12, DataFormat = DataFormat.TwosComplement)]
        public uint CellIdPingThreshold { get; set; }
        
        [ProtoMember(13)]
        public bool UsePics { get; set; }

        [ProtoMember(14)]
        public string VanityUrl { get; set; } = "";
        
        [ProtoMember(20, DataFormat = DataFormat.FixedSize)]
        public ulong ClientSuppliedSteamid { get; set; }
        
        [ProtoMember(21)]
        public string IpCountryCode { get; set; } = "";
        
        [ProtoMember(22)]
        public byte[] ParentalSettings { get; set; }
        
        [ProtoMember(23)]
        public byte[] ParentalSettingSignature { get; set; }
        
        [ProtoMember(24, DataFormat = DataFormat.TwosComplement)]
        public int CountLoginFailuresToMigrate { get; set; }
        
        [ProtoMember(25, DataFormat = DataFormat.TwosComplement)]
        public int CountDisconnectsToMigrate { get; set; }

        [ProtoMember(26, DataFormat = DataFormat.TwosComplement)]
        public int OgsDataReportTimeWindow { get; set; }
        
        [ProtoMember(27, DataFormat = DataFormat.TwosComplement)]
        public ulong ClientInstanceId { get; set; }

        private IExtension extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing) => Extensible.GetExtensionObject(ref extensionObject, createIfMissing);
    }
}
