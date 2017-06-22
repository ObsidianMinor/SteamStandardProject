using Steam.Common;
using System;
using System.Collections.Generic;

namespace Steam.Local.Apps
{
    public class LocalApp
    {
        public uint Id { get; set; }
        public Universe Universe { get; set; }
        public string Name { get; set; }
        public AppState StateFlags { get; set; }
        public string InstallFolder { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public AppUpdateResult UpdateResult { get; set; }
        public ulong SizeOnDisk { get; set; }
        public int BuildID { get; set; }
        public SteamId LastOwner { get; set; }
        public ulong BytesToDownload { get; set; }
        public ulong BytesDownloaded { get; set; }
        public byte AutoUpdateBehavior { get; set; }
        public byte AllowOtherDownloadsWhileRunning { get; set; }
        public bool FullValidateBeforeNextUpdate { get; set; }
        public bool FullValidateAfterNextUpdate { get; set; }
        public Dictionary<string, string> UserConfig { get; set; }
        public Dictionary<uint, ulong> MountedDepots { get; set; }
        public List<string> CheckGuid { get; set; }
        public Dictionary<uint, uint> SharedDepots { get; set; }
        public List<string> InstallScripts { get; set; }
        public List<Dlc> DlcDownloads { get; set; }
        public List<StagedDepot> StagedDepots { get; set; }

        public static LocalApp CreateFromFile(string path)
        {
            throw new NotImplementedException();
        }
    }
}
