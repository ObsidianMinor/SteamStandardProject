namespace Steam.Net.Utilities
{
    internal static class MessageTypeUtils
    {
        const uint ProtobufMask = 0x80000000;
        const uint MessageTypeMask = ~ProtobufMask;

        internal static (MessageType, bool) SplitMessage(uint type)
        {
            return ((MessageType)(type & MessageTypeMask), (type & ProtobufMask) > 0);
        }

        internal static (uint, bool) SplitUInt32Message(uint type)
        {
            return ((type & MessageTypeMask), (type & ProtobufMask) > 0);
        }

        internal static uint MergeMessage(MessageType type, bool isProtobuf) => MergeMessage((uint) type, isProtobuf);

        internal static uint MergeMessage(uint type, bool isProtobuf)
        {
            if (isProtobuf)
                return type | ProtobufMask;

            return type;
        }
    }
}
