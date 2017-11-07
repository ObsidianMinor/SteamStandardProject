using System;

namespace Steam.Net
{
    public class WalletUpdatedEventArgs : EventArgs
    {
        public Wallet Before { get; }

        public Wallet After { get; }

        public WalletUpdatedEventArgs(Wallet before, Wallet after)
        {
            Before = before;
            After = after;
        }
    }
}