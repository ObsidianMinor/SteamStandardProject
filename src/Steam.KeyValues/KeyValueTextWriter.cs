using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Steam.KeyValues.Utilities;

namespace Steam.KeyValues
{
    public class KeyValueTextWriter : KeyValueWriter
    {
        private const int IndentCharBufferSize = 12;
        private readonly TextWriter _writer;
        private char _indentChar;
        private int _indentation;
        private char _quoteChar;
        private bool _quoteName;
        private bool[] _charEscapeFlags;
        private char[] _writeBuffer;
        private IArrayPool<char> _arrayPool;
        private char[] _indentChars;

        /// <summary>
        /// Gets or sets the writer's character array pool.
        /// </summary>
        public IArrayPool<char> ArrayPool
        {
            get { return _arrayPool; }
            set
            {
                _arrayPool = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets or sets how many <see cref="KeyValueTextWriter.IndentChar"/>s to write for each level in the hierarchy when <see cref="KeyValueWriter.Formatting"/> is set to <see cref="Formatting.Indented"/>.
        /// </summary>
        public int Indentation
        {
            get { return _indentation; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Indentation value must be greater than 0.");
                }

                _indentation = value;
            }
        }

        /// <summary>
        /// Gets or sets which character to use to quote attribute values.
        /// </summary>
        public char QuoteChar => '"';

        /// <summary>
        /// Gets or sets which character to use for indenting when <see cref="KeyValueWriter.Formatting"/> is set to <see cref="Formatting.Indented"/>.
        /// </summary>
        public char IndentChar
        {
            get { return _indentChar; }
            set
            {
                if (value != _indentChar)
                {
                    _indentChar = value;
                    _indentChars = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether object names will be surrounded with quotes.
        /// </summary>
        public bool QuoteName
        {
            get { return _quoteName; }
            set { _quoteName = value; }
        }

        public override void WriteBase()
        {
            InternalWritePropertyName("#base");

            _writer.Write("#base");
        }

        public override void WriteStartObject()
        {
            InternalWriteStart(KeyValueToken.Start, KeyValueContainerType.Object);

            _writer.Write('{');
        }

        public override void WriteEndObject()
        {
            InternalWriteEnd(KeyValueContainerType.Object);

            _writer.Write('}');
        }

        public override void WriteValue(string value)
        {
            InternalWriteValue(KeyValueToken.String);

            WriteValueString(value, KeyValueToken.String);
        }

        public override void WriteValue(int value)
        {
            InternalWriteValue(KeyValueToken.Int32);

            WriteValueString(value.ToString(), KeyValueToken.Int32);
        }

        public override void WriteValue(IntPtr value)
        {
            InternalWriteValue(KeyValueToken.Pointer);

            WriteValueString(value.ToInt32().ToString(), KeyValueToken.Pointer);
        }

        [CLSCompliant(false)]
        public override void WriteValue(UIntPtr value)
        {
            InternalWriteValue(KeyValueToken.Pointer);

            WriteValueString(value.ToUInt32().ToString(), KeyValueToken.Pointer);
        }

        public override void WriteValue(byte value)
        {
            InternalWriteValue(KeyValueToken.Int32);

            WriteValueString(value.ToString(), KeyValueToken.Int32);
        }

        public override void WriteValue(char value)
        {
            InternalWriteValue(KeyValueToken.String);

            WriteValueString(value.ToString(), KeyValueToken.String);
        }

        [CLSCompliant(false)]
        public override void WriteValue(uint value)
        {
            InternalWriteValue(KeyValueToken.Int64);

            WriteValueString(value.ToString(), KeyValueToken.Int64);
        }

        public override void WriteValue(long value)
        {
            InternalWriteValue(KeyValueToken.Int64);

            WriteValueString(value.ToString(), KeyValueToken.Int64);
        }

        public override void WriteValue(ulong value)
        {
            InternalWriteValue(KeyValueToken.UInt64);

            WriteValueString(value.ToString(), KeyValueToken.UInt64);
        }

        public override void WriteValue(float value)
        {
            InternalWriteValue(KeyValueToken.Float32);

            WriteValueString(value.ToString(), KeyValueToken.Float32);
        }

        public override void WriteValue(double value)
        {
            InternalWriteValue(KeyValueToken.Float32);

            WriteValueString(value.ToString(), KeyValueToken.Float32);
        }

        public override void WriteValue(decimal value)
        {
            InternalWriteValue(KeyValueToken.Float32);

            WriteValueString(value.ToString(), KeyValueToken.Float32);
        }

        public override void WriteValue(short value)
        {
            InternalWriteValue(KeyValueToken.Int32);

            WriteValueString(value.ToString(), KeyValueToken.Int32);
        }

        [CLSCompliant(false)]
        public override void WriteValue(ushort value)
        {
            InternalWriteValue(KeyValueToken.Int32);

            WriteValueString(value.ToString(), KeyValueToken.Int32);
        }

        [CLSCompliant(false)]
        public override void WriteValue(sbyte value)
        {
            InternalWriteValue(KeyValueToken.Int32);

            WriteValueString(value.ToString(), KeyValueToken.Int32);
        }

        protected override void WriteValueDelimiter()
        {
            _writer.Write('\t');
        }

        protected override void WriteIndent()
        {
            _writer.Write('\t');
        }

        protected override void WriteIndentSpace()
        {
            _writer.Write(' ');
        }

        public override void WriteWhitespace(string ws)
        {
            InternalWriteWhitespace(ws);
            _writer.Write(ws);
        }

        public override void WriteConditional(Conditional conditional)
        {
            if (conditional == 0)
                return;

            InternalWriteConditional();
            
            _writer.Write('[');

            bool first = true;
            foreach (var condition in conditional.Seperate())
            {
                if (!first)
                    _writer.Write("||");
                else
                    first = false;

                if(condition.IsNot())
                    _writer.Write('!');

                _writer.Write('$');
                _writer.Write(condition.ToOriginalString());
            }

            _writer.Write(']');
        }

        public override void WriteValue(Color value)
        {
            InternalWriteValue(KeyValueToken.Color);
            WriteValueString($"{value.R} {value.G} {value.B} {value.A}", KeyValueToken.Color);
        }

        public override void WriteComment(string text)
        {
            InternalWriteComment();
            _writer.WriteLine(text);
        }

        public override void WriteWideString(string value)
        {
            InternalWriteValue(KeyValueToken.WideString);
            WriteValueString(value, KeyValueToken.WideString);
        }

        /// <summary>
        /// Writes a string value to the text writer
        /// </summary>
        /// <param name="value"></param>
        /// <param name="token">The token type of the</param>
        /// <remarks>
        /// I included this for easy access to the write function of the value. 
        /// The default text format writes two quote characters around the string but some formats don't (for instance, Steam UI files)
        /// With a simple override, the end user can make a writer that can write text for other formats
        /// </remarks>
        protected virtual void WriteValueString(string value, KeyValueToken token)
        {
            _writer.Write('"');
            _writer.Write(value);
            _writer.Write('"');
        }
    }
}
