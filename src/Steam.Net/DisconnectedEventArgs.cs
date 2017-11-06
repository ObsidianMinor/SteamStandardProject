using System;

namespace Steam.Net
{
    public class DisconnectedEventArgs : EventArgs
    {
        public bool IsReconnecting { get; }
        public Exception Exception { get; }

        internal DisconnectedEventArgs(bool reconnecting, Exception execption)
        {
            IsReconnecting = reconnecting;
            Exception = execption;
        }
    }
}
