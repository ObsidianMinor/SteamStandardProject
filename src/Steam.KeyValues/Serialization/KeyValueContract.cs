using Steam.KeyValues.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Steam.KeyValues.Serialization
{
    internal enum KeyValueContractType
    {
        None,
        Root,
        Object,
        Array,
        Primitive,
        String,
        Dictionary,
        Dynamic,
        Serializable,
        Linq
    }

    /// <summary>
    /// Handles <see cref="KeyValueSerializer"/> serialization callback events.
    /// </summary>
    /// <param name="o">The object that raised the callback event.</param>
    /// <param name="context">The streaming context.</param>
    public delegate void SerializationCallback(object o, StreamingContext context);

    /// <summary>
    /// Handles <see cref="KeyValueSerializer"/> serialization error callback events.
    /// </summary>
    /// <param name="o">The object that raised the callback event.</param>
    /// <param name="context">The streaming context.</param>
    /// <param name="errorContext">The error context.</param>
    public delegate void SerializationErrorCallback(object o, StreamingContext context, ErrorContext errorContext);

    /// <summary>
    /// Sets extension data for an object during deserialization.
    /// </summary>
    /// <param name="o">The object to set extension data on.</param>
    /// <param name="key">The extension data key.</param>
    /// <param name="value">The extension data value.</param>
    public delegate void ExtensionDataSetter(object o, string key, object value);

    /// <summary>
    /// Gets extension data for an object during serialization.
    /// </summary>
    /// <param name="o">The object to set extension data on.</param>
    public delegate IEnumerable<KeyValuePair<object, object>> ExtensionDataGetter(object o);

    /// <summary>
    /// Contract details for a <see cref="Type"/> used by the <see cref="KeyValueSerializer"/>.
    /// </summary>
    public abstract class KeyValueContract
    {
        internal bool IsNullable { get; set; }
        internal bool IsConvertable { get; set; }
        internal bool IsEnum { get; set; }
        internal Type NonNullableUnderlyingType { get; set; }
        internal ReadType InternalReadType { get; set; }
        internal KeyValueContractType ContractType { get; set; }
        internal bool IsReadOnlyOrFixedSize { get; set; }
        internal bool IsSealed { get; set; }
        internal bool IsInstantiable { get; set; }

        private List<SerializationCallback> _onDeserializedCallbacks;
        private IList<SerializationCallback> _onDeserializingCallbacks;
        private IList<SerializationCallback> _onSerializedCallbacks;
        private IList<SerializationCallback> _onSerializingCallbacks;
        private IList<SerializationErrorCallback> _onErrorCallbacks;
        private Type _createdType;

        /// <summary>
        /// Gets the underlying type for the contract.
        /// </summary>
        /// <value>The underlying type for the contract.</value>
        public Type UnderlyingType { get; }

        /// <summary>
        /// Gets or sets whether this type contract is serialized as a reference.
        /// </summary>
        /// <value>Whether this type contract is serialized as a reference.</value>
        public bool? IsReference { get; set; }

        /// <summary>
        /// Gets or sets the type created during deserialization.
        /// </summary>
        /// <value>The type created during deserialization.</value>
        public Type CreatedType
        {
            get { return _createdType; }
            set
            {
                _createdType = value;

                IsSealed = _createdType.IsSealed();
                IsInstantiable = !(_createdType.IsInterface() || _createdType.IsAbstract());
            }
        }

        /// <summary>
        /// Gets or sets the default <see cref="KeyValueConverter" /> for this contract.
        /// </summary>
        /// <value>The converter.</value>
        public KeyValueConverter Converter { get; set; }

        // internally specified JsonConverter's to override default behavour
        // checked for after passed in converters and attribute specified converters
        internal KeyValueConverter InternalConverter { get; set; }

        /// <summary>
        /// Gets or sets all methods called immediately after deserialization of the object.
        /// </summary>
        /// <value>The methods called immediately after deserialization of the object.</value>
        public IList<SerializationCallback> OnDeserializedCallbacks
        {
            get
            {
                if (_onDeserializedCallbacks == null)
                {
                    _onDeserializedCallbacks = new List<SerializationCallback>();
                }

                return _onDeserializedCallbacks;
            }
        }

        /// <summary>
        /// Gets or sets all methods called during deserialization of the object.
        /// </summary>
        /// <value>The methods called during deserialization of the object.</value>
        public IList<SerializationCallback> OnDeserializingCallbacks
        {
            get
            {
                if (_onDeserializingCallbacks == null)
                {
                    _onDeserializingCallbacks = new List<SerializationCallback>();
                }

                return _onDeserializingCallbacks;
            }
        }

        /// <summary>
        /// Gets or sets all methods called after serialization of the object graph.
        /// </summary>
        /// <value>The methods called after serialization of the object graph.</value>
        public IList<SerializationCallback> OnSerializedCallbacks
        {
            get
            {
                if (_onSerializedCallbacks == null)
                {
                    _onSerializedCallbacks = new List<SerializationCallback>();
                }

                return _onSerializedCallbacks;
            }
        }

        /// <summary>
        /// Gets or sets all methods called before serialization of the object.
        /// </summary>
        /// <value>The methods called before serialization of the object.</value>
        public IList<SerializationCallback> OnSerializingCallbacks
        {
            get
            {
                if (_onSerializingCallbacks == null)
                {
                    _onSerializingCallbacks = new List<SerializationCallback>();
                }

                return _onSerializingCallbacks;
            }
        }

        /// <summary>
        /// Gets or sets all method called when an error is thrown during the serialization of the object.
        /// </summary>
        /// <value>The methods called when an error is thrown during the serialization of the object.</value>
        public IList<SerializationErrorCallback> OnErrorCallbacks
        {
            get
            {
                if (_onErrorCallbacks == null)
                {
                    _onErrorCallbacks = new List<SerializationErrorCallback>();
                }

                return _onErrorCallbacks;
            }
        }

        /// <summary>
        /// Gets or sets the default creator method used to create the object.
        /// </summary>
        /// <value>The default creator method used to create the object.</value>
        public Func<object> DefaultCreator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the default creator is non-public.
        /// </summary>
        /// <value><c>true</c> if the default object creator is non-public; otherwise, <c>false</c>.</value>
        public bool DefaultCreatorNonPublic { get; set; }

        internal KeyValueContract(Type underlyingType)
        {
            ValidationUtils.ArgumentNotNull(underlyingType, nameof(underlyingType));

            UnderlyingType = underlyingType;

            IsNullable = ReflectionUtils.IsNullable(underlyingType);
            NonNullableUnderlyingType = (IsNullable && ReflectionUtils.IsNullableType(underlyingType)) ? Nullable.GetUnderlyingType(underlyingType) : underlyingType;

            CreatedType = NonNullableUnderlyingType;

            IsConvertable = ConvertUtils.IsConvertible(NonNullableUnderlyingType);
            IsEnum = NonNullableUnderlyingType.IsEnum();

            InternalReadType = ReadType.Read;
        }

        internal void InvokeOnSerializing(object o, StreamingContext context)
        {
            if (_onSerializingCallbacks != null)
            {
                foreach (SerializationCallback callback in _onSerializingCallbacks)
                {
                    callback(o, context);
                }
            }
        }

        internal void InvokeOnSerialized(object o, StreamingContext context)
        {
            if (_onSerializedCallbacks != null)
            {
                foreach (SerializationCallback callback in _onSerializedCallbacks)
                {
                    callback(o, context);
                }
            }
        }

        internal void InvokeOnDeserializing(object o, StreamingContext context)
        {
            if (_onDeserializingCallbacks != null)
            {
                foreach (SerializationCallback callback in _onDeserializingCallbacks)
                {
                    callback(o, context);
                }
            }
        }

        internal void InvokeOnDeserialized(object o, StreamingContext context)
        {
            if (_onDeserializedCallbacks != null)
            {
                foreach (SerializationCallback callback in _onDeserializedCallbacks)
                {
                    callback(o, context);
                }
            }
        }

        internal void InvokeOnError(object o, StreamingContext context, ErrorContext errorContext)
        {
            if (_onErrorCallbacks != null)
            {
                foreach (SerializationErrorCallback callback in _onErrorCallbacks)
                {
                    callback(o, context, errorContext);
                }
            }
        }

        internal static SerializationCallback CreateSerializationCallback(MethodInfo callbackMethodInfo)
        {
            return (o, context) => callbackMethodInfo.Invoke(o, new object[] { context });
        }

        internal static SerializationErrorCallback CreateSerializationErrorCallback(MethodInfo callbackMethodInfo)
        {
            return (o, context, econtext) => callbackMethodInfo.Invoke(o, new object[] { context, econtext });
        }
    }
}
