using Steam.KeyValues.Linq;
using Steam.KeyValues.Utilities;
using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security;

namespace Steam.KeyValues.Serialization
{
    /// <summary>
    /// Contract details for a <see cref="System.Type"/> used by the <see cref="KeyValueSerializer"/>.
    /// </summary>
    public class KeyValueObjectContract : KeyValueContainerContract
    {
        /// <summary>
        /// Gets or sets the property name of this object if it is the root object
        /// </summary>
        public string RootPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the condition for this object if it is the root object
        /// </summary>
        public Conditional? Conditional { get; set; }

        /// <summary>
        /// Gets or sets the base file for this object if it is the root object
        /// </summary>
        public string BaseString { get; set; }

        /// <summary>
        /// Gets or sets the object member serialization.
        /// </summary>
        /// <value>The member object serialization.</value>
        public MemberSerialization MemberSerialization { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the object's properties are required.
        /// </summary>
        /// <value>
        /// 	A value indicating whether the object's properties are required.
        /// </value>
        public Required? ItemRequired { get; set; }

        /// <summary>
        /// Gets the object's properties.
        /// </summary>
        /// <value>The object's properties.</value>
        public KeyValuePropertyCollection Properties { get; }

        /// <summary>
        /// Gets a collection of <see cref="KeyValueProperty"/> instances that define the parameters used with <see cref="KeyValueObjectContract.OverrideCreator"/>.
        /// </summary>
        public KeyValuePropertyCollection CreatorParameters
        {
            get
            {
                if (_creatorParameters == null)
                {
                    _creatorParameters = new KeyValuePropertyCollection(UnderlyingType);
                }

                return _creatorParameters;
            }
        }

        /// <summary>
        /// Gets or sets the function used to create the object. When set this function will override <see cref="KeyValueContract.DefaultCreator"/>.
        /// This function is called with a collection of arguments which are defined by the <see cref="KeyValueObjectContract.CreatorParameters"/> collection.
        /// </summary>
        /// <value>The function used to create the object.</value>
        public ObjectConstructor<object> OverrideCreator
        {
            get { return _overrideCreator; }
            set { _overrideCreator = value; }
        }

        internal ObjectConstructor<object> ParameterizedCreator
        {
            get { return _parameterizedCreator; }
            set { _parameterizedCreator = value; }
        }

        /// <summary>
        /// Gets or sets the extension data setter.
        /// </summary>
        public ExtensionDataSetter ExtensionDataSetter { get; set; }

        /// <summary>
        /// Gets or sets the extension data getter.
        /// </summary>
        public ExtensionDataGetter ExtensionDataGetter { get; set; }

        /// <summary>
        /// Gets or sets the extension data value type.
        /// </summary>
        public Type ExtensionDataValueType
        {
            get { return _extensionDataValueType; }
            set
            {
                _extensionDataValueType = value;
                ExtensionDataIsKVToken = (value != null && typeof(KVToken).IsAssignableFrom(value));
            }
        }

        /// <summary>
        /// Gets or sets the extension data name resolver.
        /// </summary>
        /// <value>The extension data name resolver.</value>
        public Func<string, string> ExtensionDataNameResolver { get; set; }

        internal bool ExtensionDataIsKVToken;
        private bool? _hasRequiredOrDefaultValueProperties;
        private ObjectConstructor<object> _overrideCreator;
        private ObjectConstructor<object> _parameterizedCreator;
        private KeyValuePropertyCollection _creatorParameters;
        private Type _extensionDataValueType;

        internal bool HasRequiredOrDefaultValueProperties
        {
            get
            {
                if (_hasRequiredOrDefaultValueProperties == null)
                {
                    _hasRequiredOrDefaultValueProperties = false;

                    if (ItemRequired.GetValueOrDefault(Required.Default) != Required.Default)
                    {
                        _hasRequiredOrDefaultValueProperties = true;
                    }
                    else
                    {
                        foreach (KeyValueProperty property in Properties)
                        {
                            if (property.Required != Required.Default || (property.DefaultValueHandling & DefaultValueHandling.Populate) == DefaultValueHandling.Populate)
                            {
                                _hasRequiredOrDefaultValueProperties = true;
                                break;
                            }
                        }
                    }
                }

                return _hasRequiredOrDefaultValueProperties.GetValueOrDefault();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueObjectContract"/> class.
        /// </summary>
        /// <param name="underlyingType">The underlying type for the contract.</param>
        public KeyValueObjectContract(Type underlyingType)
            : base(underlyingType)
        {
            ContractType = KeyValueContractType.Object;

            Properties = new KeyValuePropertyCollection(UnderlyingType);
        }

        [SecuritySafeCritical]
        internal object GetUninitializedObject()
        {
            // we should never get here if the environment is not fully trusted, check just in case
            if (!KeyValueTypeReflector.FullyTrusted)
            {
                throw new KeyValueException("Insufficient permissions. Creating an uninitialized '{0}' type requires full trust.".FormatWith(CultureInfo.InvariantCulture, NonNullableUnderlyingType));
            }

            return FormatterServices.GetUninitializedObject(NonNullableUnderlyingType);
        }
    }
}
