using Steam.KeyValues;

namespace Steam.Local.Models
{
    internal class InstalledWorkshopItem
    {
        [KeyValueProperty("manifest")]
        internal long Manifest { get; set; }
        [KeyValueProperty("size")]
        internal long Size { get; set; }
        [KeyValueProperty("timeupdated")]
        internal long TimeUpdated { get; set; }
    }
}
