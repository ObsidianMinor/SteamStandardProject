using Steam.KeyValues.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;

namespace Steam.KeyValues
{
    /// <summary>
    /// Specifies the settings on a <see cref="KeyValueSerializer"/> object.
    /// </summary>
    public class KeyValueSerializerSettings
    {
        internal const ReferenceLoopHandling DefaultReferenceLoopHandling = ReferenceLoopHandling.Error;
        internal const MissingMemberHandling DefaultMissingMemberHandling = MissingMemberHandling.Ignore;
        internal const DefaultValueHandling DefaultDefaultValueHandling = DefaultValueHandling.Include;
        internal const ObjectCreationHandling DefaultObjectCreationHandling = ObjectCreationHandling.Auto;
        internal const ConstructorHandling DefaultConstructorHandling = ConstructorHandling.Default;
        internal const TypeNameHandling DefaultTypeNameHandling = TypeNameHandling.None;
        internal const MetadataPropertyHandling DefaultMetadataPropertyHandling = MetadataPropertyHandling.Default;
        internal static readonly StreamingContext DefaultContext;

        internal const Formatting DefaultFormatting = Formatting.None;
        internal const FloatParseHandling DefaultFloatParseHandling = FloatParseHandling.Double;
        internal const FloatFormatHandling DefaultFloatFormatHandling = FloatFormatHandling.String;
        internal const StringEscapeHandling DefaultStringEscapeHandling = StringEscapeHandling.Default;
        internal const TypeNameAssemblyFormatHandling DefaultTypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;
        internal static readonly CultureInfo DefaultCulture;
        internal const bool DefaultCheckAdditionalContent = false;
        internal const string DefaultDateFormatString = @"yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";

        internal Formatting? _formatting;
        internal FloatFormatHandling? _floatFormatHandling;
        internal FloatParseHandling? _floatParseHandling;
        internal StringEscapeHandling? _stringEscapeHandling;
        internal CultureInfo _culture;
        internal bool? _checkAdditionalContent;
        internal int? _maxDepth;
        internal bool _maxDepthSet;
        internal string _dateFormatString;
        internal bool _dateFormatStringSet;
        internal TypeNameAssemblyFormatHandling? _typeNameAssemblyFormatHandling;
        internal DefaultValueHandling? _defaultValueHandling;
        internal ObjectCreationHandling? _objectCreationHandling;
        internal MissingMemberHandling? _missingMemberHandling;
        internal ReferenceLoopHandling? _referenceLoopHandling;
        internal StreamingContext? _context;
        internal ConstructorHandling? _constructorHandling;
        internal TypeNameHandling? _typeNameHandling;
        internal MetadataPropertyHandling? _metadataPropertyHandling;

        /// <summary>
        /// Gets or sets how reference loops (e.g. a class referencing itself) are handled.
        /// </summary>
        /// <value>Reference loop handling.</value>
        public ReferenceLoopHandling ReferenceLoopHandling
        {
            get { return _referenceLoopHandling ?? DefaultReferenceLoopHandling; }
            set { _referenceLoopHandling = value; }
        }

        /// <summary>
        /// Gets or sets how missing members (e.g. JSON contains a property that isn't a member on the object) are handled during deserialization.
        /// </summary>
        /// <value>Missing member handling.</value>
        public MissingMemberHandling MissingMemberHandling
        {
            get { return _missingMemberHandling ?? DefaultMissingMemberHandling; }
            set { _missingMemberHandling = value; }
        }

        /// <summary>
        /// Gets or sets how objects are created during deserialization.
        /// </summary>
        /// <value>The object creation handling.</value>
        public ObjectCreationHandling ObjectCreationHandling
        {
            get { return _objectCreationHandling ?? DefaultObjectCreationHandling; }
            set { _objectCreationHandling = value; }
        }
        
