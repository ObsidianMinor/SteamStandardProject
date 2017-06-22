using System;
using System.Collections.Generic;
using System.Text;

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
