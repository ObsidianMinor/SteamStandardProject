using System.Runtime.InteropServices;

namespace Steam.Net.Messages.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ChannelEncryptResponse
    {
        public uint ProtocolVersion;
        public uint KeySize;
        public byte[] Payload;
    }
}
