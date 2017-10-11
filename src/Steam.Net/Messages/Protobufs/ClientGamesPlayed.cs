using ProtoBuf;
using System.Collections.Generic;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract]
    internal class ClientGamesPlayed
    {
        public uint OsType { get; set; }
        public List<GamePlayed> GamesPlayed { get; set; } = new List<GamePlayed>();
    }

    internal class GamePlayed
    {
        [ProtoMember(1)]
        public ulong? GameServerSteamId { get; set; }
        [ProtoMember(2, DataFormat = DataFormat.FixedSize)]
        public ulong GameId { get; set; }
        [ProtoMember(3)]
        public uint? GameIpAddress { get; set; }
        [ProtoMember(4)]
        public uint? GamePort { get; set; }
        [ProtoMember(5)]
        public bool? IsSecure { get; set; }
        [ProtoMember(6)]
        public byte[] Token { get; set; }
        [ProtoMember(7)]
        public string ExtraGameInfo { get; set; }
        [ProtoMember(8)]
        public byte[] GameDataBlob { get; set; }
        [ProtoMember(9)]
        public uint? ProcessId { get; set; }
        [ProtoMember(10)]
        public uint? StreamingProviderId { get; set; }
        [ProtoMember(11)]
        public uint? GameFlags { get; set; }
        [ProtoMember(12)]
        public uint? OwnerId { get; set; }
        [ProtoMember(13)]
        public string VrHmdVendor { get; set; }
        [ProtoMember(14)]
        public string VrHmdModel { get; set; }
        [ProtoMember(15)]
        public uint LaunchOptionType { get; set; } = 0;
        [ProtoMember(16)]
        public int PrimaryControllerType { get; set; } = -1;
        [ProtoMember(17)]
        public string PrimarySteamControllerSerial { get; set; }
        [ProtoMember(18)]
        public uint TotalSteamControllerCount { get; set; } = 0;
        [ProtoMember(19)]
        public uint TotalNonSteamControllerCount { get; set; } = 0;
        [ProtoMember(20)]
        public ulong ControllerWorkshopFileId { get; set; } = 0;
    }
}