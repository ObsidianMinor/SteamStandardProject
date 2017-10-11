using System;
using System.Net;

namespace Steam.Net.Utilities
{
    internal static class IPAddressUtils
    {
        internal static uint ToUInt32(this IPAddress address)
        {
            byte[] addressBytes = address.GetAddressBytes();
            Array.Reverse(addressBytes);

            return BitConverter.ToUInt32(addressBytes, 0);
        }

        internal static IPAddress ToIPAddress(this uint ip)
        {
            byte[] valueBytes = BitConverter.GetBytes(ip);
            Array.Reverse(valueBytes);

            return new IPAddress(valueBytes);
        }
    }
}
