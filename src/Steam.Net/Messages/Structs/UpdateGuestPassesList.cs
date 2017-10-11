using System.Runtime.InteropServices;

namespace Steam.Net.Messages.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct UpdateGuestPassesList
    {
        internal Result Result;
        internal int GuestPassesToGive;
        internal int GuestPassesToRedeem;
    }
}
