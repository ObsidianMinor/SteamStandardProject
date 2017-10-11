using System;

namespace Steam.KeyValues
{
    /// <summary>
    /// The exception thrown when an error occurs during KeyValue serialization or deserialization.
    /// </summary>
    public class KeyValueException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueException"/> class.
        /// </summary>
        public KeyValueException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public KeyValueException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueException"/> class
        /// with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
        public KeyValueException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        internal static KeyValueException Create(IKeyValueLineInfo lineInfo, string path, string message)
        {
            message = KeyValuePosition.FormatMessage(lineInfo, path, message);

            return new KeyValueException(message);
        }
    }
}
