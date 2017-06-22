using System;
using System.Net;
using System.Threading.Tasks;

namespace Steam.Net.Utilities
{
    internal class HardwareUtils
    {
        internal static Task<byte[]> GetMachineId()
        {
            return Task.FromResult<byte[]>(null); // todo, implement this
        }

        internal static uint GetIpAddress(IPAddress ip)
        {
            byte[] bytes = ip.GetAddressBytes();
            Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }
    }
}
