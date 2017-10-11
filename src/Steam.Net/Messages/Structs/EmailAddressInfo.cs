using System.Runtime.InteropServices;

namespace Steam.Net.Messages.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct EmailAddressInfo
    {
        internal uint PasswordStrength;
        internal uint FlagsAccountSecurityPolicy;
        [MarshalAs(UnmanagedType.U1)]
        internal bool Validated;
    }
}
