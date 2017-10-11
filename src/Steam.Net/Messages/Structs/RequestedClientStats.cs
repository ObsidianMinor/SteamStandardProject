using System.Runtime.InteropServices;

namespace Steam.Net.Messages.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RequestedClientStats
    {
        internal int Count;
    }
}
