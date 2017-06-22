using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract]
    internal class PicsChangesSinceRequest : IExtensible
    {
        private uint? _sinceChangeNumber;
        [ProtoMember(1, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        public uint SinceChangeNumber
        {
            get => _sinceChangeNumber ?? default(uint);
            set => _sinceChangeNumber = value;
        }

        public bool SinceChangeNumberSpecified
        {
            get => _sinceChangeNumber != null;
            set
            {
                if (value == !SinceChangeNumberSpecified)
                    _sinceChangeNumber = value ? SinceChangeNumber : (uint?)null;
            }
        }

        private bool ShouldSerializeSinceChangeNumber() => SinceChangeNumberSpecified;
        private void ResetSinceChangeNumber() => SinceChangeNumberSpecified = false;
        
        private bool? _sendAppInfoChanges;
        [ProtoMember(2, IsRequired = false)]
        public bool SendAppInfoChanges
        {
            get { return _sendAppInfoChanges ?? default(bool); }
            set { _sendAppInfoChanges = value; }
        }

        public bool SendAppInfoChangesSpecified
        {
            get { return _sendAppInfoChanges != null; }
            set { if (value == (_sendAppInfoChanges == null)) _sendAppInfoChanges = value ? SendAppInfoChanges : (bool?)null; }
        }

        private bool ShouldSerializeSendAppInfoChanges() { return SendAppInfoChangesSpecified; }
        private void ResetSendAppInfoChanges() { SendAppInfoChangesSpecified = false; }


        private bool? _sendPackageInfoChanges;
        [ProtoMember(3, IsRequired = false)]
        public bool SendPackageInfoChanges
        {
            get { return _sendPackageInfoChanges ?? default(bool); }
            set { _sendPackageInfoChanges = value; }
        }

        public bool SendPackageInfoChangesSpecified
        {
            get { return _sendPackageInfoChanges != null; }
            set { if (value == (_sendPackageInfoChanges == null)) _sendPackageInfoChanges = value ? SendPackageInfoChanges : (bool?)null; }
        }
        private bool ShouldSerializeSendPackageInfoChangesSpecified() { return SendPackageInfoChangesSpecified; }
        private void ResetSendPackageInfoChangesSpecified() { SendPackageInfoChangesSpecified = false; }


        private uint? _numAppInfoCached;
        [ProtoMember(4, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        public uint NumAppInfoCached
        {
            get { return _numAppInfoCached ?? default(uint); }
            set { _numAppInfoCached = value; }
        }

        public bool NumAppInfoCachedSpecified
        {
            get { return _numAppInfoCached != null; }
            set { if (value == (_numAppInfoCached == null)) _numAppInfoCached = value ? NumAppInfoCached : (uint?)null; }
        }
        private bool ShouldSerializeNumAppInfoCached() { return NumAppInfoCachedSpecified; }
        private void ResetNumAppInfoCached() { NumAppInfoCachedSpecified = false; }


        private uint? _numPackageInfoCached;
        [ProtoMember(5, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        public uint NumPackageInfoCached
        {
            get { return _numPackageInfoCached ?? default(uint); }
            set { _numPackageInfoCached = value; }
        }

        public bool NumPackageInfoCachedSpecified
        {
            get { return _numPackageInfoCached != null; }
            set { if (value == (_numPackageInfoCached == null)) _numPackageInfoCached = value ? NumPackageInfoCached : (uint?)null; }
        }
        private bool ShouldSerializeNumPackageInfoCached() { return NumPackageInfoCachedSpecified; }
        private void ResetNumPackageInfoCached() { NumPackageInfoCachedSpecified = false; }

        private IExtension extensionObject;
        IExtension IExtensible.GetExtensionObject(bool createIfMissing)
        { return Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
    }
}
