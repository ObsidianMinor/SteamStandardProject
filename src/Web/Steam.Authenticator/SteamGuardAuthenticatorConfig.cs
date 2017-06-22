using System;
using Steam.Rest;

namespace Steam.Authenticator
{
    public class SteamGuardAuthenticatorConfig : SteamRestConfig
    {
        public override Uri BaseUri => throw new NotImplementedException();
    }
}
