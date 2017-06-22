using Steam.KeyValues;

namespace Steam.Local.Models
{
    internal class User
    {
        [KeyValueProperty]
        internal string AccountName { get; set; }
        [KeyValueProperty]
        internal string PersonaName { get; set; }
        [KeyValueProperty]
        internal int RememberPassword { get; set; }
        [KeyValueProperty]
        internal long Timestamp { get; set; }
        [KeyValueProperty]
        internal int WantsOfflineMode { get; set; }
    }
}
