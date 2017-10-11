using System;

namespace Steam.KeyValues
{
    /// <summary>
    /// The exception thrown when an error occurs during KeyValue serialization or deserialization.
    /// </summary>
    public class KeyValueSerializationException : KeyValueException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueSerializationException"/> class.
        /// </summary>
        public KeyValueSerializationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueSerializationException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public KeyValueSerializationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueSerializationException"/> class
        /// with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
        public KeyValueSerializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        
        internal static KeyValueSerializationException Create(KeyValueReader reader, string message)
        {
            return Create(reader, message, null);
        }

        internal static KeyValueSerializationException Create(KeyValueReader reader, string message, Exception ex)
        {
            return Create(reader as IKeyValueLineInfo, reader.Path, message, ex);
        }

        internal static KeyValueSerializationException Create(IKeyValueLineInfo lineInfo, string path, string message, Exception ex)
        {
            message = KeyValuePosition.FormatMessage(lineInfo, path, message);

            return new KeyValueSerializationException(message, ex);
        }
    }
}
