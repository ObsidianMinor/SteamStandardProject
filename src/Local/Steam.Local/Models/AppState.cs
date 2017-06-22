using Steam.KeyValues;
using Steam.Common;
using Steam.Local.Apps;
using System.Collections.Generic;

namespace Steam.Local.Models
{
    internal class AppState
    {
        [KeyValueProperty("appid")]
        public uint Id { get; set; }
        [KeyValueProperty("universe")]
        public Universe Universe { get; set; }
        [KeyValueProperty]
        public string Name { get; set; }
        [KeyValueProperty]
        public Apps.AppState StateFlags { get; set; }
        [KeyValueProperty("InstallDir")]
        public string InstallFolder { get; set; }
        [KeyValueProperty]
        public long LastUpdated { get; set; }
        [KeyValueProperty]
        public AppUpdateResult UpdateResult { get; set; }
        [KeyValueProperty]
        public ulong SizeOnDisk { get; set; }
        [KeyValueProperty("buildid")]
        public int BuildID { get; set; }
        [KeyValueProperty]
        public ulong LastOwner { get; set; }
        [KeyValueProperty]
        public ulong BytesToDownload { get; set; }
        [KeyValueProperty]
        public ulong BytesDownloaded { get; set; }
        [KeyValueProperty]
        public byte AutoUpdateBehavior { get; set; }
        [KeyValueProperty]
        public byte AllowOtherDownloadsWhileRunning { get; set; }
        [KeyValueProperty]
        public bool FullValidateBeforeNextUpdate { get; set; }
        [KeyValueProperty]
        public bool FullValidateAfterNextUpdate { get; set; }
        [KeyValueDictionary]
        public Dictionary<string, string> UserConfig { get; set; }
        [KeyValueDictionary]
        public Dictionary<uint, ulong> MountedDepots { get; set; }
        [KeyValueList]
        public List<string> CheckGuid { get; set; }
        [KeyValueDictionary]
        public Dictionary<uint, uint> SharedDepots { get; set; }
        [KeyValueList]
        public List<string> InstallScripts { get; set; }
        [KeyValueList]
        public List<Dlc> DlcDownloads { get; set; }
        [KeyValueList]
        public List<StagedDepot> StagedDepots { get; set; }
    }
}
