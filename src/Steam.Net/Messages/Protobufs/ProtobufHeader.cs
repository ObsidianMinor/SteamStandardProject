using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract]
    internal class ProtobufHeader : IExtensible
    {
        private ulong? _steamid;
        [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name = @"steamid", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
        public ulong steamid
        {
            get { return _steamid ?? default(ulong); }
            set { _steamid = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool steamidSpecified
        {
            get { return _steamid != null; }
            set { if (value == (_steamid == null)) _steamid = value ? this.steamid : (ulong?)null; }
        }
        private bool ShouldSerializesteamid() { return steamidSpecified; }
        private void Resetsteamid() { steamidSpecified = false; }


        private int? _client_sessionid;
        [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name = @"client_sessionid", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public int client_sessionid
        {
            get { return _client_sessionid ?? default(int); }
            set { _client_sessionid = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool client_sessionidSpecified
        {
            get { return _client_sessionid != null; }
            set { if (value == (_client_sessionid == null)) _client_sessionid = value ? this.client_sessionid : (int?)null; }
        }
        private bool ShouldSerializeclient_sessionid() { return client_sessionidSpecified; }
        private void Resetclient_sessionid() { client_sessionidSpecified = false; }


        private uint? _routing_appid;
        [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name = @"routing_appid", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public uint routing_appid
        {
            get { return _routing_appid ?? default(uint); }
            set { _routing_appid = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool routing_appidSpecified
        {
            get { return _routing_appid != null; }
            set { if (value == (_routing_appid == null)) _routing_appid = value ? this.routing_appid : (uint?)null; }
        }
        private bool ShouldSerializerouting_appid() { return routing_appidSpecified; }
        private void Resetrouting_appid() { routing_appidSpecified = false; }


        private ulong? _jobid_source;
        [global::ProtoBuf.ProtoMember(10, IsRequired = false, Name = @"jobid_source", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
        public ulong jobid_source
        {
            get { return _jobid_source ?? (ulong)18446744073709551615; }
            set { _jobid_source = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool jobid_sourceSpecified
        {
            get { return _jobid_source != null; }
            set { if (value == (_jobid_source == null)) _jobid_source = value ? this.jobid_source : (ulong?)null; }
        }
        private bool ShouldSerializejobid_source() { return jobid_sourceSpecified; }
        private void Resetjobid_source() { jobid_sourceSpecified = false; }


        private ulong? _jobid_target;
        [global::ProtoBuf.ProtoMember(11, IsRequired = false, Name = @"jobid_target", DataFormat = global::ProtoBuf.DataFormat.FixedSize)]
        public ulong jobid_target
        {
            get { return _jobid_target ?? (ulong)18446744073709551615; }
            set { _jobid_target = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool jobid_targetSpecified
        {
            get { return _jobid_target != null; }
            set { if (value == (_jobid_target == null)) _jobid_target = value ? this.jobid_target : (ulong?)null; }
        }
        private bool ShouldSerializejobid_target() { return jobid_targetSpecified; }
        private void Resetjobid_target() { jobid_targetSpecified = false; }


        private string _target_job_name;
        [global::ProtoBuf.ProtoMember(12, IsRequired = false, Name = @"target_job_name", DataFormat = global::ProtoBuf.DataFormat.Default)]
        public string target_job_name
        {
            get { return _target_job_name ?? ""; }
            set { _target_job_name = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool target_job_nameSpecified
        {
            get { return _target_job_name != null; }
            set { if (value == (_target_job_name == null)) _target_job_name = value ? this.target_job_name : (string)null; }
        }
        private bool ShouldSerializetarget_job_name() { return target_job_nameSpecified; }
        private void Resettarget_job_name() { target_job_nameSpecified = false; }


        private int? _seq_num;
        [global::ProtoBuf.ProtoMember(24, IsRequired = false, Name = @"seq_num", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public int seq_num
        {
            get { return _seq_num ?? default(int); }
            set { _seq_num = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool seq_numSpecified
        {
            get { return _seq_num != null; }
            set { if (value == (_seq_num == null)) _seq_num = value ? this.seq_num : (int?)null; }
        }
        private bool ShouldSerializeseq_num() { return seq_numSpecified; }
        private void Resetseq_num() { seq_numSpecified = false; }


        private int? _eresult;
        [global::ProtoBuf.ProtoMember(13, IsRequired = false, Name = @"eresult", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public int eresult
        {
            get { return _eresult ?? (int)2; }
            set { _eresult = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool eresultSpecified
        {
            get { return _eresult != null; }
            set { if (value == (_eresult == null)) _eresult = value ? this.eresult : (int?)null; }
        }
        private bool ShouldSerializeeresult() { return eresultSpecified; }
        private void Reseteresult() { eresultSpecified = false; }


        private string _error_message;
        [global::ProtoBuf.ProtoMember(14, IsRequired = false, Name = @"error_message", DataFormat = global::ProtoBuf.DataFormat.Default)]
        public string error_message
        {
            get { return _error_message ?? ""; }
            set { _error_message = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool error_messageSpecified
        {
            get { return _error_message != null; }
            set { if (value == (_error_message == null)) _error_message = value ? this.error_message : (string)null; }
        }
        private bool ShouldSerializeerror_message() { return error_messageSpecified; }
        private void Reseterror_message() { error_messageSpecified = false; }


        private uint? _ip;
        [global::ProtoBuf.ProtoMember(15, IsRequired = false, Name = @"ip", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public uint ip
        {
            get { return _ip ?? default(uint); }
            set { _ip = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool ipSpecified
        {
            get { return _ip != null; }
            set { if (value == (_ip == null)) _ip = value ? this.ip : (uint?)null; }
        }
        private bool ShouldSerializeip() { return ipSpecified; }
        private void Resetip() { ipSpecified = false; }


        private uint? _auth_account_flags;
        [global::ProtoBuf.ProtoMember(16, IsRequired = false, Name = @"auth_account_flags", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public uint auth_account_flags
        {
            get { return _auth_account_flags ?? default(uint); }
            set { _auth_account_flags = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool auth_account_flagsSpecified
        {
            get { return _auth_account_flags != null; }
            set { if (value == (_auth_account_flags == null)) _auth_account_flags = value ? this.auth_account_flags : (uint?)null; }
        }
        private bool ShouldSerializeauth_account_flags() { return auth_account_flagsSpecified; }
        private void Resetauth_account_flags() { auth_account_flagsSpecified = false; }


        private uint? _token_source;
        [global::ProtoBuf.ProtoMember(22, IsRequired = false, Name = @"token_source", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public uint token_source
        {
            get { return _token_source ?? default(uint); }
            set { _token_source = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool token_sourceSpecified
        {
            get { return _token_source != null; }
            set { if (value == (_token_source == null)) _token_source = value ? this.token_source : (uint?)null; }
        }
        private bool ShouldSerializetoken_source() { return token_sourceSpecified; }
        private void Resettoken_source() { token_sourceSpecified = false; }


        private bool? _admin_spoofing_user;
        [global::ProtoBuf.ProtoMember(23, IsRequired = false, Name = @"admin_spoofing_user", DataFormat = global::ProtoBuf.DataFormat.Default)]
        public bool admin_spoofing_user
        {
            get { return _admin_spoofing_user ?? default(bool); }
            set { _admin_spoofing_user = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool admin_spoofing_userSpecified
        {
            get { return _admin_spoofing_user != null; }
            set { if (value == (_admin_spoofing_user == null)) _admin_spoofing_user = value ? this.admin_spoofing_user : (bool?)null; }
        }
        private bool ShouldSerializeadmin_spoofing_user() { return admin_spoofing_userSpecified; }
        private void Resetadmin_spoofing_user() { admin_spoofing_userSpecified = false; }


        private int? _transport_error;
        [global::ProtoBuf.ProtoMember(17, IsRequired = false, Name = @"transport_error", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public int transport_error
        {
            get { return _transport_error ?? (int)1; }
            set { _transport_error = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool transport_errorSpecified
        {
            get { return _transport_error != null; }
            set { if (value == (_transport_error == null)) _transport_error = value ? this.transport_error : (int?)null; }
        }
        private bool ShouldSerializetransport_error() { return transport_errorSpecified; }
        private void Resettransport_error() { transport_errorSpecified = false; }


        private ulong? _messageid;
        [global::ProtoBuf.ProtoMember(18, IsRequired = false, Name = @"messageid", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public ulong messageid
        {
            get { return _messageid ?? (ulong)18446744073709551615; }
            set { _messageid = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool messageidSpecified
        {
            get { return _messageid != null; }
            set { if (value == (_messageid == null)) _messageid = value ? this.messageid : (ulong?)null; }
        }
        private bool ShouldSerializemessageid() { return messageidSpecified; }
        private void Resetmessageid() { messageidSpecified = false; }


        private uint? _publisher_group_id;
        [global::ProtoBuf.ProtoMember(19, IsRequired = false, Name = @"publisher_group_id", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public uint publisher_group_id
        {
            get { return _publisher_group_id ?? default(uint); }
            set { _publisher_group_id = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool publisher_group_idSpecified
        {
            get { return _publisher_group_id != null; }
            set { if (value == (_publisher_group_id == null)) _publisher_group_id = value ? this.publisher_group_id : (uint?)null; }
        }
        private bool ShouldSerializepublisher_group_id() { return publisher_group_idSpecified; }
        private void Resetpublisher_group_id() { publisher_group_idSpecified = false; }


        private uint? _sysid;
        [global::ProtoBuf.ProtoMember(20, IsRequired = false, Name = @"sysid", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public uint sysid
        {
            get { return _sysid ?? default(uint); }
            set { _sysid = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool sysidSpecified
        {
            get { return _sysid != null; }
            set { if (value == (_sysid == null)) _sysid = value ? this.sysid : (uint?)null; }
        }
        private bool ShouldSerializesysid() { return sysidSpecified; }
        private void Resetsysid() { sysidSpecified = false; }


        private ulong? _trace_tag;
        [global::ProtoBuf.ProtoMember(21, IsRequired = false, Name = @"trace_tag", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public ulong trace_tag
        {
            get { return _trace_tag ?? default(ulong); }
            set { _trace_tag = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool trace_tagSpecified
        {
            get { return _trace_tag != null; }
            set { if (value == (_trace_tag == null)) _trace_tag = value ? this.trace_tag : (ulong?)null; }
        }
        private bool ShouldSerializetrace_tag() { return trace_tagSpecified; }
        private void Resettrace_tag() { trace_tagSpecified = false; }


        private uint? _webapi_key_id;
        [global::ProtoBuf.ProtoMember(25, IsRequired = false, Name = @"webapi_key_id", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public uint webapi_key_id
        {
            get { return _webapi_key_id ?? default(uint); }
            set { _webapi_key_id = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool webapi_key_idSpecified
        {
            get { return _webapi_key_id != null; }
            set { if (value == (_webapi_key_id == null)) _webapi_key_id = value ? this.webapi_key_id : (uint?)null; }
        }
        private bool ShouldSerializewebapi_key_id() { return webapi_key_idSpecified; }
        private void Resetwebapi_key_id() { webapi_key_idSpecified = false; }


        private bool? _is_from_external_source;
        [global::ProtoBuf.ProtoMember(26, IsRequired = false, Name = @"is_from_external_source", DataFormat = global::ProtoBuf.DataFormat.Default)]
        public bool is_from_external_source
        {
            get { return _is_from_external_source ?? default(bool); }
            set { _is_from_external_source = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool is_from_external_sourceSpecified
        {
            get { return _is_from_external_source != null; }
            set { if (value == (_is_from_external_source == null)) _is_from_external_source = value ? this.is_from_external_source : (bool?)null; }
        }
        private bool ShouldSerializeis_from_external_source() { return is_from_external_sourceSpecified; }
        private void Resetis_from_external_source() { is_from_external_sourceSpecified = false; }

        private readonly global::System.Collections.Generic.List<uint> _forward_to_sysid = new global::System.Collections.Generic.List<uint>();
        [global::ProtoBuf.ProtoMember(27, Name = @"forward_to_sysid", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public global::System.Collections.Generic.List<uint> forward_to_sysid
        {
            get { return _forward_to_sysid; }
        }

        private global::ProtoBuf.IExtension extensionObject;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
    }
}