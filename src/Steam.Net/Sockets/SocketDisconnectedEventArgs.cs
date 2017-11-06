using System;

namespace Steam.Net.Sockets
{
    public class SocketDisconnectedEventArgs : EventArgs
    {
        public Exception Exception { get; }

        public SocketDisconnectedEventArgs(Exception ex)
        {
            Exception = ex;
        }
    }
}