        /// <summary>
        /// Gets or sets how default values are handled during serialization and deserialization.
        /// </summary>
        /// <value>The default value handling.</value>
        public DefaultValueHandling DefaultValueHandling
        {
            get { return _defaultValueHandling ?? DefaultDefaultValueHandling; }
            set { _defaultValueHandling = value; }
        }

        /// <summary>
        /// Gets or sets a <see cref="KeyValueConverter"/> collection that will be used during serialization.
        /// </summary>
        /// <value>The converters.</value>
        public IList<KeyValueConverter> Converters { get; set; }
        
        /// <summary>
        /// Gets or sets how type name writing and reading is handled by the serializer.
        /// </summary>
        /// <remarks>
        /// <see cref="KeyValueSerializerSettings.TypeNameHandling"/> should be used with caution when your application deserializes JSON from an external source.
        /// Incoming types should be validated with a custom <see cref="KeyValueSerializerSettings.SerializationBinder"/>
        /// when deserializing with a value other than <see cref="TypeNameHandling.None"/>.
        /// </remarks>
        /// <value>The type name handling.</value>
        public TypeNameHandling TypeNameHandling
        {
            get { return _typeNameHandling ?? DefaultTypeNameHandling; }
            set { _typeNameHandling = value; }
        }

        /// <summary>
        /// Gets or sets how metadata properties are used during deserialization.
        /// </summary>
        /// <value>The metadata properties handling.</value>
        public MetadataPropertyHandling MetadataPropertyHandling
        {
            get { return _metadataPropertyHandling ?? DefaultMetadataPropertyHandling; }
            set { _metadataPropertyHandling = value; }
        }

        /// <summary>
        /// Gets or sets how a type name assembly is written and resolved by the serializer.
        /// </summary>
        /// <value>The type name assembly format.</value>
        [Obsolete("TypeNameAssemblyFormat is obsolete. Use TypeNameAssemblyFormatHandling instead.")]
        public FormatterAssemblyStyle TypeNameAssemblyFormat
        {
            get { return (FormatterAssemblyStyle)TypeNameAssemblyFormatHandling; }
            set { TypeNameAssemblyFormatHandling = (TypeNameAssemblyFormatHandling)value; }
        }

        /// <summary>
        /// Gets or sets how a type name assembly is written and resolved by the serializer.
        /// </summary>
        /// <value>The type name assembly format.</value>
        public TypeNameAssemblyFormatHandling TypeNameAssemblyFormatHandling
        {
            get { return _typeNameAssemblyFormatHandling ?? DefaultTypeNameAssemblyFormatHandling; }
            set { _typeNameAssemblyFormatHandling = value; }
        }

        /// <summary>
        /// Gets or sets how constructors are used during deserialization.
        /// </summary>
        /// <value>The constructor handling.</value>
        public ConstructorHandling ConstructorHandling
        {
            get { return _constructorHandling ?? DefaultConstructorHandling; }
            set { _constructorHandling = value; }
        }

        /// <summary>
        /// Gets or sets the contract resolver used by the serializer when
        /// serializing .NET objects to JSON and vice versa.
        /// </summary>
        /// <value>The contract resolver.</value>
        public IContractResolver ContractResolver { get; set; }

        /// <summary>
        /// Gets or sets the equality comparer used by the serializer when comparing references.
        /// </summary>
        /// <value>The equality comparer.</value>
        public IEqualityComparer EqualityComparer { get; set; }
        
