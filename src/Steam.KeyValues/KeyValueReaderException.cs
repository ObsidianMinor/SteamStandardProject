﻿using System;

namespace Steam.KeyValues
{
    public class KeyValueReaderException : KeyValueException
    {
        /// <summary>
        /// Gets the line number indicating where the error occurred.
        /// </summary>
        /// <value>The line number indicating where the error occurred.</value>
        public int LineNumber { get; }

        /// <summary>
        /// Gets the line position indicating where the error occurred.
        /// </summary>
        /// <value>The line position indicating where the error occurred.</value>
        public int LinePosition { get; }

        /// <summary>
        /// Gets the path to the JSON where the error occurred.
        /// </summary>
        /// <value>The path to the JSON where the error occurred.</value>
        public string Path { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueReaderException"/> class.
        /// </summary>
        public KeyValueReaderException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueReaderException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public KeyValueReaderException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueReaderException"/> class
        /// with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
        public KeyValueReaderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueReaderException"/> class
        /// with a specified error message, JSON path, line number, line position, and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="path">The path to the JSON where the error occurred.</param>
        /// <param name="lineNumber">The line number indicating where the error occurred.</param>
        /// <param name="linePosition">The line position indicating where the error occurred.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
        public KeyValueReaderException(string message, string path, int lineNumber, int linePosition, Exception innerException)
            : base(message, innerException)
        {
            Path = path;
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }

        internal static KeyValueReaderException Create(KeyValueReader reader, string message)
        {
            return Create(reader, message, null);
        }

        internal static KeyValueReaderException Create(KeyValueReader reader, string message, Exception ex)
        {
            return Create(reader as IKeyValueLineInfo, reader.Path, message, ex);
        }

        internal static KeyValueReaderException Create(IKeyValueLineInfo lineInfo, string path, string message, Exception ex)
        {
            message = KeyValuePosition.FormatMessage(lineInfo, path, message);

            int lineNumber;
            int linePosition;
            if (lineInfo != null && lineInfo.HasLineInfo())
            {
                lineNumber = lineInfo.LineNumber;
                linePosition = lineInfo.LinePosition;
            }
            else
            {
                lineNumber = 0;
                linePosition = 0;
            }

            return new KeyValueReaderException(message, path, lineNumber, linePosition, ex);
        }
    }
}
