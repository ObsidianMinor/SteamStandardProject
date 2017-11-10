using System;

namespace Steam.Net
{
    public class CurrentUserUpdatedEventArgs : EventArgs
    {
        public ISelfUser Before { get; }

        public ISelfUser After { get; }

        public CurrentUserUpdatedEventArgs(ISelfUser before, ISelfUser after)
        {
            Before = before;
            After = after;
        }
    }
}
