using System;

namespace Steam.Net
{
    public class ClanUpdatedEventArgs : EventArgs
    {
        public IClan Before { get; }

        public IClan After { get; }

        public ClanUpdatedEventArgs(IClan before, IClan after)
        {
            Before = before;
            After = after;
        }
    }
}