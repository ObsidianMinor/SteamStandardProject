using System;

namespace Steam.Net
{
    public class LoginKeyReceivedEventArgs : EventArgs
    {
        public string LoginKey { get; }

        public LoginKeyReceivedEventArgs(string key)
        {
            LoginKey = key;
        }
    }
}
