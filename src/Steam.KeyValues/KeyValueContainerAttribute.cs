using Steam.KeyValues.Serialization;
using System;

namespace Steam.KeyValues
{
    /// <summary>
    /// Instructs the <see cref="KeyValueSerializer"/> how to serialize the object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public abstract class KeyValueContainerAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the collection's items converter.
        /// </summary>
        /// <value>The collection's items converter.</value>
        public Type ItemConverterType { get; set; }

        /// <summary>
        /// The parameter list to use when constructing the <see cref="KeyValueConverter"/> described by <see cref="ItemConverterType"/>.
        /// If <c>null</c>, the default constructor is used.
        /// When non-<c>null</c>, there must be a constructor defined in the <see cref="KeyValueConverter"/> that exactly matches the number,
        /// order, and type of these parameters.
        /// </summary>
        /// <example>
        /// <code>
        /// [JsonContainer(ItemConverterType = typeof(MyContainerConverter), ItemConverterParameters = new object[] { 123, "Four" })]
        /// </code>
        /// </example>
        public object[] ItemConverterParameters { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Type"/> of the <see cref="NamingStrategy"/>.
        /// </summary>
        /// <value>The <see cref="Type"/> of the <see cref="NamingStrategy"/>.</value>
        public Type NamingStrategyType
        {
            get { return _namingStrategyType; }
            set
            {
                _namingStrategyType = value;
                NamingStrategyInstance = null;
            }
        }

        /// <summary>
        /// The parameter list to use when constructing the <see cref="NamingStrategy"/> described by <see cref="NamingStrategyType"/>.
        /// If <c>null</c>, the default constructor is used.
        /// When non-<c>null</c>, there must be a constructor defined in the <see cref="NamingStrategy"/> that exactly matches the number,
        /// order, and type of these parameters.
        /// </summary>
        /// <example>
        /// <code>
        /// [JsonContainer(NamingStrategyType = typeof(MyNamingStrategy), NamingStrategyParameters = new object[] { 123, "Four" })]
        /// </code>
        /// </example>
        public object[] NamingStrategyParameters
        {
            get { return _namingStrategyParameters; }
            set
            {
                _namingStrategyParameters = value;
                NamingStrategyInstance = null;
            }
        }

        internal NamingStrategy NamingStrategyInstance { get; set; }

        // yuck. can't set nullable properties on an attribute in C#
        // have to use this approach to get an unset default state
        internal bool? _isReference;
        internal bool? _itemIsReference;
        internal ReferenceLoopHandling? _itemReferenceLoopHandling;
        internal TypeNameHandling? _itemTypeNameHandling;
        private Type _namingStrategyType;
        private object[] _namingStrategyParameters;

        /// <summary>
        /// Gets or sets a value that indicates whether to preserve object references.
        /// </summary>
        /// <value>
        /// 	<c>true</c> to keep object reference; otherwise, <c>false</c>. The default is <c>false</c>.
        /// </value>
        public bool IsReference
        {
            get { return _isReference ?? default(bool); }
            set { _isReference = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether to preserve collection's items references.
        /// </summary>
        /// <value>
        /// 	<c>true</c> to keep collection's items object references; otherwise, <c>false</c>. The default is <c>false</c>.
        /// </value>
        public bool ItemIsReference
        {
            get { return _itemIsReference ?? default(bool); }
            set { _itemIsReference = value; }
        }

        /// <summary>
        /// Gets or sets the reference loop handling used when serializing the collection's items.
        /// </summary>
        /// <value>The reference loop handling.</value>
        public ReferenceLoopHandling ItemReferenceLoopHandling
        {
            get { return _itemReferenceLoopHandling ?? default(ReferenceLoopHandling); }
            set { _itemReferenceLoopHandling = value; }
        }

        /// <summary>
        /// Gets or sets the type name handling used when serializing the collection's items.
        /// </summary>
        /// <value>The type name handling.</value>
        public TypeNameHandling ItemTypeNameHandling
        {
            get { return _itemTypeNameHandling ?? default(TypeNameHandling); }
            set { _itemTypeNameHandling = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueContainerAttribute"/> class.
        /// </summary>
        protected KeyValueContainerAttribute()
        {
        }
    }
}
