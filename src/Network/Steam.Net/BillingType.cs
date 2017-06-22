namespace Steam.Net
{
    public enum BillingType
    {
        NoCost = 0,
        BillOnceOnly = 1,
        BillMonthly = 2,
        ProofOfPrepurchaseOnly = 3,
        GuestPass = 4,
        HardwarePromo = 5,
        Gift = 6,
        AutoGrant = 7,
        OEMTicket = 8,
        RecurringOption = 9,
        BillOnceOrCDKey = 10,
        Repurchaseable = 11,
        FreeOnDemand = 12,
        Rental = 13,
        NumBillingTypes = 14,
    }
}
