using Steam.KeyValues.Serialization;
using Steam.KeyValues.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;

namespace Steam.KeyValues
{
    /// <summary>
    /// Serializes and deserializes objects into and from the KeyValues format.
    /// The <see cref="KeyValueSerializer"/> enables you to control how objects are encoded into KeyValues.
    /// </summary>
    public class KeyValueSerializer
    {
        internal TypeNameHandling _typeNameHandling;
        internal TypeNameAssemblyFormatHandling _typeNameAssemblyFormatHandling;
        internal ReferenceLoopHandling _referenceLoopHandling;
        internal MissingMemberHandling _missingMemberHandling;
        internal ObjectCreationHandling _objectCreationHandling;
        internal DefaultValueHandling _defaultValueHandling;
        internal ConstructorHandling _constructorHandling;
        internal MetadataPropertyHandling _metadataPropertyHandling;
        internal KeyValueConverterCollection _converters;
        internal IContractResolver _contractResolver;
        internal ITraceWriter _traceWriter;
        internal IEqualityComparer _equalityComparer;
        internal ISerializationBinder _serializationBinder;
        internal StreamingContext _context;

        private Formatting? _formatting;
        private FloatFormatHandling? _floatFormatHandling;
        private FloatParseHandling? _floatParseHandling;
        private StringEscapeHandling? _stringEscapeHandling;
        private CultureInfo _culture;
        private int? _maxDepth;
        private bool _maxDepthSet;
        private bool? _checkAdditionalContent;
        private string _dateFormatString;
        private bool _dateFormatStringSet;

        /// <summary>
        /// Occurs when the <see cref="KeyValueSerializer"/> errors during serialization and deserialization.
        /// </summary>
        public virtual event EventHandler<Serialization.ErrorEventArgs> Error;
        
