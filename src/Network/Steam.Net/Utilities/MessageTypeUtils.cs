namespace Steam.Net.Utilities
{
    internal static class MessageTypeUtils
    {
        const uint ProtobufMask = 0x80000000;
        const uint MessageTypeMask = ~ProtobufMask;

        internal static (MessageType, bool) SplitMessage(this uint type)
        {
            return ((MessageType)(type & MessageTypeMask), (type & ProtobufMask) > 0);
        }

        internal static uint MergeMessage(MessageType type, bool isProtobuf)
        {
            if (isProtobuf)
                return (uint)type | ProtobufMask;

            return (uint)type;
        }
    }
}
