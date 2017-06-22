using System;

namespace Steam.Rest
{
    [Flags]
    public enum RetryMode
    {
        AlwaysFail,
        RetryTimeouts,
        RetryBadGateway = 4,
        AlwaysRetry = RetryTimeouts | RetryBadGateway
    }
}
