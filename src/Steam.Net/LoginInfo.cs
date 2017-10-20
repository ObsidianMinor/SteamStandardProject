namespace Steam.Net
{
    internal class LoginInfo
    {
        internal string Username { get; set; }
        internal string Password { get; set; }
        internal string TwoFactorCode { get; set; }
        internal string AuthCode { get; set; }
        internal string LoginKey { get; set; }
        internal bool ShouldRememberPassword { get; set; }
        internal bool RequestSteam2Ticket { get; set; }
        internal byte[] SentryFileHash { get; set; }
        internal uint AccountId { get; set; }
        internal AccountType AccountType { get; set; }
    }
}