using System.Diagnostics;

namespace Steam
{
    /// <summary>
    /// Provides an object with access to its owner <see cref="SteamClient"/>
    /// </summary>
    /// <typeparam name="T">The type of client</typeparam>
    public abstract class Entity<T> where T : SteamClient
    {
        /// <summary>
        /// The client that owns this object
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected T Client { get; }

        /// <summary>
        /// Sets the client for this object
        /// </summary>
        /// <param name="client"></param>
        protected Entity(T client)
        {
            Client = client;
        }
    }
}
