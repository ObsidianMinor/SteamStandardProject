using System;
using System.Net;
using System.Threading.Tasks;

namespace Steam.Net.Utilities
{
    internal class HardwareUtils
    {
        internal static Task<byte[]> GetMachineId()
        {
            byte[] tempArray = new byte[] 
            {
                0, 77, 101, 115, 115, 97, 103, 101, 79, 98, 106, 101, 99, 116, 0, 1, 66, 66, 51, 0, 49, 50, 101, 52, 97, 50, 97, 51, 56, 49, 48, 50, 50, 99, 100, 99, 54, 55, 51, 99, 97, 55, 51, 55, 57, 52, 98,
                55, 98, 97, 53, 51, 98, 48, 102, 51, 101, 100, 49, 52, 0, 1, 70, 70, 50, 0, 55, 99, 97, 101, 50, 49, 56, 55, 48, 99, 57, 98, 54, 97, 57, 57, 48, 48, 100, 99, 54, 102, 49, 55, 50, 55, 57, 56, 56,
                50, 50, 99, 99, 57, 101, 98, 48, 52, 54, 101, 0, 1, 51, 66, 51, 0, 52, 98, 54, 99, 55, 56, 100, 98, 52, 48, 53, 48, 50, 98, 49, 48, 97, 99, 48, 51, 56, 99, 98, 56, 50, 101, 54, 102, 55, 54, 53,                54, 48, 54, 57, 53, 54, 52, 50, 51, 0, 8, 8            };
            return Task.FromResult(tempArray); // todo, implement this
        }

        internal static uint GetIpAddress(IPAddress ip)
        {
            byte[] bytes = ip.GetAddressBytes();
            Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }
    }
}
