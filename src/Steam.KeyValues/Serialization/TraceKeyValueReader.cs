using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;

namespace Steam.KeyValues.Serialization
{
    internal class TraceKeyValueReader : KeyValueReader, IKeyValueLineInfo
    {
        private readonly KeyValueReader _innerReader;
        private readonly KeyValueTextWriter _textWriter;
        private readonly StringWriter _sw;

        public TraceKeyValueReader(KeyValueReader innerReader)
        {
            _innerReader = innerReader;

            _sw = new StringWriter(CultureInfo.InvariantCulture);
            // prefix the message in the stringwriter to avoid concat with a potentially large KeyValue string
            _sw.Write("Deserialized KeyValue: " + Environment.NewLine);

            _textWriter = new KeyValueTextWriter(_sw);
            _textWriter.Formatting = Formatting.Indented;
        }

        public string GetDeserializedKeyValueMessage()
        {
            return _sw.ToString();
        }

        protected override bool ReadInternal()
        {
            bool value = _innerReader.Read();
            _textWriter.WriteToken(_innerReader, false, false, true);
            return value;
        }

        public override int? ReadAsInt32()
        {
            int? value = _innerReader.ReadAsInt32();
            _textWriter.WriteToken(_innerReader, false, false, true);
            return value;
        }

        public override string ReadAsString()
        {
            string value = _innerReader.ReadAsString();
            _textWriter.WriteToken(_innerReader, false, false, true);
            return value;
        }

        public override decimal? ReadAsDecimal()
        {
            decimal? value = _innerReader.ReadAsDecimal();
            _textWriter.WriteToken(_innerReader, false, false, true);
            return value;
        }

        public override double? ReadAsDouble()
        {
            double? value = _innerReader.ReadAsDouble();
            _textWriter.WriteToken(_innerReader, false, false, true);
            return value;
        }

        public override Color? ReadAsColor()
        {
            Color? value = _innerReader.ReadAsColor();
            _textWriter.WriteToken(_innerReader, false, false, true);
            return value;
        }

        public override int Depth
        {
            get { return _innerReader.Depth; }
        }

        public override string Path
        {
            get { return _innerReader.Path; }
        }

        public override char QuoteChar
        {
            get { return _innerReader.QuoteChar; }
            protected internal set { _innerReader.QuoteChar = value; }
        }

        public override KeyValueToken TokenType
        {
            get { return _innerReader.TokenType; }
        }

        public override object Value
        {
            get { return _innerReader.Value; }
        }

        public override Type ValueType
        {
            get { return _innerReader.ValueType; }
        }

        public override void Close()
        {
            _innerReader.Close();
        }

        bool IKeyValueLineInfo.HasLineInfo()
        {
            IKeyValueLineInfo lineInfo = _innerReader as IKeyValueLineInfo;
            return lineInfo != null && lineInfo.HasLineInfo();
        }

        int IKeyValueLineInfo.LineNumber
        {
            get
            {
                IKeyValueLineInfo lineInfo = _innerReader as IKeyValueLineInfo;
                return (lineInfo != null) ? lineInfo.LineNumber : 0;
            }
        }

        int IKeyValueLineInfo.LinePosition
        {
            get
            {
                IKeyValueLineInfo lineInfo = _innerReader as IKeyValueLineInfo;
                return (lineInfo != null) ? lineInfo.LinePosition : 0;
            }
        }
    }
}
