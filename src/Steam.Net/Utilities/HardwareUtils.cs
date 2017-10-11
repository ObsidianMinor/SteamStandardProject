using System.Threading.Tasks;

namespace Steam.Net.Utilities
{
    internal static class HardwareUtils
    {
        private static OsType _processOsType = OsType.Unknown;

        internal static Task<byte[]> GetMachineId()
        {
            return Task.FromResult<byte[]>(new byte[0]); // todo, implement this
        }

        internal static OsType GetCurrentOsType()
        {
            if (_processOsType != OsType.Unknown)
            {
                // todo: implement
            }

            return _processOsType;
        }
    }
}