        /// <summary>
        /// Gets or sets the <see cref="ITraceWriter"/> used by the serializer when writing trace messages.
        /// </summary>
        /// <value>The trace writer.</value>
        public ITraceWriter TraceWriter { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SerializationBinder"/> used by the serializer when resolving type names.
        /// </summary>
        /// <value>The binder.</value>
        [Obsolete("Binder is obsolete. Use SerializationBinder instead.")]
        public SerializationBinder Binder
        {
            get
            {
                if (SerializationBinder == null)
                {
                    return null;
                }

                if (SerializationBinder is SerializationBinderAdapter adapter)
                {
                    return adapter.SerializationBinder;
                }

                throw new InvalidOperationException("Cannot get SerializationBinder because an ISerializationBinder was previously set.");
            }
            set { SerializationBinder = value == null ? null : new SerializationBinderAdapter(value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="ISerializationBinder"/> used by the serializer when resolving type names.
        /// </summary>
        /// <value>The binder.</value>
        public ISerializationBinder SerializationBinder { get; set; }

        /// <summary>
        /// Gets or sets the error handler called during serialization and deserialization.
        /// </summary>
        /// <value>The error handler called during serialization and deserialization.</value>
        public EventHandler<ErrorEventArgs> Error { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="StreamingContext"/> used by the serializer when invoking serialization callback methods.
        /// </summary>
        /// <value>The context.</value>
        public StreamingContext Context
        {
            get { return _context ?? DefaultContext; }
            set { _context = value; }
        }

        /// <summary>
        /// Gets or sets how <see cref="DateTime"/> and <see cref="DateTimeOffset"/> values are formatted when writing JSON text,
        /// and the expected date format when reading JSON text.
        /// </summary>
        public string DateFormatString
        {
            get { return _dateFormatString ?? DefaultDateFormatString; }
            set
            {
                _dateFormatString = value;
                _dateFormatStringSet = true;
            }
        }

        /// <summary>
        /// Gets or sets the maximum depth allowed when reading JSON. Reading past this depth will throw a <see cref="KeyValueReaderException"/>.
        /// </summary>
        public int? MaxDepth
        {
            get { return _maxDepth; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Value must be positive.", nameof(value));
                }

                _maxDepth = value;
                _maxDepthSet = true;
            }
        }

        /// <summary>
        /// Indicates how JSON text output is formatted.
        /// </summary>
        public Formatting Formatting
        {
            get { return _formatting ?? DefaultFormatting; }
            set { _formatting = value; }
        }
        
        /// <summary>
        /// Gets or sets how special floating point numbers, e.g. <see cref="Double.NaN"/>,
        /// <see cref="Double.PositiveInfinity"/> and <see cref="Double.NegativeInfinity"/>,
        /// are written as JSON.
        /// </summary>
        public FloatFormatHandling FloatFormatHandling
        {
            get { return _floatFormatHandling ?? DefaultFloatFormatHandling; }
            set { _floatFormatHandling = value; }
        }

        /// <summary>
        /// Gets or sets how floating point numbers, e.g. 1.0 and 9.9, are parsed when reading JSON text.
        /// </summary>
        public FloatParseHandling FloatParseHandling
        {
            get { return _floatParseHandling ?? DefaultFloatParseHandling; }
            set { _floatParseHandling = value; }
        }

        /// <summary>
        /// Gets or sets how strings are escaped when writing JSON text.
        /// </summary>
        public StringEscapeHandling StringEscapeHandling
        {
            get { return _stringEscapeHandling ?? DefaultStringEscapeHandling; }
            set { _stringEscapeHandling = value; }
        }

        /// <summary>
        /// Gets or sets the culture used when reading JSON. Defaults to <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        public CultureInfo Culture
        {
            get { return _culture ?? DefaultCulture; }
            set { _culture = value; }
        }

        /// <summary>
        /// Gets a value indicating whether there will be a check for additional content after deserializing an object.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if there will be a check for additional content after deserializing an object; otherwise, <c>false</c>.
        /// </value>
        public bool CheckAdditionalContent
        {
            get { return _checkAdditionalContent ?? DefaultCheckAdditionalContent; }
            set { _checkAdditionalContent = value; }
        }

        static KeyValueSerializerSettings()
        {
            DefaultContext = new StreamingContext();
            DefaultCulture = CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueSerializerSettings"/> class.
        /// </summary>
        public KeyValueSerializerSettings()
        {
            Converters = new List<KeyValueConverter>();
        }
    }
}
