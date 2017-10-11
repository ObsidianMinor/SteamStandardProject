using Steam.Net.Messages.Protobufs;
using System;

namespace Steam.Net.Messages
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This only exposes the App ID routing, target job name, and trace tag parameters 
    /// due to them being the only exposed parameters in CProtoBufNetPacket class in the Steam client
    /// </remarks>
    public class ProtobufClientHeader : ClientHeader
    {
        private uint _routingAppId;
        private ulong _traceTag;

        public long RoutingAppId
        {
            get => _routingAppId;
            set
            {
                if (value > uint.MaxValue || value < uint.MinValue)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _routingAppId = (uint)value;
            }    
        }

        public string TargetJobName { get; internal set; }
        
        public decimal TraceTag
        {
            get => _traceTag;
            set
            {
                if (value > ulong.MaxValue || value < ulong.MinValue)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _traceTag = decimal.ToUInt64(value);
            }
        }

        internal ProtobufClientHeader(ProtobufHeader header) : base(header.jobid_source, header.steamid, header.client_sessionid)
        {
            if (header.routing_appidSpecified)
                _routingAppId = header.routing_appid;

            if (header.target_job_nameSpecified)
                TargetJobName = header.target_job_name;

            if (header.trace_tagSpecified)
                TraceTag = header.trace_tag;
        }

        internal ProtobufHeader CreateProtobuf()
        {
            ProtobufHeader header = new ProtobufHeader();

            if (_routingAppId != 0)
                header.routing_appid = _routingAppId;

            if (_traceTag != 0)
                header.trace_tag = _traceTag;

            if (TargetJobName != null)
                header.target_job_name = TargetJobName;

            if (JobId != 0)
                header.jobid_target = JobId;
            
            header.client_sessionid = SessionId;
            header.steamid = SteamId;

            return header;
        }
    }
}
