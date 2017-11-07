using System;

namespace Steam.Net
{
    public class UserUpdatedEventArgs : EventArgs
    {
        public IUser Before { get; }

        public IUser After { get; }

        public UserUpdatedEventArgs(IUser before, IUser after)
        {
            Before = before;
            After = after;
        }
    }
}