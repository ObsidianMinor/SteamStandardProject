using System;

namespace Steam.Net
{
    /// <summary>
    /// Provides data about a rejected login request
    /// </summary>
    public class LoginRejectedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the Steam ID of the account this login attempted to access
        /// </summary>
        public SteamId RejectedAccountSteamId { get; }

        /// <summary>
        /// Gets the result of this login
        /// </summary>
        public Result Result { get; }

        /// <summary>
        /// Gets the extended result of this login
        /// </summary>
        public Result ExtendedResult { get; }

        /// <summary>
        /// Gets the email domain for this Steam Guard account
        /// </summary>
        public string SteamGuardEmailDomain { get; }

        /// <summary>
        /// Gets whether the result of this login indicates a two factor error
        /// </summary>
        public bool IsSteamGuardError { get; }

        /// <summary>
        /// Gets whether the result of this login indicates a mobile auth error
        /// </summary>
        public bool IsMobileAuthenticatorError { get; }

        public LoginRejectedEventArgs(SteamId id, Result result, Result extended, string emailDomain)
        {
            RejectedAccountSteamId = id;
            Result = result;
            ExtendedResult = extended;
            SteamGuardEmailDomain = emailDomain;

            IsMobileAuthenticatorError = result == Result.AccountLoginDeniedNeedTwoFactor || result == Result.TwoFactorCodeMismatch;
            IsSteamGuardError = result == Result.AccountLogonDenied;
        }
    }
}
