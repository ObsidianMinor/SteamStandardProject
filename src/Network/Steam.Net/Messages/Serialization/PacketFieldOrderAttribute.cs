using System;

namespace Steam.Net.Messages.Serialization
{
    internal class PacketFieldOrderAttribute : Attribute
    {
        internal int Position { get; }

        internal PacketFieldOrderAttribute(int position)
        {
            Position = position;
        }
    }
}
