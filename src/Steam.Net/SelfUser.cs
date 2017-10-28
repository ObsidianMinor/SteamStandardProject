using System.Diagnostics;

namespace Steam.Net
{
    /// <summary>
    /// Represents the current user logged into the Steam client
    /// </summary>
    [DebuggerDisplay("{AccountName ?? \"anonymous\"} : {Id.ToString()}")]
    public class SelfUser : User
    {
        /// <summary>
        /// Get any flags on this user account
        /// </summary>
        public AccountFlags Flags { get; internal set; }

        /// <summary>
        /// Gets the account name of this user. If this user is an anonymous account, the name
        /// </summary>
        public string AccountName { get; internal set; }

        /// <summary>
        /// Gets the vanity url to access the current user's profile
        /// </summary>
        public string VanityUrl { get; internal set; }

        /// <summary>
        /// Gets the email address for the current user
        /// </summary>
        public string Email { get; internal set; }

        /// <summary>
        /// Gets the current user's wallet
        /// </summary>
        public Wallet Wallet { get; internal set; }

        /// <summary>
        /// Gets whether the current user's email is validated
        /// </summary>
        public bool EmailValidated { get; internal set; }

        /// <summary>
        /// Gets whether changing credentials requires verification through email
        /// </summary>
        public bool CredentialChangeRequiresCode { get; internal set; }

        /// <summary>
        /// Gets whether changing the current user's password or secret question or answer requires verification through email
        /// </summary>
        public bool PasswordOrSecretQuestionChangeRequiresCode { get; internal set; }
        
        internal SelfUser(SteamId id) : base(id)
        {
            Wallet = new Wallet(CurrencyCode.Invalid, 0, 0);
        }

        internal void Reset()
        {
            Flags = 0;
            Email = null;
            AccountName = null;
            PasswordOrSecretQuestionChangeRequiresCode = false;
            CredentialChangeRequiresCode = false;
            EmailValidated = false;
            VanityUrl = null;
            Wallet.Cents = 0;
            Wallet.CentsPending = 0;
            Wallet.Currency = 0;

            PlayerName = null;
            Status = 0;
        }
    }
}