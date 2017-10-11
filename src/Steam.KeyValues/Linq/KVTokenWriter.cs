using Steam.KeyValues.Utilities;
using System;
using System.Globalization;
using System.Drawing;

namespace Steam.KeyValues.Linq
{
    /// <summary>
    /// Represents a writer that provides a fast, non-cached, forward-only way of generating JSON data.
    /// </summary>
    public partial class KVTokenWriter : KeyValueWriter
    {
        private KVContainer _token;
        private KVContainer _parent;
        // used when writer is writing single value and the value has no containing parent
        private KVValue _value;
        private KVToken _current;

        /// <summary>
        /// Gets the <see cref="KVToken"/> at the writer's current position.
        /// </summary>
        public KVToken CurrentToken
        {
            get { return _current; }
        }

        /// <summary>
        /// Gets the token being written.
        /// </summary>
        /// <value>The token being written.</value>
        public KVToken Token
        {
            get
            {
                if (_token != null)
                {
                    return _token;
                }

                return _value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KVTokenWriter"/> class writing to the given <see cref="KVContainer"/>.
        /// </summary>
        /// <param name="container">The container being written to.</param>
        public KVTokenWriter(KVContainer container)
        {
            ValidationUtils.ArgumentNotNull(container, nameof(container));

            _token = container;
            _parent = container;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KVTokenWriter"/> class.
        /// </summary>
        public KVTokenWriter()
        {
        }

        /// <summary>
        /// Flushes whatever is in the buffer to the underlying <see cref="KVContainer"/>.
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// Closes this writer.
        /// If <see cref="KeyValueWriter.AutoCompleteOnClose"/> is set to <c>true</c>, the KeyValue is auto-completed.
        /// </summary>
        /// <remarks>
        /// Setting <see cref="KeyValueWriter.CloseOutput"/> to <c>true</c> has no additional effect, since the underlying <see cref="KVContainer"/> is a type that cannot be closed.
        /// </remarks>
        public override void Close()
        {
            base.Close();
        }

        /// <summary>
        /// Writes the beginning of a KeyValue object.
        /// </summary>
        public override void WriteStartObject()
        {
            base.WriteStartObject();

            AddParent(new KVObject());
        }

        private void AddParent(KVContainer container)
        {
            if (_parent == null)
            {
                _token = container;
            }
            else
            {
                _parent.AddAndSkipParentCheck(container);
            }

            _parent = container;
            _current = container;
        }

        private void RemoveParent()
        {
            _current = _parent;
            _parent = _parent.Parent;

            if (_parent != null && _parent.Type == KVTokenType.Property)
            {
                _parent = _parent.Parent;
            }
        }

        /// <summary>
        /// Writes the end.
        /// </summary>
        /// <param name="token">The token.</param>
        protected override void WriteEnd(KeyValueToken token)
        {
            RemoveParent();
        }

        /// <summary>
        /// Writes the property name of a name/value pair on a KeyValue object.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        public override void WritePropertyName(string name)
        {
            // avoid duplicate property name exception
            // last property name wins
            (_parent as KVObject)?.Remove(name); // todo: check if existing property has the same conditional

            AddParent(new KVProperty(name));

            // don't set state until after in case of an error
            // incorrect state will cause issues if writer is disposed when closing open properties
            base.WritePropertyName(name);
        }

        private void AddValue(object value, KeyValueToken token)
        {
            AddValue(new KVValue(value), token);
        }

        internal void AddValue(KVValue value, KeyValueToken token)
        {
            if (_parent != null)
            {
                _parent.Add(value);
                _current = _parent.Last;

                if (_parent.Type == KVTokenType.Property)
                {
                    _parent = _parent.Parent;
                }
            }
            else
            {
                _value = value ?? KVValue.CreateString("");
                _current = _value;
            }
        }

        #region WriteValue methods
        /// <summary>
        /// Writes a <see cref="Object"/> value.
        /// An error will be raised if the value cannot be written as a single KeyValue token.
        /// </summary>
        /// <param name="value">The <see cref="Object"/> value to write.</param>
        public override void WriteValue(object value)
        {
            base.WriteValue(value);
        }

        /// <summary>
        /// Writes raw KeyValue.
        /// </summary>
        /// <param name="KeyValue">The raw KeyValue to write.</param>
        public override void WriteRaw(string KeyValue)
        {
            base.WriteRaw(KeyValue);
            AddValue(new KVRaw(KeyValue), KeyValueToken.Raw);
        }

        /// <summary>
        /// Writes a comment <c>/*...*/</c> containing the specified text.
        /// </summary>
        /// <param name="text">Text to place inside the comment.</param>
        public override void WriteComment(string text)
        {
            base.WriteComment(text);
            AddValue(KVValue.CreateComment(text), KeyValueToken.Comment);
        }

        /// <summary>
        /// Writes a <see cref="String"/> value.
        /// </summary>
        /// <param name="value">The <see cref="String"/> value to write.</param>
        public override void WriteValue(string value)
        {
            base.WriteValue(value);
            AddValue(value, KeyValueToken.String);
        }

        /// <summary>
        /// Writes a <see cref="Int32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int32"/> value to write.</param>
        public override void WriteValue(int value)
        {
            base.WriteValue(value);
            AddValue(value, KeyValueToken.Int32);
        }

        /// <summary>
        /// Writes a <see cref="UInt32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt32"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(uint value)
        {
            base.WriteValue(value);
            AddValue(value, KeyValueToken.Int64);
        }

        /// <summary>
        /// Writes a <see cref="Int64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int64"/> value to write.</param>
        public override void WriteValue(long value)
        {
            base.WriteValue(value);
            AddValue(value, KeyValueToken.Int64);
        }

        /// <summary>
        /// Writes a <see cref="UInt64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt64"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(ulong value)
        {
            base.WriteValue(value);
            AddValue(value, KeyValueToken.UInt64);
        }

        /// <summary>
        /// Writes a <see cref="Single"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Single"/> value to write.</param>
        public override void WriteValue(float value)
        {
            base.WriteValue(value);
            AddValue(value, KeyValueToken.Float32);
        }

        /// <summary>
        /// Writes a <see cref="Double"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Double"/> value to write.</param>
        public override void WriteValue(double value)
        {
            base.WriteValue(value);
            AddValue(value, KeyValueToken.Float32);
        }
        
        /// <summary>
        /// Writes a <see cref="Int16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int16"/> value to write.</param>
        public override void WriteValue(short value)
        {
            base.WriteValue(value);
            AddValue(value, KeyValueToken.Int32);
        }

        /// <summary>
        /// Writes a <see cref="UInt16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt16"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(ushort value)
        {
            base.WriteValue(value);
            AddValue(value, KeyValueToken.Int32);
        }

        /// <summary>
        /// Writes a <see cref="Char"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Char"/> value to write.</param>
        public override void WriteValue(char value)
        {
            base.WriteValue(value);
            string s = null;
            s = value.ToString(CultureInfo.InvariantCulture);
            AddValue(s, KeyValueToken.String);
        }

        /// <summary>
        /// Writes a <see cref="Byte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Byte"/> value to write.</param>
        public override void WriteValue(byte value)
        {
            base.WriteValue(value);
            AddValue(value, KeyValueToken.Int32);
        }

        /// <summary>
        /// Writes a <see cref="SByte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="SByte"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(sbyte value)
        {
            base.WriteValue(value);
            AddValue(value, KeyValueToken.Int32);
        }

        /// <summary>
        /// Writes a <see cref="Decimal"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Decimal"/> value to write.</param>
        public override void WriteValue(decimal value)
        {
            base.WriteValue(value);
            AddValue(value, KeyValueToken.Float32);
        }

        /// <summary>
        /// Writes a <see cref="Color"/> value.
        /// </summary>
        /// <param name="value"></param>
        public override void WriteValue(Color value)
        {
            base.WriteValue(value);
            AddValue(value, KeyValueToken.Color);
        }

        /// <summary>
        /// Writes a <see cref="IntPtr"/> value.
        /// </summary>
        /// <param name="value"></param>
        public override void WriteValue(IntPtr value)
        {
            base.WriteValue(value);
            AddValue(value, KeyValueToken.Pointer);
        }

        /// <summary>
        /// Writes a <see cref="UIntPtr"/> value.
        /// </summary>
        /// <param name="value"></param>
        [CLSCompliant(false)]
        public override void WriteValue(UIntPtr value)
        {
            base.WriteValue(value);
            AddValue(value, KeyValueToken.Pointer);
        }

        /// <summary>
        /// Writes a <see cref="string"/> value as a <see cref="KeyValueToken.WideString"/> token.
        /// </summary>
        /// <param name="value"></param>
        public override void WriteWideString(string value)
        {
            base.WriteWideString(value);
            AddValue(value, KeyValueToken.WideString);
        }

        #endregion

        internal override void WriteToken(KeyValueReader reader, bool writeChildren, bool writeComments)
        {
            // cloning the token rather than reading then writing it doesn't lose some type information, e.g. Guid, byte[], etc
            if (reader is KVTokenReader tokenReader && writeChildren && writeComments)
            {
                if (tokenReader.TokenType == KeyValueToken.None)
                {
                    if (!tokenReader.Read())
                    {
                        return;
                    }
                }

                KVToken value = tokenReader.CurrentToken.CloneToken();

                if (_parent != null)
                {
                    _parent.Add(value);
                    _current = _parent.Last;

                    // if the writer was in a property then move out of it and up to its parent object
                    if (_parent.Type == KVTokenType.Property)
                    {
                        _parent = _parent.Parent;
                        InternalWriteValue(KeyValueToken.String);
                    }
                }
                else
                {
                    _current = value;

                    if (_token == null && _value == null)
                    {
                        _token = value as KVContainer;
                        _value = value as KVValue;
                    }
                }

                tokenReader.Skip();
            }
            else
            {
                base.WriteToken(reader, writeChildren, writeComments);
            }
        }
    }
}
