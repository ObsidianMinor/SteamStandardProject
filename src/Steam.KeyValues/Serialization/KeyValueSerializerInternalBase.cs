using Steam.KeyValues.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Steam.KeyValues.Serialization
{
    internal abstract class KeyValueSerializerInternalBase
    {
        private class ReferenceEqualsEqualityComparer : IEqualityComparer<object>
        {
            bool IEqualityComparer<object>.Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            int IEqualityComparer<object>.GetHashCode(object obj)
            {
                // put objects in a bucket based on their reference
                return RuntimeHelpers.GetHashCode(obj);
            }
        }

        private ErrorContext _currentErrorContext;
        private BidirectionalDictionary<string, object> _mappings;

        internal readonly KeyValueSerializer Serializer;
        internal readonly ITraceWriter TraceWriter;
        protected KeyValueSerializerProxy InternalSerializer;

        protected KeyValueSerializerInternalBase(KeyValueSerializer serializer)
        {
            ValidationUtils.ArgumentNotNull(serializer, nameof(serializer));

            Serializer = serializer;
            TraceWriter = serializer.TraceWriter;
        }

        internal BidirectionalDictionary<string, object> DefaultReferenceMappings
        {
            get
            {
                // override equality comparer for object key dictionary
                // object will be modified as it deserializes and might have mutable hashcode
                if (_mappings == null)
                {
                    _mappings = new BidirectionalDictionary<string, object>(
                        EqualityComparer<string>.Default,
                        new ReferenceEqualsEqualityComparer(),
                        "A different value already has the Id '{0}'.",
                        "A different Id has already been assigned for value '{0}'.");
                }

                return _mappings;
            }
        }

        private ErrorContext GetErrorContext(object currentObject, object member, string path, Exception error)
        {
            if (_currentErrorContext == null)
            {
                _currentErrorContext = new ErrorContext(currentObject, member, path, error);
            }

            if (_currentErrorContext.Error != error)
            {
                throw new InvalidOperationException("Current error context error is different to requested error.");
            }

            return _currentErrorContext;
        }

        protected void ClearErrorContext()
        {
            if (_currentErrorContext == null)
            {
                throw new InvalidOperationException("Could not clear error context. Error context is already null.");
            }

            _currentErrorContext = null;
        }

        protected bool IsErrorHandled(object currentObject, KeyValueContract contract, object keyValue, IKeyValueLineInfo lineInfo, string path, Exception ex)
        {
            ErrorContext errorContext = GetErrorContext(currentObject, keyValue, path, ex);

            if (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Error && !errorContext.Traced)
            {
                // only write error once
                errorContext.Traced = true;

                // kind of a hack but meh. might clean this up later
                string message = (GetType() == typeof(KeyValueSerializerInternalWriter)) ? "Error serializing" : "Error deserializing";
                if (contract != null)
                {
                    message += " " + contract.UnderlyingType;
                }
                message += ". " + ex.Message;

                // add line information to non-json.net exception message
                if (!(ex is KeyValueException))
                {
                    message = KeyValuePosition.FormatMessage(lineInfo, path, message);
                }

                TraceWriter.Trace(TraceLevel.Error, message, ex);
            }

            // attribute method is non-static so don't invoke if no object
            if (contract != null && currentObject != null)
            {
                contract.InvokeOnError(currentObject, Serializer.Context, errorContext);
            }

            if (!errorContext.Handled)
            {
                Serializer.OnError(new ErrorEventArgs(currentObject, errorContext));
            }

            return errorContext.Handled;
        }
    }
}
