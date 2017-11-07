namespace Steam.Net
{
    /// <summary>
    /// Represents the current user's Steam wallet
    /// </summary>
    public class Wallet
    {   
        private long _cents;
        private long _centsPending;

        /// <summary>
        /// Gets the currency of this wallet
        /// </summary>
        public CurrencyCode Currency { get; }

        /// <summary>
        /// Get the cents in this wallet that can be used
        /// </summary>
        public long Cents => _cents;

        /// <summary>
        /// Get the cents in this wallet that are pending and cannot be used yet
        /// </summary>
        public long CentsPending => _centsPending;

        private Wallet(CurrencyCode code, long cents, long pending)
        {
            Currency = code;
            _cents = cents;
            _centsPending = pending;
        }

        internal static Wallet Create(CurrencyCode code, long cents, long pending)
        {
            return new Wallet(code, cents, pending);
        }
    }
}
