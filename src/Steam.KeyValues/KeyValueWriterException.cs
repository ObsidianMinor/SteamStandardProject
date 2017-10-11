using System;
using System.Collections.Generic;
using System.Text;

namespace Steam.KeyValues
{
    /// <summary>
    /// The exception thrown when an error occurs while writing KeyValue text.
    /// </summary>
    public class KeyValueWriterException : KeyValueException
    {
        /// <summary>
        /// Gets the path to the KeyValue where the error occurred.
        /// </summary>
        /// <value>The path to the KeyValue where the error occurred.</value>
        public string Path { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueWriterException"/> class.
        /// </summary>
        public KeyValueWriterException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueWriterException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public KeyValueWriterException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueWriterException"/> class
        /// with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
        public KeyValueWriterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueWriterException"/> class
        /// with a specified error message, KeyValue path and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="path">The path to the KeyValue where the error occurred.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
        public KeyValueWriterException(string message, string path, Exception innerException)
            : base(message, innerException)
        {
            Path = path;
        }

        internal static KeyValueWriterException Create(KeyValueWriter writer, string message, Exception ex)
        {
            return Create(writer.ContainerPath, message, ex);
        }

        internal static KeyValueWriterException Create(string path, string message, Exception ex)
        {
            message = KeyValuePosition.FormatMessage(null, path, message);

            return new KeyValueWriterException(message, path, ex);
        }
    }
}
