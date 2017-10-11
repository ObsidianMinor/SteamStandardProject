using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract]
    internal class MasterServerQuery
    {
        [ProtoMember(1, IsRequired = false,DataFormat = DataFormat.TwosComplement)]
        internal uint AppId { get; set; }
        [ProtoMember(2, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        internal uint? GeoLocationIp { get; set; }
        [ProtoMember(3, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        internal uint? RegionCode { get; set; }
        [ProtoMember(4, IsRequired = false)]
        internal string FilterText { get; set; }
        [ProtoMember(5, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        internal uint? MaxServers { get; set; }
    }
}
