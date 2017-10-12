using System.Runtime.InteropServices;

namespace Steam.Net.Messages.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ChannelEncryptResponse
    {
        public uint ProtocolVersion;
        public uint KeySize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] EncryptedHandshake;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] KeyHash;
        public uint Padding;
    }
}
