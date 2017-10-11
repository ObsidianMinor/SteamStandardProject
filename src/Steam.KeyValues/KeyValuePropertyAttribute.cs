using Steam.KeyValues.Serialization;
using System;

namespace Steam.KeyValues
{
    /// <summary>
    /// Instructs the <see cref="KeyValueSerializer"/> to always serialize this property with the specified name
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
    public class KeyValuePropertyAttribute : Attribute
    {
        // yuck. can't set nullable properties on an attribute in C#
        // have to use this approach to get an unset default state
        internal DefaultValueHandling? _defaultValueHandling;
        internal ReferenceLoopHandling? _referenceLoopHandling;
        internal ObjectCreationHandling? _objectCreationHandling;
        internal TypeNameHandling? _typeNameHandling;
        internal bool? _isReference;
        internal int? _order;
        internal Required? _required;
        internal bool? _itemIsReference;
        internal ReferenceLoopHandling? _itemReferenceLoopHandling;
        internal TypeNameHandling? _itemTypeNameHandling;
        internal Conditional? _condition;

        /// <summary>
        /// Gets or sets the <see cref="KeyValueConverter"/> used when serializing the property's collection items.
        /// </summary>
        /// <value>The collection's items <see cref="KeyValueConverter"/>.</value>
        public Type ItemConverterType { get; set; }

        /// <summary>
        /// The parameter list to use when constructing the <see cref="KeyValueConverter"/> described by <see cref="ItemConverterType"/>.
        /// If <c>null</c>, the default constructor is used.
        /// When non-<c>null</c>, there must be a constructor defined in the <see cref="KeyValueConverter"/> that exactly matches the number,
        /// order, and type of these parameters.
        /// </summary>
        /// <example>
        /// <code>
        /// [JsonProperty(ItemConverterType = typeof(MyContainerConverter), ItemConverterParameters = new object[] { 123, "Four" })]
        /// </code>
        /// </example>
        public object[] ItemConverterParameters { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Type"/> of the <see cref="NamingStrategy"/>.
        /// </summary>
        /// <value>The <see cref="Type"/> of the <see cref="NamingStrategy"/>.</value>
        public Type NamingStrategyType { get; set; }

        /// <summary>
        /// The parameter list to use when constructing the <see cref="NamingStrategy"/> described by <see cref="KeyValuePropertyAttribute.NamingStrategyType"/>.
        /// If <c>null</c>, the default constructor is used.
        /// When non-<c>null</c>, there must be a constructor defined in the <see cref="NamingStrategy"/> that exactly matches the number,
        /// order, and type of these parameters.
        /// </summary>
        /// <example>
        /// <code>
        /// [JsonProperty(NamingStrategyType = typeof(MyNamingStrategy), NamingStrategyParameters = new object[] { 123, "Four" })]
        /// </code>
        /// </example>
        public object[] NamingStrategyParameters { get; set; }
        
        /// <summary>
        /// Gets or sets the default value handling used when serializing this property.
        /// </summary>
        /// <value>The default value handling.</value>
        public DefaultValueHandling DefaultValueHandling
        {
            get { return _defaultValueHandling ?? default(DefaultValueHandling); }
            set { _defaultValueHandling = value; }
        }

        /// <summary>
        /// Gets or sets the reference loop handling used when serializing this property.
        /// </summary>
        /// <value>The reference loop handling.</value>
        public ReferenceLoopHandling ReferenceLoopHandling
        {
            get { return _referenceLoopHandling ?? default(ReferenceLoopHandling); }
            set { _referenceLoopHandling = value; }
        }

        /// <summary>
        /// Gets or sets the object creation handling used when deserializing this property.
        /// </summary>
        /// <value>The object creation handling.</value>
        public ObjectCreationHandling ObjectCreationHandling
        {
            get { return _objectCreationHandling ?? default(ObjectCreationHandling); }
            set { _objectCreationHandling = value; }
        }

        /// <summary>
        /// Gets or sets the type name handling used when serializing this property.
        /// </summary>
        /// <value>The type name handling.</value>
        public TypeNameHandling TypeNameHandling
        {
            get { return _typeNameHandling ?? default(TypeNameHandling); }
            set { _typeNameHandling = value; }
        }

        /// <summary>
        /// Gets or sets whether this property's value is serialized as a reference.
        /// </summary>
        /// <value>Whether this property's value is serialized as a reference.</value>
        public bool IsReference
        {
            get { return _isReference ?? default(bool); }
            set { _isReference = value; }
        }

        /// <summary>
        /// Gets or sets the order of serialization of a member.
        /// </summary>
        /// <value>The numeric order of serialization.</value>
        public int Order
        {
            get { return _order ?? default(int); }
            set { _order = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this property is required.
        /// </summary>
        /// <value>
        /// 	A value indicating whether this property is required.
        /// </value>
        public Required Required
        {
            get { return _required ?? Required.Default; }
            set { _required = value; }
        }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the reference loop handling used when serializing the property's collection items.
        /// </summary>
        /// <value>The collection's items reference loop handling.</value>
        public ReferenceLoopHandling ItemReferenceLoopHandling
        {
            get { return _itemReferenceLoopHandling ?? default(ReferenceLoopHandling); }
            set { _itemReferenceLoopHandling = value; }
        }

        /// <summary>
        /// Gets or sets the type name handling used when serializing the property's collection items.
        /// </summary>
        /// <value>The collection's items type name handling.</value>
        public TypeNameHandling ItemTypeNameHandling
        {
            get { return _itemTypeNameHandling ?? default(TypeNameHandling); }
            set { _itemTypeNameHandling = value; }
        }

        /// <summary>
        /// Gets or sets whether this property's collection items are serialized as a reference.
        /// </summary>
        /// <value>Whether this property's collection items are serialized as a reference.</value>
        public bool ItemIsReference
        {
            get { return _itemIsReference ?? default(bool); }
            set { _itemIsReference = value; }
        }

        /// <summary>
        /// Gets or sets this property's condition
        /// </summary>
        public Conditional Condition
        {
            get => _condition ?? default(Conditional);
            set
            {
                if (value > Conditional.NotWindows || value < Conditional.None)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _condition = value;
            }
        }

        /// <summary>
        /// Initializes the attribute with the default values
        /// </summary>
        public KeyValuePropertyAttribute() { }

        /// <summary>
        /// Initializes the attribute with the default values and the specified property name
        /// </summary>
        /// <param name="propertyName"></param>
        public KeyValuePropertyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}
