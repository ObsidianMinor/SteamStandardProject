using System;

namespace Steam.KeyValues
{
    /// <summary>
    /// Instructs the <see cref="KeyValueSerializer"/> to use the specified <see cref="KeyValueConverter"/> when serializing the member or class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class KeyValueConverterAttribute : Attribute
    {
        private readonly Type _converterType;

        /// <summary>
        /// Gets the <see cref="Type"/> of the <see cref="KeyValueConverter"/>.
        /// </summary>
        /// <value>The <see cref="Type"/> of the <see cref="KeyValueConverter"/>.</value>
        public Type ConverterType
        {
            get { return _converterType; }
        }

        /// <summary>
        /// The parameter list to use when constructing the <see cref="KeyValueConverter"/> described by <see cref="ConverterType"/>.
        /// If <c>null</c>, the default constructor is used.
        /// </summary>
        public object[] ConverterParameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueConverterAttribute"/> class.
        /// </summary>
        /// <param name="converterType">Type of the <see cref="KeyValueConverter"/>.</param>
        public KeyValueConverterAttribute(Type converterType)
        {
            _converterType = converterType ?? throw new ArgumentNullException(nameof(converterType));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueConverterAttribute"/> class.
        /// </summary>
        /// <param name="converterType">Type of the <see cref="KeyValueConverter"/>.</param>
        /// <param name="converterParameters">Parameter list to use when constructing the <see cref="KeyValueConverter"/>. Can be <c>null</c>.</param>
        public KeyValueConverterAttribute(Type converterType, params object[] converterParameters)
            : this(converterType)
        {
            ConverterParameters = converterParameters;
        }
    }
}
