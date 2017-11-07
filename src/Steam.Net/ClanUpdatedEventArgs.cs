using System;

namespace Steam.Net
{
    public class ClanUpdatedEventArgs : EventArgs
    {
        public IClan Before { get; }

        public IClan After { get; }
    }
}