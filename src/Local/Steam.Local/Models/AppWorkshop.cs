using Steam.KeyValues;
using System.Collections.Generic;

namespace Steam.Local.Models
{
    internal class AppWorkshop
    {
        [KeyValueProperty("appid")]
        internal uint AppId { get; set; }
        [KeyValueProperty("SizeOnDisk")]
        internal ulong SizeOnDisk { get; set; }
        [KeyValueProperty("NeedsUpdate")]
        internal int NeedsUpdate { get; set; }
        [KeyValueProperty]
        internal int NeedsDownload { get; set; }
        [KeyValueProperty]
        internal long TimeLastUpdated { get; set; }
        [KeyValueProperty]
        internal Dictionary<ulong, InstalledWorkshopItem> WorkshopItemsInstalled { get; set; }
        [KeyValueProperty]
        internal Dictionary<ulong, InstalledWorkshopItemDetails> WorkshopItemDetails { get; set; }
    }
}
