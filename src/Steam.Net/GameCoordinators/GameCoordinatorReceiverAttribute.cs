using System;

namespace Steam.Net.GameCoordinators
{
    /// <summary>
    /// Informs the client that this method listens for the specified message type
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class GameCoordinatorReceiverAttribute : Attribute
    {
        public GameCoordinatorReceiverAttribute(GameCoordinatorMessageType type)
        {
            Type = type;
        }

        public GameCoordinatorMessageType Type { get; }
    }
}
