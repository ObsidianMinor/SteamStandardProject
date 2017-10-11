using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract]
    internal class LogonRequest : IExtensible
    {
        private uint? _protocol_version;
        [ProtoMember(1, IsRequired = false, Name = @"protocol_version", DataFormat = DataFormat.TwosComplement)]
        public uint protocol_version
        {
            get { return _protocol_version ?? default(uint); }
            set { _protocol_version = value; }
        }
        

        public bool protocol_versionSpecified
        {
            get { return _protocol_version != null; }
            set { if (value == (_protocol_version == null)) _protocol_version = value ? this.protocol_version : (uint?)null; }
        }
        private bool ShouldSerializeprotocol_version() { return protocol_versionSpecified; }
        private void Resetprotocol_version() { protocol_versionSpecified = false; }


        private uint? _obfustucated_private_ip;
        [ProtoMember(2, IsRequired = false, Name = @"obfustucated_private_ip", DataFormat = DataFormat.TwosComplement)]
        public uint obfustucated_private_ip
        {
            get { return _obfustucated_private_ip ?? default(uint); }
            set { _obfustucated_private_ip = value; }
        }
        

        public bool obfustucated_private_ipSpecified
        {
            get { return _obfustucated_private_ip != null; }
            set { if (value == (_obfustucated_private_ip == null)) _obfustucated_private_ip = value ? this.obfustucated_private_ip : (uint?)null; }
        }
        private bool ShouldSerializeobfustucated_private_ip() { return obfustucated_private_ipSpecified; }
        private void Resetobfustucated_private_ip() { obfustucated_private_ipSpecified = false; }


        private uint? _cell_id;
        [ProtoMember(3, IsRequired = false, Name = @"cell_id", DataFormat = DataFormat.TwosComplement)]
        public uint cell_id
        {
            get { return _cell_id ?? default(uint); }
            set { _cell_id = value; }
        }
        

        public bool cell_idSpecified
        {
            get { return _cell_id != null; }
            set { if (value == (_cell_id == null)) _cell_id = value ? this.cell_id : (uint?)null; }
        }
        private bool ShouldSerializecell_id() { return cell_idSpecified; }
        private void Resetcell_id() { cell_idSpecified = false; }


        private uint? _last_session_id;
        [ProtoMember(4, IsRequired = false, Name = @"last_session_id", DataFormat = DataFormat.TwosComplement)]
        public uint last_session_id
        {
            get { return _last_session_id ?? default(uint); }
            set { _last_session_id = value; }
        }
        

        public bool last_session_idSpecified
        {
            get { return _last_session_id != null; }
            set { if (value == (_last_session_id == null)) _last_session_id = value ? this.last_session_id : (uint?)null; }
        }
        private bool ShouldSerializelast_session_id() { return last_session_idSpecified; }
        private void Resetlast_session_id() { last_session_idSpecified = false; }


        private uint? _client_package_version;
        [ProtoMember(5, IsRequired = false, Name = @"client_package_version", DataFormat = DataFormat.TwosComplement)]
        public uint client_package_version
        {
            get { return _client_package_version ?? default(uint); }
            set { _client_package_version = value; }
        }
        

        public bool client_package_versionSpecified
        {
            get { return _client_package_version != null; }
            set { if (value == (_client_package_version == null)) _client_package_version = value ? this.client_package_version : (uint?)null; }
        }
        private bool ShouldSerializeclient_package_version() { return client_package_versionSpecified; }
        private void Resetclient_package_version() { client_package_versionSpecified = false; }


        private string _client_language;
        [ProtoMember(6, IsRequired = false, Name = @"client_language", DataFormat = DataFormat.Default)]
        public string client_language
        {
            get { return _client_language ?? ""; }
            set { _client_language = value; }
        }
        

        public bool client_languageSpecified
        {
            get { return _client_language != null; }
            set { if (value == (_client_language == null)) _client_language = value ? this.client_language : (string)null; }
        }
        private bool ShouldSerializeclient_language() { return client_languageSpecified; }
        private void Resetclient_language() { client_languageSpecified = false; }


        private uint? _client_os_type;
        [ProtoMember(7, IsRequired = false, Name = @"client_os_type", DataFormat = DataFormat.TwosComplement)]
        public uint client_os_type
        {
            get { return _client_os_type ?? default(uint); }
            set { _client_os_type = value; }
        }
        

        public bool client_os_typeSpecified
        {
            get { return _client_os_type != null; }
            set { if (value == (_client_os_type == null)) _client_os_type = value ? this.client_os_type : (uint?)null; }
        }
        private bool ShouldSerializeclient_os_type() { return client_os_typeSpecified; }
        private void Resetclient_os_type() { client_os_typeSpecified = false; }


        private bool? _should_remember_password;
        [ProtoMember(8, IsRequired = false, Name = @"should_remember_password", DataFormat = DataFormat.Default)]
        public bool should_remember_password
        {
            get { return _should_remember_password ?? (bool)false; }
            set { _should_remember_password = value; }
        }
        

        public bool should_remember_passwordSpecified
        {
            get { return _should_remember_password != null; }
            set { if (value == (_should_remember_password == null)) _should_remember_password = value ? this.should_remember_password : (bool?)null; }
        }
        private bool ShouldSerializeshould_remember_password() { return should_remember_passwordSpecified; }
        private void Resetshould_remember_password() { should_remember_passwordSpecified = false; }


        private string _wine_version;
        [ProtoMember(9, IsRequired = false, Name = @"wine_version", DataFormat = DataFormat.Default)]
        public string wine_version
        {
            get { return _wine_version ?? ""; }
            set { _wine_version = value; }
        }
        

        public bool wine_versionSpecified
        {
            get { return _wine_version != null; }
            set { if (value == (_wine_version == null)) _wine_version = value ? this.wine_version : (string)null; }
        }
        private bool ShouldSerializewine_version() { return wine_versionSpecified; }
        private void Resetwine_version() { wine_versionSpecified = false; }


        private uint? _ping_ms_from_cell_search;
        [ProtoMember(10, IsRequired = false, Name = @"ping_ms_from_cell_search", DataFormat = DataFormat.TwosComplement)]
        public uint ping_ms_from_cell_search
        {
            get { return _ping_ms_from_cell_search ?? default(uint); }
            set { _ping_ms_from_cell_search = value; }
        }
        

        public bool ping_ms_from_cell_searchSpecified
        {
            get { return _ping_ms_from_cell_search != null; }
            set { if (value == (_ping_ms_from_cell_search == null)) _ping_ms_from_cell_search = value ? this.ping_ms_from_cell_search : (uint?)null; }
        }
        private bool ShouldSerializeping_ms_from_cell_search() { return ping_ms_from_cell_searchSpecified; }
        private void Resetping_ms_from_cell_search() { ping_ms_from_cell_searchSpecified = false; }


        private uint? _public_ip;
        [ProtoMember(20, IsRequired = false, Name = @"public_ip", DataFormat = DataFormat.TwosComplement)]
        public uint public_ip
        {
            get { return _public_ip ?? default(uint); }
            set { _public_ip = value; }
        }
        

        public bool public_ipSpecified
        {
            get { return _public_ip != null; }
            set { if (value == (_public_ip == null)) _public_ip = value ? this.public_ip : (uint?)null; }
        }
        private bool ShouldSerializepublic_ip() { return public_ipSpecified; }
        private void Resetpublic_ip() { public_ipSpecified = false; }


        private uint? _qos_level;
        [ProtoMember(21, IsRequired = false, Name = @"qos_level", DataFormat = DataFormat.TwosComplement)]
        public uint qos_level
        {
            get { return _qos_level ?? default(uint); }
            set { _qos_level = value; }
        }
        

        public bool qos_levelSpecified
        {
            get { return _qos_level != null; }
            set { if (value == (_qos_level == null)) _qos_level = value ? this.qos_level : (uint?)null; }
        }
        private bool ShouldSerializeqos_level() { return qos_levelSpecified; }
        private void Resetqos_level() { qos_levelSpecified = false; }


        private ulong? _client_supplied_steam_id;
        [ProtoMember(22, IsRequired = false, Name = @"client_supplied_steam_id", DataFormat = DataFormat.FixedSize)]
        public ulong client_supplied_steam_id
        {
            get { return _client_supplied_steam_id ?? default(ulong); }
            set { _client_supplied_steam_id = value; }
        }
        

        public bool client_supplied_steam_idSpecified
        {
            get { return _client_supplied_steam_id != null; }
            set { if (value == (_client_supplied_steam_id == null)) _client_supplied_steam_id = value ? this.client_supplied_steam_id : (ulong?)null; }
        }
        private bool ShouldSerializeclient_supplied_steam_id() { return client_supplied_steam_idSpecified; }
        private void Resetclient_supplied_steam_id() { client_supplied_steam_idSpecified = false; }


        private byte[] _machine_id;
        [ProtoMember(30, IsRequired = false, Name = @"machine_id", DataFormat = DataFormat.Default)]
        public byte[] machine_id
        {
            get { return _machine_id ?? null; }
            set { _machine_id = value; }
        }
        

        public bool machine_idSpecified
        {
            get { return _machine_id != null; }
            set { if (value == (_machine_id == null)) _machine_id = value ? this.machine_id : (byte[])null; }
        }
        private bool ShouldSerializemachine_id() { return machine_idSpecified; }
        private void Resetmachine_id() { machine_idSpecified = false; }


        private uint? _launcher_type;
        [ProtoMember(31, IsRequired = false, Name = @"launcher_type", DataFormat = DataFormat.TwosComplement)]
        public uint launcher_type
        {
            get { return _launcher_type ?? (uint)0; }
            set { _launcher_type = value; }
        }
        

        public bool launcher_typeSpecified
        {
            get { return _launcher_type != null; }
            set { if (value == (_launcher_type == null)) _launcher_type = value ? this.launcher_type : (uint?)null; }
        }
        private bool ShouldSerializelauncher_type() { return launcher_typeSpecified; }
        private void Resetlauncher_type() { launcher_typeSpecified = false; }


        private uint? _ui_mode;
        [ProtoMember(32, IsRequired = false, Name = @"ui_mode", DataFormat = DataFormat.TwosComplement)]
        public uint ui_mode
        {
            get { return _ui_mode ?? (uint)0; }
            set { _ui_mode = value; }
        }
        

        public bool ui_modeSpecified
        {
            get { return _ui_mode != null; }
            set { if (value == (_ui_mode == null)) _ui_mode = value ? this.ui_mode : (uint?)null; }
        }
        private bool ShouldSerializeui_mode() { return ui_modeSpecified; }
        private void Resetui_mode() { ui_modeSpecified = false; }


        private byte[] _steam2_auth_ticket;
        [ProtoMember(41, IsRequired = false, Name = @"steam2_auth_ticket", DataFormat = DataFormat.Default)]
        public byte[] steam2_auth_ticket
        {
            get { return _steam2_auth_ticket ?? null; }
            set { _steam2_auth_ticket = value; }
        }
        

        public bool steam2_auth_ticketSpecified
        {
            get { return _steam2_auth_ticket != null; }
            set { if (value == (_steam2_auth_ticket == null)) _steam2_auth_ticket = value ? this.steam2_auth_ticket : (byte[])null; }
        }
        private bool ShouldSerializesteam2_auth_ticket() { return steam2_auth_ticketSpecified; }
        private void Resetsteam2_auth_ticket() { steam2_auth_ticketSpecified = false; }


        private string _email_address;
        [ProtoMember(42, IsRequired = false, Name = @"email_address", DataFormat = DataFormat.Default)]
        public string email_address
        {
            get { return _email_address ?? ""; }
            set { _email_address = value; }
        }
        

        public bool email_addressSpecified
        {
            get { return _email_address != null; }
            set { if (value == (_email_address == null)) _email_address = value ? this.email_address : (string)null; }
        }
        private bool ShouldSerializeemail_address() { return email_addressSpecified; }
        private void Resetemail_address() { email_addressSpecified = false; }


        private uint? _rtime32_account_creation;
        [ProtoMember(43, IsRequired = false, Name = @"rtime32_account_creation", DataFormat = DataFormat.FixedSize)]
        public uint rtime32_account_creation
        {
            get { return _rtime32_account_creation ?? default(uint); }
            set { _rtime32_account_creation = value; }
        }
        

        public bool rtime32_account_creationSpecified
        {
            get { return _rtime32_account_creation != null; }
            set { if (value == (_rtime32_account_creation == null)) _rtime32_account_creation = value ? this.rtime32_account_creation : (uint?)null; }
        }
        private bool ShouldSerializertime32_account_creation() { return rtime32_account_creationSpecified; }
        private void Resetrtime32_account_creation() { rtime32_account_creationSpecified = false; }


        private string _account_name;
        [ProtoMember(50, IsRequired = false, Name = @"account_name", DataFormat = DataFormat.Default)]
        public string account_name
        {
            get { return _account_name ?? ""; }
            set { _account_name = value; }
        }
        

        public bool account_nameSpecified
        {
            get { return _account_name != null; }
            set { if (value == (_account_name == null)) _account_name = value ? this.account_name : (string)null; }
        }
        private bool ShouldSerializeaccount_name() { return account_nameSpecified; }
        private void Resetaccount_name() { account_nameSpecified = false; }


        private string _password;
        [ProtoMember(51, IsRequired = false, Name = @"password", DataFormat = DataFormat.Default)]
        public string password
        {
            get { return _password ?? ""; }
            set { _password = value; }
        }
        

        public bool passwordSpecified
        {
            get { return _password != null; }
            set { if (value == (_password == null)) _password = value ? this.password : (string)null; }
        }
        private bool ShouldSerializepassword() { return passwordSpecified; }
        private void Resetpassword() { passwordSpecified = false; }


        private string _game_server_token;
        [ProtoMember(52, IsRequired = false, Name = @"game_server_token", DataFormat = DataFormat.Default)]
        public string game_server_token
        {
            get { return _game_server_token ?? ""; }
            set { _game_server_token = value; }
        }
        

        public bool game_server_tokenSpecified
        {
            get { return _game_server_token != null; }
            set { if (value == (_game_server_token == null)) _game_server_token = value ? this.game_server_token : (string)null; }
        }
        private bool ShouldSerializegame_server_token() { return game_server_tokenSpecified; }
        private void Resetgame_server_token() { game_server_tokenSpecified = false; }


        private string _login_key;
        [ProtoMember(60, IsRequired = false, Name = @"login_key", DataFormat = DataFormat.Default)]
        public string login_key
        {
            get { return _login_key ?? ""; }
            set { _login_key = value; }
        }
        

        public bool login_keySpecified
        {
            get { return _login_key != null; }
            set { if (value == (_login_key == null)) _login_key = value ? this.login_key : (string)null; }
        }
        private bool ShouldSerializelogin_key() { return login_keySpecified; }
        private void Resetlogin_key() { login_keySpecified = false; }


        private bool? _was_converted_deprecated_msg;
        [ProtoMember(70, IsRequired = false, Name = @"was_converted_deprecated_msg", DataFormat = DataFormat.Default)]
        public bool was_converted_deprecated_msg
        {
            get { return _was_converted_deprecated_msg ?? (bool)false; }
            set { _was_converted_deprecated_msg = value; }
        }
        

        public bool was_converted_deprecated_msgSpecified
        {
            get { return _was_converted_deprecated_msg != null; }
            set { if (value == (_was_converted_deprecated_msg == null)) _was_converted_deprecated_msg = value ? this.was_converted_deprecated_msg : (bool?)null; }
        }
        private bool ShouldSerializewas_converted_deprecated_msg() { return was_converted_deprecated_msgSpecified; }
        private void Resetwas_converted_deprecated_msg() { was_converted_deprecated_msgSpecified = false; }


        private string _anon_user_target_account_name;
        [ProtoMember(80, IsRequired = false, Name = @"anon_user_target_account_name", DataFormat = DataFormat.Default)]
        public string anon_user_target_account_name
        {
            get { return _anon_user_target_account_name ?? ""; }
            set { _anon_user_target_account_name = value; }
        }
        

        public bool anon_user_target_account_nameSpecified
        {
            get { return _anon_user_target_account_name != null; }
            set { if (value == (_anon_user_target_account_name == null)) _anon_user_target_account_name = value ? this.anon_user_target_account_name : (string)null; }
        }
        private bool ShouldSerializeanon_user_target_account_name() { return anon_user_target_account_nameSpecified; }
        private void Resetanon_user_target_account_name() { anon_user_target_account_nameSpecified = false; }


        private ulong? _resolved_user_steam_id;
        [ProtoMember(81, IsRequired = false, Name = @"resolved_user_steam_id", DataFormat = DataFormat.FixedSize)]
        public ulong resolved_user_steam_id
        {
            get { return _resolved_user_steam_id ?? default(ulong); }
            set { _resolved_user_steam_id = value; }
        }
        

        public bool resolved_user_steam_idSpecified
        {
            get { return _resolved_user_steam_id != null; }
            set { if (value == (_resolved_user_steam_id == null)) _resolved_user_steam_id = value ? this.resolved_user_steam_id : (ulong?)null; }
        }
        private bool ShouldSerializeresolved_user_steam_id() { return resolved_user_steam_idSpecified; }
        private void Resetresolved_user_steam_id() { resolved_user_steam_idSpecified = false; }


        private int? _eresult_sentryfile;
        [ProtoMember(82, IsRequired = false, Name = @"eresult_sentryfile", DataFormat = DataFormat.TwosComplement)]
        public int eresult_sentryfile
        {
            get { return _eresult_sentryfile ?? default(int); }
            set { _eresult_sentryfile = value; }
        }
        

        public bool eresult_sentryfileSpecified
        {
            get { return _eresult_sentryfile != null; }
            set { if (value == (_eresult_sentryfile == null)) _eresult_sentryfile = value ? this.eresult_sentryfile : (int?)null; }
        }
        private bool ShouldSerializeeresult_sentryfile() { return eresult_sentryfileSpecified; }
        private void Reseteresult_sentryfile() { eresult_sentryfileSpecified = false; }


        private byte[] _sha_sentryfile;
        [ProtoMember(83, IsRequired = false, Name = @"sha_sentryfile", DataFormat = DataFormat.Default)]
        public byte[] sha_sentryfile
        {
            get { return _sha_sentryfile ?? null; }
            set { _sha_sentryfile = value; }
        }
        

        public bool sha_sentryfileSpecified
        {
            get { return _sha_sentryfile != null; }
            set { if (value == (_sha_sentryfile == null)) _sha_sentryfile = value ? this.sha_sentryfile : (byte[])null; }
        }
        private bool ShouldSerializesha_sentryfile() { return sha_sentryfileSpecified; }
        private void Resetsha_sentryfile() { sha_sentryfileSpecified = false; }


        private string _auth_code;
        [ProtoMember(84, IsRequired = false, Name = @"auth_code", DataFormat = DataFormat.Default)]
        public string auth_code
        {
            get { return _auth_code ?? ""; }
            set { _auth_code = value; }
        }
        

        public bool auth_codeSpecified
        {
            get { return _auth_code != null; }
            set { if (value == (_auth_code == null)) _auth_code = value ? this.auth_code : (string)null; }
        }
        private bool ShouldSerializeauth_code() { return auth_codeSpecified; }
        private void Resetauth_code() { auth_codeSpecified = false; }


        private int? _otp_type;
        [ProtoMember(85, IsRequired = false, Name = @"otp_type", DataFormat = DataFormat.TwosComplement)]
        public int otp_type
        {
            get { return _otp_type ?? default(int); }
            set { _otp_type = value; }
        }
        

        public bool otp_typeSpecified
        {
            get { return _otp_type != null; }
            set { if (value == (_otp_type == null)) _otp_type = value ? this.otp_type : (int?)null; }
        }
        private bool ShouldSerializeotp_type() { return otp_typeSpecified; }
        private void Resetotp_type() { otp_typeSpecified = false; }


        private uint? _otp_value;
        [ProtoMember(86, IsRequired = false, Name = @"otp_value", DataFormat = DataFormat.TwosComplement)]
        public uint otp_value
        {
            get { return _otp_value ?? default(uint); }
            set { _otp_value = value; }
        }
        

        public bool otp_valueSpecified
        {
            get { return _otp_value != null; }
            set { if (value == (_otp_value == null)) _otp_value = value ? this.otp_value : (uint?)null; }
        }
        private bool ShouldSerializeotp_value() { return otp_valueSpecified; }
        private void Resetotp_value() { otp_valueSpecified = false; }


        private string _otp_identifier;
        [ProtoMember(87, IsRequired = false, Name = @"otp_identifier", DataFormat = DataFormat.Default)]
        public string otp_identifier
        {
            get { return _otp_identifier ?? ""; }
            set { _otp_identifier = value; }
        }
        

        public bool otp_identifierSpecified
        {
            get { return _otp_identifier != null; }
            set { if (value == (_otp_identifier == null)) _otp_identifier = value ? this.otp_identifier : (string)null; }
        }
        private bool ShouldSerializeotp_identifier() { return otp_identifierSpecified; }
        private void Resetotp_identifier() { otp_identifierSpecified = false; }


        private bool? _steam2_ticket_request;
        [ProtoMember(88, IsRequired = false, Name = @"steam2_ticket_request", DataFormat = DataFormat.Default)]
        public bool steam2_ticket_request
        {
            get { return _steam2_ticket_request ?? default(bool); }
            set { _steam2_ticket_request = value; }
        }
        

        public bool steam2_ticket_requestSpecified
        {
            get { return _steam2_ticket_request != null; }
            set { if (value == (_steam2_ticket_request == null)) _steam2_ticket_request = value ? this.steam2_ticket_request : (bool?)null; }
        }
        private bool ShouldSerializesteam2_ticket_request() { return steam2_ticket_requestSpecified; }
        private void Resetsteam2_ticket_request() { steam2_ticket_requestSpecified = false; }


        private byte[] _sony_psn_ticket;
        [ProtoMember(90, IsRequired = false, Name = @"sony_psn_ticket", DataFormat = DataFormat.Default)]
        public byte[] sony_psn_ticket
        {
            get { return _sony_psn_ticket ?? null; }
            set { _sony_psn_ticket = value; }
        }
        

        public bool sony_psn_ticketSpecified
        {
            get { return _sony_psn_ticket != null; }
            set { if (value == (_sony_psn_ticket == null)) _sony_psn_ticket = value ? this.sony_psn_ticket : (byte[])null; }
        }
        private bool ShouldSerializesony_psn_ticket() { return sony_psn_ticketSpecified; }
        private void Resetsony_psn_ticket() { sony_psn_ticketSpecified = false; }


        private string _sony_psn_service_id;
        [ProtoMember(91, IsRequired = false, Name = @"sony_psn_service_id", DataFormat = DataFormat.Default)]
        public string sony_psn_service_id
        {
            get { return _sony_psn_service_id ?? ""; }
            set { _sony_psn_service_id = value; }
        }
        

        public bool sony_psn_service_idSpecified
        {
            get { return _sony_psn_service_id != null; }
            set { if (value == (_sony_psn_service_id == null)) _sony_psn_service_id = value ? this.sony_psn_service_id : (string)null; }
        }
        private bool ShouldSerializesony_psn_service_id() { return sony_psn_service_idSpecified; }
        private void Resetsony_psn_service_id() { sony_psn_service_idSpecified = false; }


        private bool? _create_new_psn_linked_account_if_needed;
        [ProtoMember(92, IsRequired = false, Name = @"create_new_psn_linked_account_if_needed", DataFormat = DataFormat.Default)]
        public bool create_new_psn_linked_account_if_needed
        {
            get { return _create_new_psn_linked_account_if_needed ?? (bool)false; }
            set { _create_new_psn_linked_account_if_needed = value; }
        }
        

        public bool create_new_psn_linked_account_if_neededSpecified
        {
            get { return _create_new_psn_linked_account_if_needed != null; }
            set { if (value == (_create_new_psn_linked_account_if_needed == null)) _create_new_psn_linked_account_if_needed = value ? this.create_new_psn_linked_account_if_needed : (bool?)null; }
        }
        private bool ShouldSerializecreate_new_psn_linked_account_if_needed() { return create_new_psn_linked_account_if_neededSpecified; }
        private void Resetcreate_new_psn_linked_account_if_needed() { create_new_psn_linked_account_if_neededSpecified = false; }


        private string _sony_psn_name;
        [ProtoMember(93, IsRequired = false, Name = @"sony_psn_name", DataFormat = DataFormat.Default)]
        public string sony_psn_name
        {
            get { return _sony_psn_name ?? ""; }
            set { _sony_psn_name = value; }
        }
        

        public bool sony_psn_nameSpecified
        {
            get { return _sony_psn_name != null; }
            set { if (value == (_sony_psn_name == null)) _sony_psn_name = value ? this.sony_psn_name : (string)null; }
        }
        private bool ShouldSerializesony_psn_name() { return sony_psn_nameSpecified; }
        private void Resetsony_psn_name() { sony_psn_nameSpecified = false; }


        private int? _game_server_app_id;
        [ProtoMember(94, IsRequired = false, Name = @"game_server_app_id", DataFormat = DataFormat.TwosComplement)]
        public int game_server_app_id
        {
            get { return _game_server_app_id ?? default(int); }
            set { _game_server_app_id = value; }
        }
        

        public bool game_server_app_idSpecified
        {
            get { return _game_server_app_id != null; }
            set { if (value == (_game_server_app_id == null)) _game_server_app_id = value ? this.game_server_app_id : (int?)null; }
        }
        private bool ShouldSerializegame_server_app_id() { return game_server_app_idSpecified; }
        private void Resetgame_server_app_id() { game_server_app_idSpecified = false; }


        private bool? _steamguard_dont_remember_computer;
        [ProtoMember(95, IsRequired = false, Name = @"steamguard_dont_remember_computer", DataFormat = DataFormat.Default)]
        public bool steamguard_dont_remember_computer
        {
            get { return _steamguard_dont_remember_computer ?? default(bool); }
            set { _steamguard_dont_remember_computer = value; }
        }
        

        public bool steamguard_dont_remember_computerSpecified
        {
            get { return _steamguard_dont_remember_computer != null; }
            set { if (value == (_steamguard_dont_remember_computer == null)) _steamguard_dont_remember_computer = value ? this.steamguard_dont_remember_computer : (bool?)null; }
        }
        private bool ShouldSerializesteamguard_dont_remember_computer() { return steamguard_dont_remember_computerSpecified; }
        private void Resetsteamguard_dont_remember_computer() { steamguard_dont_remember_computerSpecified = false; }


        private string _machine_name;
        [ProtoMember(96, IsRequired = false, Name = @"machine_name", DataFormat = DataFormat.Default)]
        public string machine_name
        {
            get { return _machine_name ?? ""; }
            set { _machine_name = value; }
        }
        

        public bool machine_nameSpecified
        {
            get { return _machine_name != null; }
            set { if (value == (_machine_name == null)) _machine_name = value ? this.machine_name : (string)null; }
        }
        private bool ShouldSerializemachine_name() { return machine_nameSpecified; }
        private void Resetmachine_name() { machine_nameSpecified = false; }


        private string _machine_name_userchosen;
        [ProtoMember(97, IsRequired = false, Name = @"machine_name_userchosen", DataFormat = DataFormat.Default)]
        public string machine_name_userchosen
        {
            get { return _machine_name_userchosen ?? ""; }
            set { _machine_name_userchosen = value; }
        }
        

        public bool machine_name_userchosenSpecified
        {
            get { return _machine_name_userchosen != null; }
            set { if (value == (_machine_name_userchosen == null)) _machine_name_userchosen = value ? this.machine_name_userchosen : (string)null; }
        }
        private bool ShouldSerializemachine_name_userchosen() { return machine_name_userchosenSpecified; }
        private void Resetmachine_name_userchosen() { machine_name_userchosenSpecified = false; }


        private string _country_override;
        [ProtoMember(98, IsRequired = false, Name = @"country_override", DataFormat = DataFormat.Default)]
        public string country_override
        {
            get { return _country_override ?? ""; }
            set { _country_override = value; }
        }
        

        public bool country_overrideSpecified
        {
            get { return _country_override != null; }
            set { if (value == (_country_override == null)) _country_override = value ? this.country_override : (string)null; }
        }
        private bool ShouldSerializecountry_override() { return country_overrideSpecified; }
        private void Resetcountry_override() { country_overrideSpecified = false; }


        private bool? _is_steam_box;
        [ProtoMember(99, IsRequired = false, Name = @"is_steam_box", DataFormat = DataFormat.Default)]
        public bool is_steam_box
        {
            get { return _is_steam_box ?? default(bool); }
            set { _is_steam_box = value; }
        }
        

        public bool is_steam_boxSpecified
        {
            get { return _is_steam_box != null; }
            set { if (value == (_is_steam_box == null)) _is_steam_box = value ? this.is_steam_box : (bool?)null; }
        }
        private bool ShouldSerializeis_steam_box() { return is_steam_boxSpecified; }
        private void Resetis_steam_box() { is_steam_boxSpecified = false; }


        private ulong? _client_instance_id;
        [ProtoMember(100, IsRequired = false, Name = @"client_instance_id", DataFormat = DataFormat.TwosComplement)]
        public ulong client_instance_id
        {
            get { return _client_instance_id ?? default(ulong); }
            set { _client_instance_id = value; }
        }
        

        public bool client_instance_idSpecified
        {
            get { return _client_instance_id != null; }
            set { if (value == (_client_instance_id == null)) _client_instance_id = value ? this.client_instance_id : (ulong?)null; }
        }
        private bool ShouldSerializeclient_instance_id() { return client_instance_idSpecified; }
        private void Resetclient_instance_id() { client_instance_idSpecified = false; }


        private string _two_factor_code;
        [ProtoMember(101, IsRequired = false, Name = @"two_factor_code", DataFormat = DataFormat.Default)]
        public string two_factor_code
        {
            get { return _two_factor_code ?? ""; }
            set { _two_factor_code = value; }
        }
        

        public bool two_factor_codeSpecified
        {
            get { return _two_factor_code != null; }
            set { if (value == (_two_factor_code == null)) _two_factor_code = value ? this.two_factor_code : (string)null; }
        }
        private bool ShouldSerializetwo_factor_code() { return two_factor_codeSpecified; }
        private void Resettwo_factor_code() { two_factor_codeSpecified = false; }


        private bool? _supports_rate_limit_response;
        [ProtoMember(102, IsRequired = false, Name = @"supports_rate_limit_response", DataFormat = DataFormat.Default)]
        public bool supports_rate_limit_response
        {
            get { return _supports_rate_limit_response ?? default(bool); }
            set { _supports_rate_limit_response = value; }
        }
        

        public bool supports_rate_limit_responseSpecified
        {
            get { return _supports_rate_limit_response != null; }
            set { if (value == (_supports_rate_limit_response == null)) _supports_rate_limit_response = value ? this.supports_rate_limit_response : (bool?)null; }
        }
        private bool ShouldSerializesupports_rate_limit_response() { return supports_rate_limit_responseSpecified; }
        private void Resetsupports_rate_limit_response() { supports_rate_limit_responseSpecified = false; }

        private global::ProtoBuf.IExtension extensionObject;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
    }
}
