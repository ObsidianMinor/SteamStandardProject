using System;

namespace Steam.Net
{
    public class ClanEventStartedEventArgs : EventArgs
    {
        public IClan Clan { get; }

        public Event Event { get; }

        public ClanEventStartedEventArgs(IClan clan, Event @event)
        {
            Clan = clan;
            Event = @event;
        }
    }
}
