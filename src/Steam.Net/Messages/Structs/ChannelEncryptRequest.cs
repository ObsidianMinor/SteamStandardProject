using System.Runtime.InteropServices;

namespace Steam.Net.Messages.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ChannelEncryptRequest
    {
        internal uint ProtocolVersion;
        internal Universe Universe;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        internal byte[] Challenge;
    }
}
