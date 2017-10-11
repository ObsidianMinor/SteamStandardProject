using ProtoBuf;
using System.Collections.Generic;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract]
    internal class PicsChangesSinceResponse : IExtensible
    {
        private uint? _currentChangeNumber;
        [ProtoMember(1, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        public uint CurrentChangeNumber
        {
            get { return _currentChangeNumber ?? default(uint); }
            set { _currentChangeNumber = value; }
        }

        public bool CurrentChangeNumberSpecified
        {
            get { return _currentChangeNumber != null; }
            set { if (value == (_currentChangeNumber == null)) _currentChangeNumber = value ? this.CurrentChangeNumber : (uint?)null; }
        }
        private bool ShouldSerializeCurrentChangeNumber() { return CurrentChangeNumberSpecified; }
        private void ResetCurrentChangeNumber() { CurrentChangeNumberSpecified = false; }


        private uint? _sinceChangeNumber;
        [ProtoMember(2, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        public uint SinceChangeNumber
        {
            get { return _sinceChangeNumber ?? default(uint); }
            set { _sinceChangeNumber = value; }
        }

        public bool SinceChangeNumberSpecified
        {
            get { return _sinceChangeNumber != null; }
            set { if (value == (_sinceChangeNumber == null)) _sinceChangeNumber = value ? this.SinceChangeNumber : (uint?)null; }
        }
        private bool ShouldSerializeSinceChangeNumber() { return SinceChangeNumberSpecified; }
        private void ResetSinceChangeNumber() { SinceChangeNumberSpecified = false; }


        private bool? _forceFullUpdate;
        [ProtoMember(3, IsRequired = false)]
        public bool ForceFullUpdate
        {
            get { return _forceFullUpdate ?? default(bool); }
            set { _forceFullUpdate = value; }
        }

        public bool ForceFullUpdateSpecified
        {
            get { return _forceFullUpdate != null; }
            set { if (value == (_forceFullUpdate == null)) _forceFullUpdate = value ? this.ForceFullUpdate : (bool?)null; }
        }
        private bool ShouldSerializeForceFullUpdate() { return ForceFullUpdateSpecified; }
        private void ResetForceFullUpdate() { ForceFullUpdateSpecified = false; }

        private readonly List<PackageChange> _package_changes = new List<PackageChange>();
        [ProtoMember(4)]
        public List<PackageChange> PackageChanges => _package_changes;

        private readonly List<AppChange> _app_changes = new List<AppChange>();
        [ProtoMember(5)]
        public List<AppChange> app_changes
        {
            get { return _app_changes; }
        }


        private bool? _force_full_app_update;
        [ProtoMember(6, IsRequired = false, Name = @"force_full_app_update", DataFormat = DataFormat.Default)]
        public bool force_full_app_update
        {
            get { return _force_full_app_update ?? default(bool); }
            set { _force_full_app_update = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool force_full_app_updateSpecified
        {
            get { return _force_full_app_update != null; }
            set { if (value == (_force_full_app_update == null)) _force_full_app_update = value ? this.force_full_app_update : (bool?)null; }
        }
        private bool ShouldSerializeforce_full_app_update() { return force_full_app_updateSpecified; }
        private void Resetforce_full_app_update() { force_full_app_updateSpecified = false; }


        private bool? _force_full_package_update;
        [ProtoMember(7, IsRequired = false, Name = @"force_full_package_update", DataFormat = global::ProtoBuf.DataFormat.Default)]
        public bool force_full_package_update
        {
            get { return _force_full_package_update ?? default(bool); }
            set { _force_full_package_update = value; }
        }
        [global::System.Xml.Serialization.XmlIgnore]

        public bool force_full_package_updateSpecified
        {
            get { return _force_full_package_update != null; }
            set { if (value == (_force_full_package_update == null)) _force_full_package_update = value ? this.force_full_package_update : (bool?)null; }
        }
        private bool ShouldSerializeforce_full_package_update() { return force_full_package_updateSpecified; }
        private void Resetforce_full_package_update() { force_full_package_updateSpecified = false; }

        [global::ProtoBuf.ProtoContract(Name = @"PackageChange")]
        public partial class PackageChange : IExtensible
        {
            public PackageChange() { }


            private uint? _packageid;
            [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name = @"packageid", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
            public uint packageid
            {
                get { return _packageid ?? default(uint); }
                set { _packageid = value; }
            }
            [global::System.Xml.Serialization.XmlIgnore]

            public bool packageidSpecified
            {
                get { return _packageid != null; }
                set { if (value == (_packageid == null)) _packageid = value ? this.packageid : (uint?)null; }
            }
            private bool ShouldSerializepackageid() { return packageidSpecified; }
            private void Resetpackageid() { packageidSpecified = false; }


            private uint? _change_number;
            [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name = @"change_number", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
            public uint change_number
            {
                get { return _change_number ?? default(uint); }
                set { _change_number = value; }
            }
            [global::System.Xml.Serialization.XmlIgnore]

            public bool change_numberSpecified
            {
                get { return _change_number != null; }
                set { if (value == (_change_number == null)) _change_number = value ? this.change_number : (uint?)null; }
            }
            private bool ShouldSerializechange_number() { return change_numberSpecified; }
            private void Resetchange_number() { change_numberSpecified = false; }


            private bool? _needs_token;
            [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name = @"needs_token", DataFormat = global::ProtoBuf.DataFormat.Default)]
            public bool needs_token
            {
                get { return _needs_token ?? default(bool); }
                set { _needs_token = value; }
            }
            [global::System.Xml.Serialization.XmlIgnore]

            public bool needs_tokenSpecified
            {
                get { return _needs_token != null; }
                set { if (value == (_needs_token == null)) _needs_token = value ? this.needs_token : (bool?)null; }
            }
            private bool ShouldSerializeneeds_token() { return needs_tokenSpecified; }
            private void Resetneeds_token() { needs_tokenSpecified = false; }

            private global::ProtoBuf.IExtension extensionObject;
            global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
        }

        [global::ProtoBuf.ProtoContract(Name = @"AppChange")]
        public partial class AppChange : global::ProtoBuf.IExtensible
        {
            public AppChange() { }


            private uint? _appid;
            [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name = @"appid", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
            public uint appid
            {
                get { return _appid ?? default(uint); }
                set { _appid = value; }
            }
            [global::System.Xml.Serialization.XmlIgnore]

            public bool appidSpecified
            {
                get { return _appid != null; }
                set { if (value == (_appid == null)) _appid = value ? this.appid : (uint?)null; }
            }
            private bool ShouldSerializeappid() { return appidSpecified; }
            private void Resetappid() { appidSpecified = false; }


            private uint? _change_number;
            [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name = @"change_number", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
            public uint change_number
            {
                get { return _change_number ?? default(uint); }
                set { _change_number = value; }
            }
            [global::System.Xml.Serialization.XmlIgnore]

            public bool change_numberSpecified
            {
                get { return _change_number != null; }
                set { if (value == (_change_number == null)) _change_number = value ? this.change_number : (uint?)null; }
            }
            private bool ShouldSerializechange_number() { return change_numberSpecified; }
            private void Resetchange_number() { change_numberSpecified = false; }


            private bool? _needs_token;
            [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name = @"needs_token", DataFormat = global::ProtoBuf.DataFormat.Default)]
            public bool needs_token
            {
                get { return _needs_token ?? default(bool); }
                set { _needs_token = value; }
            }
            [global::System.Xml.Serialization.XmlIgnore]

            public bool needs_tokenSpecified
            {
                get { return _needs_token != null; }
                set { if (value == (_needs_token == null)) _needs_token = value ? this.needs_token : (bool?)null; }
            }
            private bool ShouldSerializeneeds_token() { return needs_tokenSpecified; }
            private void Resetneeds_token() { needs_tokenSpecified = false; }

            private global::ProtoBuf.IExtension extensionObject;
            global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
        }

        private global::ProtoBuf.IExtension extensionObject;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
    }
}