        /// <summary>
        /// Gets or sets the <see cref="SerializationBinder"/> used by the serializer when resolving type names.
        /// </summary>
        [Obsolete("Binder is obsolete. Use SerializationBinder instead.")]
        public virtual SerializationBinder Binder
        {
            get
            {
                if (_serializationBinder == null)
                {
                    return null;
                }

                if (_serializationBinder is SerializationBinder legacySerializationBinder)
                {
                    return legacySerializationBinder;
                }

                if (_serializationBinder is SerializationBinderAdapter adapter)
                {
                    return adapter.SerializationBinder;
                }

                throw new InvalidOperationException("Cannot get SerializationBinder because an ISerializationBinder was previously set.");
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Serialization binder cannot be null.");
                }

                _serializationBinder = value as ISerializationBinder ?? new SerializationBinderAdapter(value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ISerializationBinder"/> used by the serializer when resolving type names.
        /// </summary>
        public virtual ISerializationBinder SerializationBinder
        {
            get { return _serializationBinder; }
            set
            {
                _serializationBinder = value ?? throw new ArgumentNullException(nameof(value), "Serialization binder cannot be null.");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ITraceWriter"/> used by the serializer when writing trace messages.
        /// </summary>
        /// <value>The trace writer.</value>
        public virtual ITraceWriter TraceWriter
        {
            get { return _traceWriter; }
            set { _traceWriter = value; }
        }

        /// <summary>
        /// Gets or sets the equality comparer used by the serializer when comparing references.
        /// </summary>
        /// <value>The equality comparer.</value>
        public virtual IEqualityComparer EqualityComparer
        {
            get { return _equalityComparer; }
            set { _equalityComparer = value; }
        }

        /// <summary>
        /// Gets or sets how type name writing and reading is handled by the serializer.
        /// </summary>
        /// <remarks>
        /// <see cref="KeyValueSerializer.TypeNameHandling"/> should be used with caution when your application deserializes KeyValue from an external source.
        /// Incoming types should be validated with a custom <see cref="KeyValueSerializer.SerializationBinder"/>
        /// when deserializing with a value other than <see cref="TypeNameHandling.None"/>.
        /// </remarks>
        public virtual TypeNameHandling TypeNameHandling
        {
            get { return _typeNameHandling; }
            set
            {
                if (value < TypeNameHandling.None || value > TypeNameHandling.Auto)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _typeNameHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how a type name assembly is written and resolved by the serializer.
        /// </summary>
        /// <value>The type name assembly format.</value>
        [Obsolete("TypeNameAssemblyFormat is obsolete. Use TypeNameAssemblyFormatHandling instead.")]
        public virtual FormatterAssemblyStyle TypeNameAssemblyFormat
        {
            get { return (FormatterAssemblyStyle)_typeNameAssemblyFormatHandling; }
            set
            {
                if (value < FormatterAssemblyStyle.Simple || value > FormatterAssemblyStyle.Full)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _typeNameAssemblyFormatHandling = (TypeNameAssemblyFormatHandling)value;
            }
        }

        /// <summary>
        /// Gets or sets how a type name assembly is written and resolved by the serializer.
        /// </summary>
        /// <value>The type name assembly format.</value>
        public virtual TypeNameAssemblyFormatHandling TypeNameAssemblyFormatHandling
        {
            get { return _typeNameAssemblyFormatHandling; }
            set
            {
                if (value < TypeNameAssemblyFormatHandling.Simple || value > TypeNameAssemblyFormatHandling.Full)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _typeNameAssemblyFormatHandling = value;
            }
        }
        
        /// <summary>
        /// Gets or sets how reference loops (e.g. a class referencing itself) is handled.
        /// </summary>
        public virtual ReferenceLoopHandling ReferenceLoopHandling
        {
            get { return _referenceLoopHandling; }
            set
            {
                if (value < ReferenceLoopHandling.Error || value > ReferenceLoopHandling.Serialize)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _referenceLoopHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how missing members (e.g. KeyValue contains a property that isn't a member on the object) are handled during deserialization.
        /// </summary>
        public virtual MissingMemberHandling MissingMemberHandling
        {
            get { return _missingMemberHandling; }
            set
            {
                if (value < MissingMemberHandling.Ignore || value > MissingMemberHandling.Error)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _missingMemberHandling = value;
            }
        }
        
        /// <summary>
        /// Gets or sets how default values are handled during serialization and deserialization.
        /// </summary>
        public virtual DefaultValueHandling DefaultValueHandling
        {
            get { return _defaultValueHandling; }
            set
            {
                if (value < DefaultValueHandling.Include || value > DefaultValueHandling.IgnoreAndPopulate)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _defaultValueHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how objects are created during deserialization.
        /// </summary>
        /// <value>The object creation handling.</value>
        public virtual ObjectCreationHandling ObjectCreationHandling
        {
            get { return _objectCreationHandling; }
            set
            {
                if (value < ObjectCreationHandling.Auto || value > ObjectCreationHandling.Replace)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _objectCreationHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how constructors are used during deserialization.
        /// </summary>
        /// <value>The constructor handling.</value>
        public virtual ConstructorHandling ConstructorHandling
        {
            get { return _constructorHandling; }
            set
            {
                if (value < ConstructorHandling.Default || value > ConstructorHandling.AllowNonPublicDefaultConstructor)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _constructorHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how metadata properties are used during deserialization.
        /// </summary>
        /// <value>The metadata properties handling.</value>
        public virtual MetadataPropertyHandling MetadataPropertyHandling
        {
            get { return _metadataPropertyHandling; }
            set
            {
                if (value < MetadataPropertyHandling.Default || value > MetadataPropertyHandling.Ignore)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _metadataPropertyHandling = value;
            }
        }

        /// <summary>
        /// Gets a collection <see cref="KeyValueConverter"/> that will be used during serialization.
        /// </summary>
        /// <value>Collection <see cref="KeyValueConverter"/> that will be used during serialization.</value>
        public virtual KeyValueConverterCollection Converters
        {
            get
            {
                if (_converters == null)
                {
                    _converters = new KeyValueConverterCollection();
                }

                return _converters;
            }
        }

        /// <summary>
        /// Gets or sets the contract resolver used by the serializer when
        /// serializing .NET objects to KeyValue and vice versa.
        /// </summary>
        public virtual IContractResolver ContractResolver
        {
            get { return _contractResolver; }
            set { _contractResolver = value ?? DefaultContractResolver.Instance; }
        }

        /// <summary>
        /// Gets or sets the <see cref="StreamingContext"/> used by the serializer when invoking serialization callback methods.
        /// </summary>
        /// <value>The context.</value>
        public virtual StreamingContext Context
        {
            get { return _context; }
            set { _context = value; }
        }

        /// <summary>
        /// Indicates how KeyValue text output is formatted.
        /// </summary>
        public virtual Formatting Formatting
        {
            get { return _formatting ?? KeyValueSerializerSettings.DefaultFormatting; }
            set { _formatting = value; }
        }

        /// <summary>
        /// Gets or sets how floating point numbers, e.g. 1.0 and 9.9, are parsed when reading KeyValue text.
        /// </summary>
        public virtual FloatParseHandling FloatParseHandling
        {
            get { return _floatParseHandling ?? KeyValueSerializerSettings.DefaultFloatParseHandling; }
            set { _floatParseHandling = value; }
        }

        /// <summary>
        /// Gets or sets how special floating point numbers, e.g. <see cref="Double.NaN"/>,
        /// <see cref="Double.PositiveInfinity"/> and <see cref="Double.NegativeInfinity"/>,
        /// are written as KeyValue text.
        /// </summary>
        public virtual FloatFormatHandling FloatFormatHandling
        {
            get { return _floatFormatHandling ?? KeyValueSerializerSettings.DefaultFloatFormatHandling; }
            set { _floatFormatHandling = value; }
        }

        /// <summary>
        /// Gets or sets how strings are escaped when writing KeyValue text.
        /// </summary>
        public virtual StringEscapeHandling StringEscapeHandling
        {
            get { return _stringEscapeHandling ?? KeyValueSerializerSettings.DefaultStringEscapeHandling; }
            set { _stringEscapeHandling = value; }
        }

        /// <summary>
        /// Gets or sets how <see cref="DateTime"/> and <see cref="DateTimeOffset"/> values are formatted when writing KeyValue text,
        /// and the expected date format when reading KeyValue text.
        /// </summary>
        public virtual string DateFormatString
        {
            get { return _dateFormatString ?? KeyValueSerializerSettings.DefaultDateFormatString; }
            set
            {
                _dateFormatString = value;
                _dateFormatStringSet = true;
            }
        }

        /// <summary>
        /// Gets or sets the culture used when reading KeyValue. Defaults to <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        public virtual CultureInfo Culture
        {
            get { return _culture ?? KeyValueSerializerSettings.DefaultCulture; }
            set { _culture = value; }
        }

        /// <summary>
        /// Gets or sets the maximum depth allowed when reading KeyValue. Reading past this depth will throw a <see cref="KeyValueReaderException"/>.
        /// </summary>
        public virtual int? MaxDepth
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
        /// Gets a value indicating whether there will be a check for additional KeyValue content after deserializing an object.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if there will be a check for additional KeyValue content after deserializing an object; otherwise, <c>false</c>.
        /// </value>
        public virtual bool CheckAdditionalContent
        {
            get { return _checkAdditionalContent ?? KeyValueSerializerSettings.DefaultCheckAdditionalContent; }
            set { _checkAdditionalContent = value; }
        }

        internal bool IsCheckAdditionalContentSet()
        {
            return (_checkAdditionalContent != null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueSerializer"/> class.
        /// </summary>
        public KeyValueSerializer()
        {
            _referenceLoopHandling = KeyValueSerializerSettings.DefaultReferenceLoopHandling;
            _missingMemberHandling = KeyValueSerializerSettings.DefaultMissingMemberHandling;
            _defaultValueHandling = KeyValueSerializerSettings.DefaultDefaultValueHandling;
            _objectCreationHandling = KeyValueSerializerSettings.DefaultObjectCreationHandling;
            _constructorHandling = KeyValueSerializerSettings.DefaultConstructorHandling;
            _typeNameHandling = KeyValueSerializerSettings.DefaultTypeNameHandling;
            _metadataPropertyHandling = KeyValueSerializerSettings.DefaultMetadataPropertyHandling;
            _context = KeyValueSerializerSettings.DefaultContext;
            _serializationBinder = DefaultSerializationBinder.Instance;

            _culture = KeyValueSerializerSettings.DefaultCulture;
            _contractResolver = DefaultContractResolver.Instance;
        }

        /// <summary>
        /// Creates a new <see cref="KeyValueSerializer"/> instance.
        /// The <see cref="KeyValueSerializer"/> will not use default settings 
        /// from <see cref="KeyValueConvert.DefaultSettings"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="KeyValueSerializer"/> instance.
        /// The <see cref="KeyValueSerializer"/> will not use default settings 
        /// from <see cref="KeyValueConvert.DefaultSettings"/>.
        /// </returns>
        public static KeyValueSerializer Create()
        {
            return new KeyValueSerializer();
        }

        /// <summary>
        /// Creates a new <see cref="KeyValueSerializer"/> instance using the specified <see cref="KeyValueSerializerSettings"/>.
        /// The <see cref="KeyValueSerializer"/> will not use default settings 
        /// from <see cref="KeyValueConvert.DefaultSettings"/>.
        /// </summary>
        /// <param name="settings">The settings to be applied to the <see cref="KeyValueSerializer"/>.</param>
        /// <returns>
        /// A new <see cref="KeyValueSerializer"/> instance using the specified <see cref="KeyValueSerializerSettings"/>.
        /// The <see cref="KeyValueSerializer"/> will not use default settings 
        /// from <see cref="KeyValueConvert.DefaultSettings"/>.
        /// </returns>
        public static KeyValueSerializer Create(KeyValueSerializerSettings settings)
        {
            KeyValueSerializer serializer = Create();

            if (settings != null)
            {
                ApplySerializerSettings(serializer, settings);
            }

            return serializer;
        }

        /// <summary>
        /// Creates a new <see cref="KeyValueSerializer"/> instance.
        /// The <see cref="KeyValueSerializer"/> will use default settings 
        /// from <see cref="KeyValueConvert.DefaultSettings"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="KeyValueSerializer"/> instance.
        /// The <see cref="KeyValueSerializer"/> will use default settings 
        /// from <see cref="KeyValueConvert.DefaultSettings"/>.
        /// </returns>
        public static KeyValueSerializer CreateDefault()
        {
            // copy static to local variable to avoid concurrency issues
            KeyValueSerializerSettings defaultSettings = KeyValueConvert.DefaultSettings?.Invoke();

            return Create(defaultSettings);
        }

        /// <summary>
        /// Creates a new <see cref="KeyValueSerializer"/> instance using the specified <see cref="KeyValueSerializerSettings"/>.
        /// The <see cref="KeyValueSerializer"/> will use default settings 
        /// from <see cref="KeyValueConvert.DefaultSettings"/> as well as the specified <see cref="KeyValueSerializerSettings"/>.
        /// </summary>
        /// <param name="settings">The settings to be applied to the <see cref="KeyValueSerializer"/>.</param>
        /// <returns>
        /// A new <see cref="KeyValueSerializer"/> instance using the specified <see cref="KeyValueSerializerSettings"/>.
        /// The <see cref="KeyValueSerializer"/> will use default settings 
        /// from <see cref="KeyValueConvert.DefaultSettings"/> as well as the specified <see cref="KeyValueSerializerSettings"/>.
        /// </returns>
        public static KeyValueSerializer CreateDefault(KeyValueSerializerSettings settings)
        {
            KeyValueSerializer serializer = CreateDefault();
            if (settings != null)
            {
                ApplySerializerSettings(serializer, settings);
            }

            return serializer;
        }

        private static void ApplySerializerSettings(KeyValueSerializer serializer, KeyValueSerializerSettings settings)
        {
            if (!CollectionUtils.IsNullOrEmpty(settings.Converters))
            {
                // insert settings converters at the beginning so they take precedence
                // if user wants to remove one of the default converters they will have to do it manually
                for (int i = 0; i < settings.Converters.Count; i++)
                {
                    serializer.Converters.Insert(i, settings.Converters[i]);
                }
            }

            // serializer specific
            if (settings._typeNameHandling != null)
            {
                serializer.TypeNameHandling = settings.TypeNameHandling;
            }
            if (settings._metadataPropertyHandling != null)
            {
                serializer.MetadataPropertyHandling = settings.MetadataPropertyHandling;
            }
            if (settings._typeNameAssemblyFormatHandling != null)
            {
                serializer.TypeNameAssemblyFormatHandling = settings.TypeNameAssemblyFormatHandling;
            }
            if (settings._referenceLoopHandling != null)
            {
                serializer.ReferenceLoopHandling = settings.ReferenceLoopHandling;
            }
            if (settings._missingMemberHandling != null)
            {
                serializer.MissingMemberHandling = settings.MissingMemberHandling;
            }
            if (settings._objectCreationHandling != null)
            {
                serializer.ObjectCreationHandling = settings.ObjectCreationHandling;
            }
            if (settings._defaultValueHandling != null)
            {
                serializer.DefaultValueHandling = settings.DefaultValueHandling;
            }
            if (settings._constructorHandling != null)
            {
                serializer.ConstructorHandling = settings.ConstructorHandling;
            }
            if (settings._context != null)
            {
                serializer.Context = settings.Context;
            }
            if (settings._checkAdditionalContent != null)
            {
                serializer._checkAdditionalContent = settings._checkAdditionalContent;
            }

            if (settings.Error != null)
            {
                serializer.Error += settings.Error;
            }

            if (settings.ContractResolver != null)
            {
                serializer.ContractResolver = settings.ContractResolver;
            }
            if (settings.TraceWriter != null)
            {
                serializer.TraceWriter = settings.TraceWriter;
            }
            if (settings.EqualityComparer != null)
            {
                serializer.EqualityComparer = settings.EqualityComparer;
            }
            if (settings.SerializationBinder != null)
            {
                serializer.SerializationBinder = settings.SerializationBinder;
            }

            // reader/writer specific
            // unset values won't override reader/writer set values
            if (settings._formatting != null)
            {
                serializer._formatting = settings._formatting;
            }
            if (settings._dateFormatStringSet)
            {
                serializer._dateFormatString = settings._dateFormatString;
                serializer._dateFormatStringSet = settings._dateFormatStringSet;
            }
            if (settings._floatFormatHandling != null)
            {
                serializer._floatFormatHandling = settings._floatFormatHandling;
            }
            if (settings._floatParseHandling != null)
            {
                serializer._floatParseHandling = settings._floatParseHandling;
            }
            if (settings._stringEscapeHandling != null)
            {
                serializer._stringEscapeHandling = settings._stringEscapeHandling;
            }
            if (settings._culture != null)
            {
                serializer._culture = settings._culture;
            }
            if (settings._maxDepthSet)
            {
                serializer._maxDepth = settings._maxDepth;
                serializer._maxDepthSet = settings._maxDepthSet;
            }
        }

        /// <summary>
        /// Populates the KeyValue values onto the target object.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> that contains the KeyValue structure to reader values from.</param>
        /// <param name="target">The target object to populate values onto.</param>
        public void Populate(TextReader reader, object target)
        {
            Populate(new KeyValueTextReader(reader), target);
        }

        /// <summary>
        /// Populates the KeyValue values onto the target object.
        /// </summary>
        /// <param name="reader">The <see cref="KeyValueReader"/> that contains the KeyValue structure to reader values from.</param>
        /// <param name="target">The target object to populate values onto.</param>
        public void Populate(KeyValueReader reader, object target)
        {
            PopulateInternal(reader, target);
        }

        internal virtual void PopulateInternal(KeyValueReader reader, object target)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));
            ValidationUtils.ArgumentNotNull(target, nameof(target));

            // set serialization options onto reader
            SetupReader(reader, out CultureInfo previousCulture, out FloatParseHandling? previousFloatParseHandling, out int? previousMaxDepth, out string previousDateFormatString);

            TraceKeyValueReader traceKeyValueReader = (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
                ? new TraceKeyValueReader(reader)
                : null;

            KeyValueSerializerInternalReader serializerReader = new KeyValueSerializerInternalReader(this);
            serializerReader.Populate(traceKeyValueReader ?? reader, target);

            if (traceKeyValueReader != null)
            {
                TraceWriter.Trace(TraceLevel.Verbose, traceKeyValueReader.GetDeserializedKeyValueMessage(), null);
            }

            ResetReader(reader, previousCulture, previousFloatParseHandling, previousMaxDepth, previousDateFormatString);
        }

        /// <summary>
        /// Deserializes the KeyValue structure contained by the specified <see cref="KeyValueReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="KeyValueReader"/> that contains the KeyValue structure to deserialize.</param>
        /// <returns>The <see cref="Object"/> being deserialized.</returns>
        public object Deserialize(KeyValueReader reader)
        {
            return Deserialize(reader, null);
        }

        /// <summary>
        /// Deserializes the KeyValue structure contained by the specified <see cref="StringReader"/>
        /// into an instance of the specified type.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> containing the object.</param>
        /// <param name="objectType">The <see cref="Type"/> of object being deserialized.</param>
        /// <returns>The instance of <paramref name="objectType"/> being deserialized.</returns>
        public object Deserialize(TextReader reader, Type objectType)
        {
            return Deserialize(new KeyValueTextReader(reader), objectType);
        }

        /// <summary>
        /// Deserializes the KeyValue structure contained by the specified <see cref="KeyValueReader"/>
        /// into an instance of the specified type.
        /// </summary>
        /// <param name="reader">The <see cref="KeyValueReader"/> containing the object.</param>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <returns>The instance of <typeparamref name="T"/> being deserialized.</returns>
        public T Deserialize<T>(KeyValueReader reader)
        {
            return (T)Deserialize(reader, typeof(T));
        }

        /// <summary>
        /// Deserializes the KeyValue structure contained by the specified <see cref="KeyValueReader"/>
        /// into an instance of the specified type.
        /// </summary>
        /// <param name="reader">The <see cref="KeyValueReader"/> containing the object.</param>
        /// <param name="objectType">The <see cref="Type"/> of object being deserialized.</param>
        /// <returns>The instance of <paramref name="objectType"/> being deserialized.</returns>
        public object Deserialize(KeyValueReader reader, Type objectType)
        {
            return DeserializeInternal(reader, objectType);
        }

        internal virtual object DeserializeInternal(KeyValueReader reader, Type objectType)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            // set serialization options onto reader
            CultureInfo previousCulture;
            FloatParseHandling? previousFloatParseHandling;
            int? previousMaxDepth;
            string previousDateFormatString;
            SetupReader(reader, out previousCulture, out previousFloatParseHandling, out previousMaxDepth, out previousDateFormatString);

            TraceKeyValueReader traceKeyValueReader = (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
                ? new TraceKeyValueReader(reader)
                : null;

            KeyValueSerializerInternalReader serializerReader = new KeyValueSerializerInternalReader(this);
            object value = serializerReader.Deserialize(traceKeyValueReader ?? reader, objectType, CheckAdditionalContent);

            if (traceKeyValueReader != null)
            {
                TraceWriter.Trace(TraceLevel.Verbose, traceKeyValueReader.GetDeserializedKeyValueMessage(), null);
            }

            ResetReader(reader, previousCulture, previousFloatParseHandling, previousMaxDepth, previousDateFormatString);

            return value;
        }

        private void SetupReader(KeyValueReader reader, out CultureInfo previousCulture, out FloatParseHandling? previousFloatParseHandling, out int? previousMaxDepth, out string previousDateFormatString)
        {
            if (_culture != null && !_culture.Equals(reader.Culture))
            {
                previousCulture = reader.Culture;
                reader.Culture = _culture;
            }
            else
            {
                previousCulture = null;
            }

            if (_floatParseHandling != null && reader.FloatParseHandling != _floatParseHandling)
            {
                previousFloatParseHandling = reader.FloatParseHandling;
                reader.FloatParseHandling = _floatParseHandling.GetValueOrDefault();
            }
            else
            {
                previousFloatParseHandling = null;
            }

            if (_maxDepthSet && reader.MaxDepth != _maxDepth)
            {
                previousMaxDepth = reader.MaxDepth;
                reader.MaxDepth = _maxDepth;
            }
            else
            {
                previousMaxDepth = null;
            }

            if (_dateFormatStringSet && reader.DateFormatString != _dateFormatString)
            {
                previousDateFormatString = reader.DateFormatString;
                reader.DateFormatString = _dateFormatString;
            }
            else
            {
                previousDateFormatString = null;
            }

            KeyValueTextReader textReader = reader as KeyValueTextReader;
            if (textReader != null)
            {
                DefaultContractResolver resolver = _contractResolver as DefaultContractResolver;
                if (resolver != null)
                {
                    textReader.NameTable = resolver.GetNameTable();
                }
            }
        }

        private void ResetReader(KeyValueReader reader, CultureInfo previousCulture, FloatParseHandling? previousFloatParseHandling, int? previousMaxDepth, string previousDateFormatString)
        {
            // reset reader back to previous options
            if (previousCulture != null)
            {
                reader.Culture = previousCulture;
            }
            if (previousFloatParseHandling != null)
            {
                reader.FloatParseHandling = previousFloatParseHandling.GetValueOrDefault();
            }
            if (_maxDepthSet)
            {
                reader.MaxDepth = previousMaxDepth;
            }
            if (_dateFormatStringSet)
            {
                reader.DateFormatString = previousDateFormatString;
            }

            KeyValueTextReader textReader = reader as KeyValueTextReader;
            if (textReader != null)
            {
                textReader.NameTable = null;
            }
        }

        /// <summary>
        /// Serializes the specified <see cref="Object"/> and writes the KeyValue structure
        /// using the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> used to write the KeyValue structure.</param>
        /// <param name="value">The <see cref="Object"/> to serialize.</param>
        public void Serialize(TextWriter textWriter, object value)
        {
            Serialize(new KeyValueTextWriter(textWriter), value);
        }

        /// <summary>
        /// Serializes the specified <see cref="Object"/> and writes the KeyValue structure
        /// using the specified <see cref="KeyValueWriter"/>.
        /// </summary>
        /// <param name="KeyValueWriter">The <see cref="KeyValueWriter"/> used to write the KeyValue structure.</param>
        /// <param name="value">The <see cref="Object"/> to serialize.</param>
        /// <param name="objectType">
        /// The type of the value being serialized.
        /// This parameter is used when <see cref="KeyValueSerializer.TypeNameHandling"/> is <see cref="TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.
        /// </param>
        public void Serialize(KeyValueWriter KeyValueWriter, object value, Type objectType)
        {
            SerializeInternal(KeyValueWriter, value, objectType);
        }

        /// <summary>
        /// Serializes the specified <see cref="Object"/> and writes the KeyValue structure
        /// using the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> used to write the KeyValue structure.</param>
        /// <param name="value">The <see cref="Object"/> to serialize.</param>
        /// <param name="objectType">
        /// The type of the value being serialized.
        /// This parameter is used when <see cref="TypeNameHandling"/> is Auto to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.
        /// </param>
        public void Serialize(TextWriter textWriter, object value, Type objectType)
        {
            Serialize(new KeyValueTextWriter(textWriter), value, objectType);
        }

        /// <summary>
        /// Serializes the specified <see cref="Object"/> and writes the KeyValue structure
        /// using the specified <see cref="KeyValueWriter"/>.
        /// </summary>
        /// <param name="KeyValueWriter">The <see cref="KeyValueWriter"/> used to write the KeyValue structure.</param>
        /// <param name="value">The <see cref="Object"/> to serialize.</param>
        public void Serialize(KeyValueWriter KeyValueWriter, object value)
        {
            SerializeInternal(KeyValueWriter, value, null);
        }

        internal virtual void SerializeInternal(KeyValueWriter KeyValueWriter, object value, Type objectType)
        {
            ValidationUtils.ArgumentNotNull(KeyValueWriter, nameof(KeyValueWriter));

            // set serialization options onto writer
            Formatting? previousFormatting = null;
            if (_formatting != null && KeyValueWriter.Formatting != _formatting)
            {
                previousFormatting = KeyValueWriter.Formatting;
                KeyValueWriter.Formatting = _formatting.GetValueOrDefault();
            }

            FloatFormatHandling? previousFloatFormatHandling = null;
            if (_floatFormatHandling != null && KeyValueWriter.FloatFormatHandling != _floatFormatHandling)
            {
                previousFloatFormatHandling = KeyValueWriter.FloatFormatHandling;
                KeyValueWriter.FloatFormatHandling = _floatFormatHandling.GetValueOrDefault();
            }

            StringEscapeHandling? previousStringEscapeHandling = null;
            if (_stringEscapeHandling != null && KeyValueWriter.StringEscapeHandling != _stringEscapeHandling)
            {
                previousStringEscapeHandling = KeyValueWriter.StringEscapeHandling;
                KeyValueWriter.StringEscapeHandling = _stringEscapeHandling.GetValueOrDefault();
            }

            CultureInfo previousCulture = null;
            if (_culture != null && !_culture.Equals(KeyValueWriter.Culture))
            {
                previousCulture = KeyValueWriter.Culture;
                KeyValueWriter.Culture = _culture;
            }

            string previousDateFormatString = null;
            if (_dateFormatStringSet && KeyValueWriter.DateFormatString != _dateFormatString)
            {
                previousDateFormatString = KeyValueWriter.DateFormatString;
                KeyValueWriter.DateFormatString = _dateFormatString;
            }

            TraceKeyValueWriter traceKeyValueWriter = (TraceWriter != null && TraceWriter.LevelFilter >= TraceLevel.Verbose)
                ? new TraceKeyValueWriter(KeyValueWriter)
                : null;

            KeyValueSerializerInternalWriter serializerWriter = new KeyValueSerializerInternalWriter(this);
            serializerWriter.Serialize(traceKeyValueWriter ?? KeyValueWriter, value, objectType);

            if (traceKeyValueWriter != null)
            {
                TraceWriter.Trace(TraceLevel.Verbose, traceKeyValueWriter.GetSerializedKeyValueMessage(), null);
            }

            // reset writer back to previous options
            if (previousFormatting != null)
            {
                KeyValueWriter.Formatting = previousFormatting.GetValueOrDefault();
            }
            if (previousFloatFormatHandling != null)
            {
                KeyValueWriter.FloatFormatHandling = previousFloatFormatHandling.GetValueOrDefault();
            }
            if (previousStringEscapeHandling != null)
            {
                KeyValueWriter.StringEscapeHandling = previousStringEscapeHandling.GetValueOrDefault();
            }
            if (_dateFormatStringSet)
            {
                KeyValueWriter.DateFormatString = previousDateFormatString;
            }
            if (previousCulture != null)
            {
                KeyValueWriter.Culture = previousCulture;
            }
        }
        
        internal KeyValueConverter GetMatchingConverter(Type type)
        {
            return GetMatchingConverter(_converters, type);
        }

        internal static KeyValueConverter GetMatchingConverter(IList<KeyValueConverter> converters, Type objectType)
        {
#if DEBUG
            ValidationUtils.ArgumentNotNull(objectType, nameof(objectType));
#endif

            if (converters != null)
            {
                for (int i = 0; i < converters.Count; i++)
                {
                    KeyValueConverter converter = converters[i];

                    if (converter.CanConvert(objectType))
                    {
                        return converter;
                    }
                }
            }

            return null;
        }

        internal void OnError(Serialization.ErrorEventArgs e)
        {
            Error?.Invoke(this, e);
        }
    }
}
