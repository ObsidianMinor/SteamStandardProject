using System;

namespace Steam.Authenticator
{
    public class SteamGuardAuthenticator
    {
        public string SharedSecret { get; private set; }
        public string SerialNumber { get; private set; }
        public string RevocationCode { get; private set; }
        public Uri Uri { get; private set; }
        public DateTimeOffset ServerTime { get; private set; }
        public string AccountName { get; private set; }
        public string TokenGid { get; private set; }
        public string IdentitySecret { get; private set; }
        public string Secret { get; private set; }
        public int Status { get; private set; }
        public string DeviceId { get; private set; }
    }
}
