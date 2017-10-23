using Steam.Net.Messages.Protobufs;

namespace Steam.Net.Messages
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This only exposes the App ID routing, target job name, and trace tag parameters 
    /// due to them being the only exposed fields in the CProtoBufNetPacket class
    /// </remarks>
    public class ProtobufClientHeader : ClientHeader
    {
        private uint _routingAppId;
        private ulong _traceTag;

        public long RoutingAppId => _routingAppId;

        public string TargetJobName { get; }

        public decimal TraceTag => _traceTag;

        /// <summary>
        /// Clones the current <see cref="ProtobufClientHeader"/>
        /// </summary>
        /// <returns></returns>
        protected override Header Clone()
        {
            return new ProtobufClientHeader(_routingAppId, _traceTag, TargetJobName, JobId, SteamId, SessionId);
        }

        internal ProtobufClientHeader(uint routingAppId, ulong traceTag, string targetJobName, SteamGid jobId, SteamId steamId, int sessionId) : base(jobId, steamId, sessionId)
        {
            _routingAppId = routingAppId;
            _traceTag = traceTag;
            TargetJobName = targetJobName;
        }

        internal ProtobufClientHeader() : base(SteamGid.Invalid, SteamId.Zero, 0) { }

        internal ProtobufClientHeader(CMsgProtoBufHeader header, bool server) : base(server ? header.jobid_source : header.jobid_target, header.steamid, header.client_sessionid)
        {
            if (header.routing_appidSpecified)
                _routingAppId = header.routing_appid;

            if (header.target_job_nameSpecified)
                TargetJobName = header.target_job_name;

            if (header.trace_tagSpecified)
                _traceTag = header.trace_tag;
        }

        internal CMsgProtoBufHeader CreateProtobuf(bool server)
        {
            CMsgProtoBufHeader header = new CMsgProtoBufHeader();

            if (_routingAppId != 0)
                header.routing_appid = _routingAppId;

            if (_traceTag != 0)
                header.trace_tag = _traceTag;

            if (TargetJobName != null)
                header.target_job_name = TargetJobName;

            if (JobId != SteamGid.Invalid)
                header.jobid_source = JobId;

            header.client_sessionid = SessionId;
            header.steamid = SteamId;

            return header;
        }
    }
}
