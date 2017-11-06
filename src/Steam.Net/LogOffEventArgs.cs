using System;

namespace Steam.Net
{
    public class LogOffEventArgs : EventArgs
    {
        public Result Result { get; }

        public LogOffEventArgs(Result result)
        {
            Result = Result;
        }
    }
}
