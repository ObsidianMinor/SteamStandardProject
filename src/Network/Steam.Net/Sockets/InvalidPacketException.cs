using System;

namespace Steam.Net.Sockets
{
    public class InvalidPacketException : Exception
    {
        public object ExcepectedData { get; set; }

        public object ActualData { get; set; }

        public InvalidPacketException() : base() { }

        public InvalidPacketException(string message) : base(message) { }
        public override string ToString()
        {
            return $"{Message + ". "}Expected {ExcepectedData}, received {ActualData}";
        }
    }
}
