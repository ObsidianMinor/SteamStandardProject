using System;

namespace Steam.Net
{
    public class CurrentUserUpdatedEventArgs : EventArgs
    {
        public SelfUser Before { get; }

        public SelfUser After { get; }

        public CurrentUserUpdatedEventArgs(SelfUser before, SelfUser after)
        {
            Before = before;
            After = after;
        }
    }
}