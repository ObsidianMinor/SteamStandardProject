using System;

namespace Steam.Net
{
    internal class DisconnectedEventArgs : EventArgs
    {
        internal bool IsReconnecting { get; }
        internal Exception Exception { get; }

        internal DisconnectedEventArgs(bool reconnecting, Exception execption)
        {
            IsReconnecting = reconnecting;
            Exception = execption;
        }
    }
}
