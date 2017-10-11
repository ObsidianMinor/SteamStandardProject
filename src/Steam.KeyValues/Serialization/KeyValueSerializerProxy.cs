using Steam.KeyValues.Utilities;
using System;
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;

namespace Steam.KeyValues.Serialization
{
    internal class KeyValueSerializerProxy : KeyValueSerializer
    {
        private readonly KeyValueSerializerInternalReader _serializerReader;
        private readonly KeyValueSerializerInternalWriter _serializerWriter;
        private readonly KeyValueSerializer _serializer;

        public override event EventHandler<ErrorEventArgs> Error
        {
            add { _serializer.Error += value; }
            remove { _serializer.Error -= value; }
        }
        
        public override ITraceWriter TraceWriter
        {
            get { return _serializer.TraceWriter; }
            set { _serializer.TraceWriter = value; }
        }

        public override IEqualityComparer EqualityComparer
        {
            get { return _serializer.EqualityComparer; }
            set { _serializer.EqualityComparer = value; }
        }

        public override KeyValueConverterCollection Converters
        {
            get { return _serializer.Converters; }
        }

        public override DefaultValueHandling DefaultValueHandling
        {
            get { return _serializer.DefaultValueHandling; }
            set { _serializer.DefaultValueHandling = value; }
        }

        public override IContractResolver ContractResolver
        {
            get { return _serializer.ContractResolver; }
            set { _serializer.ContractResolver = value; }
        }

        public override MissingMemberHandling MissingMemberHandling
        {
            get { return _serializer.MissingMemberHandling; }
            set { _serializer.MissingMemberHandling = value; }
        }
        
        public override ObjectCreationHandling ObjectCreationHandling
        {
            get { return _serializer.ObjectCreationHandling; }
            set { _serializer.ObjectCreationHandling = value; }
        }

        public override ReferenceLoopHandling ReferenceLoopHandling
        {
            get { return _serializer.ReferenceLoopHandling; }
            set { _serializer.ReferenceLoopHandling = value; }
        }
        
        public override TypeNameHandling TypeNameHandling
        {
            get { return _serializer.TypeNameHandling; }
            set { _serializer.TypeNameHandling = value; }
        }

        public override MetadataPropertyHandling MetadataPropertyHandling
        {
            get { return _serializer.MetadataPropertyHandling; }
            set { _serializer.MetadataPropertyHandling = value; }
        }

        [Obsolete("TypeNameAssemblyFormat is obsolete. Use TypeNameAssemblyFormatHandling instead.")]
        public override FormatterAssemblyStyle TypeNameAssemblyFormat
        {
            get { return _serializer.TypeNameAssemblyFormat; }
            set { _serializer.TypeNameAssemblyFormat = value; }
        }

        public override TypeNameAssemblyFormatHandling TypeNameAssemblyFormatHandling
        {
            get { return _serializer.TypeNameAssemblyFormatHandling; }
            set { _serializer.TypeNameAssemblyFormatHandling = value; }
        }

        public override ConstructorHandling ConstructorHandling
        {
            get { return _serializer.ConstructorHandling; }
            set { _serializer.ConstructorHandling = value; }
        }

        [Obsolete("Binder is obsolete. Use SerializationBinder instead.")]
        public override SerializationBinder Binder
        {
            get { return _serializer.Binder; }
            set { _serializer.Binder = value; }
        }

        public override ISerializationBinder SerializationBinder
        {
            get { return _serializer.SerializationBinder; }
            set { _serializer.SerializationBinder = value; }
        }

        public override StreamingContext Context
        {
            get { return _serializer.Context; }
            set { _serializer.Context = value; }
        }

        public override Formatting Formatting
        {
            get { return _serializer.Formatting; }
            set { _serializer.Formatting = value; }
        }
        
        public override FloatFormatHandling FloatFormatHandling
        {
            get { return _serializer.FloatFormatHandling; }
            set { _serializer.FloatFormatHandling = value; }
        }

        public override FloatParseHandling FloatParseHandling
        {
            get { return _serializer.FloatParseHandling; }
            set { _serializer.FloatParseHandling = value; }
        }

        public override StringEscapeHandling StringEscapeHandling
        {
            get { return _serializer.StringEscapeHandling; }
            set { _serializer.StringEscapeHandling = value; }
        }

        public override string DateFormatString
        {
            get { return _serializer.DateFormatString; }
            set { _serializer.DateFormatString = value; }
        }

        public override CultureInfo Culture
        {
            get { return _serializer.Culture; }
            set { _serializer.Culture = value; }
        }

        public override int? MaxDepth
        {
            get { return _serializer.MaxDepth; }
            set { _serializer.MaxDepth = value; }
        }

        public override bool CheckAdditionalContent
        {
            get { return _serializer.CheckAdditionalContent; }
            set { _serializer.CheckAdditionalContent = value; }
        }

        internal KeyValueSerializerInternalBase GetInternalSerializer()
        {
            if (_serializerReader != null)
            {
                return _serializerReader;
            }
            else
            {
                return _serializerWriter;
            }
        }

        public KeyValueSerializerProxy(KeyValueSerializerInternalReader serializerReader)
        {
            ValidationUtils.ArgumentNotNull(serializerReader, nameof(serializerReader));

            _serializerReader = serializerReader;
            _serializer = serializerReader.Serializer;
        }

        public KeyValueSerializerProxy(KeyValueSerializerInternalWriter serializerWriter)
        {
            ValidationUtils.ArgumentNotNull(serializerWriter, nameof(serializerWriter));

            _serializerWriter = serializerWriter;
            _serializer = serializerWriter.Serializer;
        }

        internal override object DeserializeInternal(KeyValueReader reader, Type objectType)
        {
            if (_serializerReader != null)
            {
                return _serializerReader.Deserialize(reader, objectType, false);
            }
            else
            {
                return _serializer.Deserialize(reader, objectType);
            }
        }

        internal override void PopulateInternal(KeyValueReader reader, object target)
        {
            if (_serializerReader != null)
            {
                _serializerReader.Populate(reader, target);
            }
            else
            {
                _serializer.Populate(reader, target);
            }
        }

        internal override void SerializeInternal(KeyValueWriter jsonWriter, object value, Type rootType)
        {
            if (_serializerWriter != null)
            {
                _serializerWriter.Serialize(jsonWriter, value, rootType);
            }
            else
            {
                _serializer.Serialize(jsonWriter, value);
            }
        }
    }
}
