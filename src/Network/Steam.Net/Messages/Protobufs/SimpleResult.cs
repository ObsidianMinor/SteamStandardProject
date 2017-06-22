using ProtoBuf;

namespace Steam.Net.Messages.Protobufs
{
    [ProtoContract]
    internal class SimpleResult
    {
        private uint? _result;
        [ProtoMember(1, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        public uint Result
        {
            get => _result ?? 0;
            set => _result = value;
        }

        public bool ResultSpecified
        {
            get => _result != null;
            set
            {
                if (value == !ResultSpecified)
                    _result = value ? Result : (uint?)null;
            }
        }

        private bool ShouldSerializeResult() => ResultSpecified;
        private void ResetResult() => ResultSpecified = false;
    }
}
