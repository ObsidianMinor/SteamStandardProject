using Steam.KeyValues;

namespace Steam.Local.Apps
{
    public class Dlc
    {
        [KeyValueProperty]
        public ulong BytesDownloaded { get; set; }
        [KeyValueProperty]
        public ulong BytesToDownload { get; set; }
    }
}
