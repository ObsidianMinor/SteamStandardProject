using System;

namespace Steam.KeyValues
{
    public class KeyValuesException : Exception
    {
        public KeyValuesException() : base() { }

        public KeyValuesException(string message) : base(message) { }

        public KeyValuesException(string message, Exception innerException) : base(message, innerException) { }
    }
}
