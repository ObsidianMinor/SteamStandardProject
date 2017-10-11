using Steam.KeyValues.Serialization;
using Steam.KeyValues.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

namespace Steam.KeyValues
{
    /// <summary>
    /// Represents a reader that provides fast, non-cached, forward-only access to serialized KeyValue data.
    /// </summary>
    public abstract class KeyValueReader : IDisposable
    {
        /// <summary>
        /// Specifies the state of the reader.
        /// </summary>
        protected internal enum State
        {
            /// <summary>
            /// A <see cref="KeyValueReader"/> read method has not been called.
            /// </summary>
            Start,
            
            /// <summary>
            /// The end of the file has been reached successfully.
            /// </summary>
            Complete,

            /// <summary>
            /// Reader is at a property.
            /// </summary>
            Property,

            /// <summary>
            /// Reader is at the start of an object.
            /// </summary>
            ObjectStart,

            /// <summary>
            /// Reader is in an object.
            /// </summary>
            Object,

            /// <summary>
            /// The <see cref="Close()"/> method has been called.
            /// </summary>
            Closed,
            
            /// <summary>
            /// Reader has just read a value.
            /// </summary>
            PostValue,

            /// <summary>
            /// An error occurred that prevents the read operation from continuing.
            /// </summary>
            Error,

            /// <summary>
            /// The end of the file has been reached successfully.
            /// </summary>
            Finished
        }

        // current Token data
        private KeyValueToken _tokenType;
        private object _value;
        internal char _quoteChar;
        internal State _currentState;
        private KeyValuePosition _currentPosition;
        private CultureInfo _culture;
        private int? _maxDepth;
        private bool _hasExceededMaxDepth;
        internal FloatParseHandling _floatParseHandling;
        private string _dateFormatString;
        private List<KeyValuePosition> _stack;
        private bool _skip;

        /// <summary>
        /// Gets the current reader state.
        /// </summary>
        protected State CurrentState => _currentState;

