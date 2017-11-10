using System;

namespace Steam.Net
{
    /// <summary>
    /// Represents a clan event on the Steam community
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Gets this event's ID
        /// </summary>
        public SteamGid Id { get; }

        /// <summary>
        /// Gets the time this event was posted
        /// </summary>
        public DateTimeOffset Time { get; }

        /// <summary>
        /// Gets the headline for this event
        /// </summary>
        public string Headline { get; }

        /// <summary>
        /// Get the ID of the game associated with this event
        /// </summary>
        public GameId GameId { get; }

        /// <summary>
        /// Gets whether this event was just posted
        /// </summary>
        public bool JustPosted { get; }

        internal Event(ulong id, uint time, string headline, ulong game, bool justPosted)
        {
            Id = id;
            Time = DateTimeOffset.FromUnixTimeSeconds(time);
            Headline = headline;
            GameId = game;
            JustPosted = justPosted;
        }
    }
}
