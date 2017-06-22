using System;

namespace Steam.Net
{
    [Flags]
    public enum LicenseFlags
    {
        None = 0,
        Renew = 0x01,
        RenewalFailed = 0x02,
        Pending = 0x04,
        Expired = 0x08,
        CancelledByUser = 0x10,
        CancelledByAdmin = 0x20,
        LowViolenceContent = 0x40,
        ImportedFromSteam2 = 0x80,
        ForceRunRestriction = 0x100,
        RegionRestrictionExpired = 0x200,
        CancelledByFriendlyFraudLock = 0x400,
        NotActivated = 0x800,
    }
}
