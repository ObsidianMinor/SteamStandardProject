using Steam.KeyValues;

namespace Steam.Local.Models
{
    internal class InstalledWorkshopItemDetails
    {
        [KeyValueProperty("manifest")]
        internal long Manifest { get; set; }
        [KeyValueProperty("timeupdated")]
        internal long TimeUpdated { get; set; }
        [KeyValueProperty("timetouched")]
        internal long TimeTouched { get; set; }
        [KeyValueProperty("subscribedby")]
        internal uint SubscribedBy { get; set; }
    }
}