        /// <summary>
        /// Gets or sets a value indicating whether the source should be closed when this reader is closed.
        /// </summary>
        /// <value>
        /// <c>true</c> to close the source when this reader is closed; otherwise <c>false</c>. The default is <c>true</c>.
        /// </value>
        public bool CloseInput { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether multiple pieces of JSON content can
        /// be read from a continuous stream without erroring.
        /// </summary>
        /// <value>
        /// <c>true</c> to support reading multiple pieces of JSON content; otherwise <c>false</c>.
        /// The default is <c>false</c>.
        /// </value>
        public bool SupportMultipleContent { get; set; }

        /// <summary>
        /// Gets the quotation mark character used to enclose the value of a string.
        /// </summary>
        public virtual char QuoteChar
        {
            get { return _quoteChar; }
            protected internal set { _quoteChar = value; }
        }
        
        /// <summary>
        /// Gets or sets how floating point numbers, e.g. 1.0 and 9.9, are parsed when reading JSON text.
        /// </summary>
        public FloatParseHandling FloatParseHandling
        {
            get { return _floatParseHandling; }
            set
            {
                if (value < FloatParseHandling.Double || value > FloatParseHandling.Decimal)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _floatParseHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how custom date formatted strings are parsed when reading JSON.
        /// </summary>
        public string DateFormatString
        {
            get { return _dateFormatString; }
            set { _dateFormatString = value; }
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
            }
        }

        /// <summary>
        /// Gets the type of the current KeyValue token. 
        /// </summary>
        public virtual KeyValueToken TokenType
        {
            get { return _tokenType; }
        }

        /// <summary>
        /// Gets the text value of the current JSON token.
        /// </summary>
        public virtual object Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets the .NET type for the current JSON token.
        /// </summary>
        public virtual Type ValueType
        {
            get { return _value?.GetType(); }
        }

        /// <summary>
        /// Gets the depth of the current token in the JSON document.
        /// </summary>
        /// <value>The depth of the current token in the JSON document.</value>
        public virtual int Depth
        {
            get
            {
                int depth = (_stack != null) ? _stack.Count : 0;
                if (KeyValueTokenUtils.IsStartToken(TokenType) || _currentPosition.Type == KeyValueContainerType.None)
                {
                    return depth;
                }
                else
                {
                    return depth + 1;
                }
            }
        }

        /// <summary>
        /// Gets the path of the current JSON token. 
        /// </summary>
        public virtual string Path
        {
            get
            {
                if (_currentPosition.Type == KeyValueContainerType.None)
                {
                    return string.Empty;
                }

                bool insideContainer = _currentState != State.ObjectStart;

                KeyValuePosition? current = insideContainer ? (KeyValuePosition?)_currentPosition : null;

                return KeyValuePosition.BuildPath(_stack, current);
            }
        }

        /// <summary>
        /// Gets or sets the culture used when reading JSON. Defaults to <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        public CultureInfo Culture
        {
            get { return _culture ?? CultureInfo.InvariantCulture; }
            set { _culture = value; }
        }

        internal KeyValuePosition GetPosition(int depth)
        {
            if (_stack != null && depth < _stack.Count)
            {
                return _stack[depth];
            }

            return _currentPosition;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueReader"/> class.
        /// </summary>
        protected KeyValueReader()
        {
            _currentState = State.Start;
            _floatParseHandling = FloatParseHandling.Double;

            CloseInput = true;
        }

        private void Push(KeyValueContainerType value)
        {
            UpdateScopeWithFinishedValue();

            if (_currentPosition.Type == KeyValueContainerType.None)
            {
                _currentPosition = new KeyValuePosition(value);
            }
            else
            {
                if (_stack == null)
                {
                    _stack = new List<KeyValuePosition>();
                }

                _stack.Add(_currentPosition);
                _currentPosition = new KeyValuePosition(value);

                // this is a little hacky because Depth increases when first property/value is written but only testing here is faster/simpler
                if (_maxDepth != null && Depth + 1 > _maxDepth && !_hasExceededMaxDepth)
                {
                    _hasExceededMaxDepth = true;
                    throw KeyValueReaderException.Create(this, "The reader's MaxDepth of {0} has been exceeded.".FormatWith(CultureInfo.InvariantCulture, _maxDepth));
                }
            }
        }

        private KeyValueContainerType Pop()
        {
            KeyValuePosition oldPosition;
            if (_stack != null && _stack.Count > 0)
            {
                oldPosition = _currentPosition;
                _currentPosition = _stack[_stack.Count - 1];
                _stack.RemoveAt(_stack.Count - 1);
            }
            else
            {
                oldPosition = _currentPosition;
                _currentPosition = new KeyValuePosition();
            }

            if (_maxDepth != null && Depth <= _maxDepth)
            {
                _hasExceededMaxDepth = false;
            }

            return oldPosition.Type;
        }

        private KeyValueContainerType Peek()
        {
            return _currentPosition.Type;
        }

        /// <summary>
        /// Reads the next KeyValue token from the source.
        /// </summary>
        /// <returns><c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.</returns>
        public bool Read() // lazy way to read ahead
        {
            if (_skip)
            {
                _skip = false;
                return true;
            }
            else
            {
                return ReadInternal();
            }
        }

        /// <summary>
        /// Reads the next KeyValue token from the source.
        /// </summary>
        /// <returns><c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.</returns>
        protected abstract bool ReadInternal();

        /// <summary>
        /// Reads the next KeyValue token from the source as a base token
        /// </summary>
        /// <returns>A <see cref="String"/></returns>
        public virtual string ReadAsBase()
        {
            KeyValueToken t = GetContentToken();
            
            if(t == KeyValueToken.Base)
                return (string)Value;

            throw KeyValueReaderException.Create(this, "Error reading base. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
        }

        /// <summary>
        /// Reads the next KeyValue token from the source as a <see cref="Nullable{T}"/> of <see cref="Int32"/>.
        /// </summary>
        public virtual int? ReadAsInt32()
        {
            KeyValueToken t = GetContentToken();

            switch(t)
            {
                case KeyValueToken.Int32:
                case KeyValueToken.Float32:
                case KeyValueToken.Int64:
                case KeyValueToken.Pointer:
                case KeyValueToken.UInt64:
                    if (!(Value is int))
                    {
                        SetToken(KeyValueToken.Int32, Convert.ToInt32(Value, CultureInfo.InvariantCulture), false);
                    }

                    return (int)Value;
                case KeyValueToken.String:
                    string s = (string)Value;
                    return ReadInt32String(s);
            }

            throw KeyValueReaderException.Create(this, "Error reading integer. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
        }

        internal int? ReadInt32String(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                SetToken(KeyValueToken.String, "", false);
                return null;
            }

            if (int.TryParse(s, NumberStyles.Integer, Culture, out int i))
            {
                SetToken(KeyValueToken.Int32, i, false);
                return i;
            }
            else
            {
                SetToken(KeyValueToken.String, s, false);
                throw KeyValueReaderException.Create(this, "Could not convert string to integer: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
            }
        }

        /// <summary>
        /// Reads the next KeyValue token from the source as a <see cref="Nullable{T}"/> of <see cref="UIntPtr"/>
        /// </summary>
        /// <returns></returns>
        [CLSCompliant(false)]
        public virtual UIntPtr? ReadAsPointer()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads the next KeyValue token from the source as a <see cref="Nullable{T}"/> of <see cref="long"/>
        /// </summary>
        /// <returns></returns>
        public virtual long? ReadAsInt64()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads the next KeyValue token from the source as a <see cref="Nullable{T}"/> of <see cref="ulong"/>
        /// </summary>
        /// <returns></returns>
        [CLSCompliant(false)]
        public virtual ulong? ReadAsUInt64()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads the next KeyValue token from the source as a <see cref="Nullable{T}"/> of <see cref="decimal"/>
        /// </summary>
        /// <returns></returns>
        public virtual decimal? ReadAsDecimal()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads the next KeyValue token from the source as a <see cref="Nullable{T}"/> of <see cref="double"/>
        /// </summary>
        /// <returns></returns>
        public virtual double? ReadAsDouble()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads the next KeyValue token from the source as a <see cref="Nullable{T}"/> of <see cref="float"/>
        /// </summary>
        /// <returns></returns>
        public virtual float? ReadAsFloat()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads the next KeyValue token from the source as a <see cref="Nullable{T}"/> of <see cref="Color"/>
        /// </summary>
        /// <returns></returns>
        public virtual Color? ReadAsColor()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads the next KeyValue token from the source as a collection of <see cref="Conditional"/>s
        /// </summary>
        public virtual Conditional? ReadAsConditions()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads the next KeyValue token from the source as a <see cref="String"/>.
        /// </summary>
        /// <returns>A <see cref="String"/></returns>
        public virtual string ReadAsString()
        {
            KeyValueToken t = GetContentToken();

            switch (t)
            {
                case KeyValueToken.String:
                    return (string)Value;
            }

            if (KeyValueTokenUtils.IsPrimitiveToken(t))
            {
                if (Value != null)
                {
                    string s;
                    if (Value is IFormattable formattable)
                    {
                        s = formattable.ToString(null, Culture);
                    }
                    else
                    {
                        s = Value.ToString();
                    }

                    SetToken(KeyValueToken.String, s, false);
                    return s;
                }
            }

            throw KeyValueReaderException.Create(this, "Error reading string. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, t));
        }

        /// <summary>
        /// Skips the children of the current token
        /// </summary>
        public void Skip()
        {
            if (TokenType == KeyValueToken.PropertyName)
            {
                Read();

                _skip = true;
            }
            
            if (TokenType == KeyValueToken.Conditional)
            {
                Read();

                int depth = Depth;

                while (Read() && (depth < Depth))
                {
                }
            }
            else
            {
                Read();

                if(TokenType == KeyValueToken.Conditional)
                {
                    Read();
                }

                _skip = true;
            }
        }

        /// <summary>
        /// Sets the current token.
        /// </summary>
        /// <param name="newToken">The new token.</param>
        protected void SetToken(KeyValueToken newToken)
        {
            SetToken(newToken, null, true);
        }

        /// <summary>
        /// Sets the current token and value.
        /// </summary>
        /// <param name="newToken">The new token.</param>
        /// <param name="value">The value.</param>
        protected void SetToken(KeyValueToken newToken, object value)
        {
            SetToken(newToken, value, true);
        }

        /// <summary>
        /// Sets the current token and value.
        /// </summary>
        /// <param name="newToken">The new token.</param>
        /// <param name="value">The value.</param>
        /// <param name="updateIndex">A flag indicating whether the position index inside an array should be updated.</param>
        protected void SetToken(KeyValueToken newToken, object value, bool updateIndex)
        {
            _tokenType = newToken;
            _value = value;

            switch(newToken)
            {
                case KeyValueToken.Start:
                    _currentState = State.ObjectStart;
                    Push(KeyValueContainerType.Object);
                    break;
                case KeyValueToken.End:
                    ValidateEnd();
                    break;
                case KeyValueToken.PropertyName:
                    _currentState = State.Property;
                    _currentPosition.PropertyName = (string)value;
                    break;
                case KeyValueToken.Color:
                case KeyValueToken.Float32:
                case KeyValueToken.Int32:
                case KeyValueToken.Int64:
                case KeyValueToken.Pointer:
                case KeyValueToken.String:
                case KeyValueToken.UInt64:
                case KeyValueToken.WideString:
                    SetPostValueState(updateIndex);
                    break;
            }
        }

        internal void SetPostValueState(bool updateIndex)
        {
            if(Peek() != KeyValueContainerType.None)
            {
                _currentState = State.PostValue;
            }
            else
            {
                SetFinished();
            }

            if (updateIndex)
                UpdateScopeWithFinishedValue();
        }
        
        private void UpdateScopeWithFinishedValue()
        {
            if (_currentPosition.HasIndex)
            {
                _currentPosition.Position++;
            }
        }

        private void ValidateEnd(KeyValueToken endToken)
        {
            KeyValueContainerType currentObject = Pop();

            if (GetTypeForCloseToken(endToken) != currentObject)
            {
                throw KeyValueReaderException.Create(this, "JsonToken {0} is not valid for closing JsonType {1}.".FormatWith(CultureInfo.InvariantCulture, endToken, currentObject));
            }

            if (Peek() != KeyValueContainerType.None)
            {
                _currentState = State.PostValue;
            }
            else
            {
                SetFinished();
            }
        }

        /// <summary>
        /// Sets the state based on current token type.
        /// </summary>
        protected void SetStateBasedOnCurrent()
        {
            KeyValueContainerType currentObject = Peek();

            switch (currentObject)
            {
                case KeyValueContainerType.Object:
                    _currentState = State.Object;
                    break;
                case KeyValueContainerType.None:
                    SetFinished();
                    break;
                default:
                    throw KeyValueReaderException.Create(this, "While setting the reader state back to current object an unexpected JsonType was encountered: {0}".FormatWith(CultureInfo.InvariantCulture, currentObject));
            }
        }

        private void SetFinished()
        {
            if (SupportMultipleContent)
            {
                _currentState = State.Start;
            }
            else
            {
                _currentState = State.Finished;
            }
        }

        private KeyValueContainerType GetTypeForCloseToken(KeyValueToken token)
        {
            switch (token)
            {
                case KeyValueToken.End:
                    return KeyValueContainerType.Object;
                default:
                    throw KeyValueReaderException.Create(this, "Not a valid close JsonToken: {0}".FormatWith(CultureInfo.InvariantCulture, token));
            }
        }
        
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_currentState != State.Closed && disposing)
            {
                Close();
            }
        }

        /// <summary>
        /// Changes the reader's state to <see cref="State.Closed"/>.
        /// If <see cref="CloseInput"/> is set to <c>true</c>, the source is also closed.
        /// </summary>
        public virtual void Close()
        {
            _currentState = State.Closed;
            _tokenType = KeyValueToken.Start;
            _value = null;
        }
        
        internal void ReadAndAssert()
        {
            if (!Read())
            {
                throw KeyValueSerializationException.Create(this, "Unexpected end when reading JSON.");
            }
        }

        internal void ReadForTypeAndAssert(KeyValueContract contract, bool hasConverter)
        {
            if (!ReadForType(contract, hasConverter))
            {
                throw KeyValueSerializationException.Create(this, "Unexpected end when reading JSON.");
            }
        }

        internal bool ReadForType(KeyValueContract contract, bool hasConverter)
        {
            // don't read properties with converters as a specific value
            // the value might be a string which will then get converted which will error if read as date for example
            if (hasConverter)
            {
                return Read();
            }

            ReadType t = (contract != null) ? contract.InternalReadType : ReadType.Read;

            switch (t)
            {
                case ReadType.Read:
                    return ReadAndMoveToContent();
                case ReadType.ReadAsInt32:
                    ReadAsInt32();
                    break;
                case ReadType.ReadAsDecimal:
                    ReadAsDecimal();
                    break;
                case ReadType.ReadAsDouble:
                    ReadAsDouble();
                    break;
                case ReadType.ReadAsInt64:
                    ReadAsInt64();
                    break;
                case ReadType.ReadAsUInt64:
                    ReadAsUInt64();
                    break;
                case ReadType.ReadAsString:
                    ReadAsString();
                    break;
                case ReadType.ReadAsPointer:
                    ReadAsPointer();
                    break;
                case ReadType.ReadAsColor:
                    ReadAsColor();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return (TokenType != KeyValueToken.None);
        }

        internal bool ReadAndMoveToContent()
        {
            return Read() && MoveToContent();
        }

        internal bool MoveToContent()
        {
            KeyValueToken t = TokenType;
            while (t == KeyValueToken.None || t == KeyValueToken.Comment)
            {
                if (!Read())
                {
                    return false;
                }

                t = TokenType;
            }

            return true;
        }
        
        private void ValidateEnd()
        {
            Pop();

            if (Peek() != KeyValueContainerType.None)
                _currentState = State.PostValue;
            else
                SetFinished();
        }

        private KeyValueToken GetContentToken()
        {
            KeyValueToken t;
            do
            {
                if (!Read())
                {
                    SetToken(KeyValueToken.Start);
                    return KeyValueToken.Start;
                }
                else
                {
                    t = TokenType;
                }
            } while (t == KeyValueToken.Comment);

            return t;
        }
    }
}
