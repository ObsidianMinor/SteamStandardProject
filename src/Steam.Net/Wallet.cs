namespace Steam.Net
{
    /// <summary>
    /// Represents the current user's Steam wallet
    /// </summary>
    public sealed class Wallet
    {
        /// <summary>
        /// Gets the currency of this wallet
        /// </summary>
        public CurrencyCode Currency { get; internal set; }

        /// <summary>
        /// Get the cents in this wallet that can be used
        /// </summary>
        public long Cents { get; internal set; }

        /// <summary>
        /// Get the cents in this wallet that are pending and cannot be used yet
        /// </summary>
        public long CentsPending { get; internal set; }

        internal Wallet(CurrencyCode code, long cents, long pending)
        {
            Currency = code;
            Cents = cents;
            CentsPending = pending;
        }

        internal void Update(long cents, long pending)
        {
            Cents = cents;
            CentsPending = pending;
        }
    }
}
