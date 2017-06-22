using System;
using System.Globalization;

namespace Steam.Net
{
    /// <summary>
    /// Represents a license on Steam
    /// </summary>
    public class License
    {
        /// <summary>
        /// Gets the package Id this license applies to
        /// </summary>
        public uint PackageId { get; private set; }
        /// <summary>
        /// Gets the last change number this license was in
        /// </summary>
        public int LastChangeNumber { get; private set; }
        /// <summary>
        /// Gets the time this license was created
        /// </summary>
        public DateTimeOffset TimeCreated { get; private set; }
        /// <summary>
        /// Gets the next time the license will be processed
        /// </summary>
        public DateTimeOffset TimeNextProcess { get; private set; }
        /// <summary>
        /// Gets the time limit of this license if it has one
        /// </summary>
        public TimeSpan TimeLimit { get; private set; }
        /// <summary>
        /// Gets the time this license has been used
        /// </summary>
        public TimeSpan TimeUsed { get; private set; }
        /// <summary>
        /// Gets the payment method used to purchase this license
        /// </summary>
        public PaymentMethod PaymentMethod { get; private set; }
        /// <summary>
        /// Gets the country this license was purchased in
        /// </summary>
        public RegionInfo PurchaseCountry { get; private set; }
        /// <summary>
        /// Gets the license flags of this license
        /// </summary>
        public LicenseFlags Flags { get; private set; }
        /// <summary>
        /// Gets the type of this license
        /// </summary>
        public LicenseType Type { get; private set; }
        /// <summary>
        /// Gets the territory code this license applies in
        /// </summary>
        public int TerritoryCode { get; private set; }
    }
}
