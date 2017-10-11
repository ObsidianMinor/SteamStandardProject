using Steam.KeyValues.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace Steam.KeyValues
{
    /// <summary>
    /// Represents a writer that provides a fast, non-cached, forward-only way of generating KeyValue data.
    /// </summary>
    public abstract partial class KeyValueWriter : IDisposable
    {
        internal enum State
        {
            Start = 0,
            Property = 1,
            ObjectStart = 2,
            Object = 3,
            Conditional = 4,
            Closed = 5,
            Error = 6
        }

        // array that gives a new state based on the current state an the token being written
        private static readonly State[][] StateArray;

        internal static readonly State[][] StateArrayTempate = new[]
        {
            //                                      Start                    PropertyName            ObjectStart         Object            Closed       Error
            //
            /* None                        */new[] { State.Error,            State.Error,            State.Error,        State.Error,      State.Error, State.Error },
            /* StartObject                 */new[] { State.Error,            State.ObjectStart,      State.Error,        State.Error,      State.Error, State.Error },
            /* Property                    */new[] { State.Property,         State.Error,            State.Property,     State.Property,   State.Error, State.Error },
            /* Conditional                 */new[] { State.Error,            State.ObjectStart,      State.Error,        State.Error,      State.Error, State.Error },
            /* Comment                     */new[] { State.Start,            State.Property,         State.ObjectStart,  State.Object,     State.Error, State.Error },
            /* Raw                         */new[] { State.Start,            State.Property,         State.ObjectStart,  State.Object,     State.Error, State.Error },
            /* Value (this will be copied) */new[] { State.Error,            State.Object,           State.Error,        State.Error,      State.Error, State.Error }
        };

        internal static State[][] BuildStateArray()
        {
            List<State[]> allStates = StateArrayTempate.ToList();
            State[] errorStates = StateArrayTempate[0];
            State[] valueStates = StateArrayTempate[7];

            foreach (KeyValueToken valueToken in EnumUtils.GetValues<KeyValueToken>())
            {
                if (allStates.Count <= (int)valueToken)
                {
                    switch (valueToken)
                    {
                        case KeyValueToken.Int32:
                        case KeyValueToken.Float32:
                        case KeyValueToken.String:
                        case KeyValueToken.Int64:
                        case KeyValueToken.UInt64:
                        case KeyValueToken.WideString:
                        case KeyValueToken.Pointer:
                        case KeyValueToken.Color:
                            allStates.Add(valueStates);
                            break;
                        default:
                            allStates.Add(errorStates);
                            break;
                    }
                }
            }

            return allStates.ToArray();
        }

        static KeyValueWriter()
        {
            StateArray = BuildStateArray();
        }

        private List<KeyValuePosition> _stack;
        private KeyValuePosition _currentPosition;
        private State _currentState;
        private Formatting _formatting;

        /// <summary>
        /// Gets or sets a value indicating whether the destination should be closed when this writer is closed.
        /// </summary>
        /// <value>
        /// <c>true</c> to close the destination when this writer is closed; otherwise <c>false</c>. The default is <c>true</c>.
        /// </value>
        public bool CloseOutput { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the KeyValue should be auto-completed when this writer is closed.
        /// </summary>
        /// <value>
        /// <c>true</c> to auto-complete the KeyValue when this writer is closed; otherwise <c>false</c>. The default is <c>true</c>.
        /// </value>
        public bool AutoCompleteOnClose { get; set; }

        /// <summary>
        /// Gets the top.
        /// </summary>
        /// <value>The top.</value>
        protected internal int Top
        {
            get
            {
                int depth = (_stack != null) ? _stack.Count : 0;
                if (Peek() != KeyValueContainerType.None)
                {
                    depth++;
                }

                return depth;
            }
        }

        /// <summary>
        /// Gets the state of the writer.
        /// </summary>
        public WriteState WriteState
        {
            get
            {
                switch (_currentState)
                {
                    case State.Error:
                        return WriteState.Error;
                    case State.Closed:
                        return WriteState.Closed;
                    case State.Object:
                    case State.ObjectStart:
                        return WriteState.Object;
                    case State.Property:
                        return WriteState.Property;
                    case State.Start:
                        return WriteState.Start;
                    default:
                        throw KeyValueWriterException.Create(this, "Invalid state: " + _currentState, null);
                }
            }
        }

        internal string ContainerPath
        {
            get
            {
                if (_currentPosition.Type == KeyValueContainerType.None || _stack == null)
                {
                    return string.Empty;
                }

                return KeyValuePosition.BuildPath(_stack, null);
            }
        }

        /// <summary>
        /// Gets the path of the writer. 
        /// </summary>
        public string Path
        {
            get
            {
                if (_currentPosition.Type == KeyValueContainerType.None)
                {
                    return string.Empty;
                }

                bool insideContainer = (_currentState != State.ObjectStart);

                KeyValuePosition? current = insideContainer ? (KeyValuePosition?)_currentPosition : null;

                return KeyValuePosition.BuildPath(_stack, current);
            }
        }
        
        private StringEscapeHandling _stringEscapeHandling;
        private FloatFormatHandling _floatFormatHandling;
        private string _dateFormatString;
        private CultureInfo _culture;

        /// <summary>
        /// Gets or sets a value indicating how KeyValue text output should be formatted.
        /// </summary>
        public Formatting Formatting
        {
            get { return _formatting; }
            set
            {
                if (value < Formatting.None || value > Formatting.Indented)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _formatting = value;
            }
        }

        /// <summary>
        /// Gets or sets how strings are escaped when writing KeyValue text.
        /// </summary>
        public StringEscapeHandling StringEscapeHandling
        {
            get { return _stringEscapeHandling; }
            set
            {
                if (value < StringEscapeHandling.Default || value > StringEscapeHandling.EscapeHtml)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _stringEscapeHandling = value;
                OnStringEscapeHandlingChanged();
            }
        }

        internal virtual void OnStringEscapeHandlingChanged()
        {
            // hacky but there is a calculated value that relies on StringEscapeHandling
        }

        /// <summary>
        /// Gets or sets how special floating point numbers, e.g. <see cref="Double.NaN"/>,
        /// <see cref="Double.PositiveInfinity"/> and <see cref="Double.NegativeInfinity"/>,
        /// are written to KeyValue text.
        /// </summary>
        public FloatFormatHandling FloatFormatHandling
        {
            get { return _floatFormatHandling; }
            set
            {
                if (value < FloatFormatHandling.String || value > FloatFormatHandling.DefaultValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _floatFormatHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how <see cref="DateTime"/> and <see cref="DateTimeOffset"/> values are formatted when writing KeyValue text.
        /// </summary>
        public string DateFormatString
        {
            get { return _dateFormatString; }
            set { _dateFormatString = value; }
        }

        /// <summary>
        /// Gets or sets the culture used when writing KeyValue. Defaults to <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        public CultureInfo Culture
        {
            get { return _culture ?? CultureInfo.InvariantCulture; }
            set { _culture = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueWriter"/> class.
        /// </summary>
        protected KeyValueWriter()
        {
            _currentState = State.Start;
            _formatting = Formatting.None;

            CloseOutput = true;
            AutoCompleteOnClose = true;
        }

        internal void UpdateScopeWithFinishedValue()
        {
            if (_currentPosition.HasIndex)
            {
                _currentPosition.Position++;
            }
        }

        private void Push(KeyValueContainerType value)
        {
            if (_currentPosition.Type != KeyValueContainerType.None)
            {
                if (_stack == null)
                {
                    _stack = new List<KeyValuePosition>();
                }

                _stack.Add(_currentPosition);
            }

            _currentPosition = new KeyValuePosition(value);
        }

        private KeyValueContainerType Pop()
        {
            KeyValuePosition oldPosition = _currentPosition;

            if (_stack != null && _stack.Count > 0)
            {
                _currentPosition = _stack[_stack.Count - 1];
                _stack.RemoveAt(_stack.Count - 1);
            }
            else
            {
                _currentPosition = new KeyValuePosition();
            }

            return oldPosition.Type;
        }

        private KeyValueContainerType Peek()
        {
            return _currentPosition.Type;
        }

        /// <summary>
        /// Flushes whatever is in the buffer to the destination and also flushes the destination.
        /// </summary>
        public abstract void Flush();

        /// <summary>
        /// Closes this writer.
        /// If <see cref="CloseOutput"/> is set to <c>true</c>, the destination is also closed.
        /// If <see cref="AutoCompleteOnClose"/> is set to <c>true</c>, the KeyValue is auto-completed.
        /// </summary>
        public virtual void Close()
        {
            if (AutoCompleteOnClose)
            {
                AutoCompleteAll();
            }
        }

        /// <summary>
        /// Writes the beginning of a KeyValue object.
        /// </summary>
        public virtual void WriteStartObject()
        {
            InternalWriteStart(KeyValueToken.Start, KeyValueContainerType.Object);
        }

        /// <summary>
        /// Writes the end of a KeyValue object.
        /// </summary>
        public virtual void WriteEndObject()
        {
            InternalWriteEnd(KeyValueContainerType.Object);
        }

        /// <summary>
        /// Writes the property name of a name/value pair of a KeyValue object.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        public virtual void WritePropertyName(string name)
        {
            InternalWritePropertyName(name);
        }

        /// <summary>
        /// Writes the property name of a name/value pair of a KeyValue object.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="escape">A flag to indicate whether the text should be escaped when it is written as a KeyValue property name.</param>
        public virtual void WritePropertyName(string name, bool escape)
        {
            WritePropertyName(name);
        }

        /// <summary>
        /// Writes a base string if the writer is in a valid state
        /// </summary>
        public virtual void WriteBase()
        {
            AutoComplete(KeyValueToken.PropertyName);
        }

        /// <summary>
        /// Writes a conditional string for the current property
        /// </summary>
        /// <param name="conditional"></param>
        public virtual void WriteConditional(Conditional conditional)
        {
            AutoComplete(KeyValueToken.Conditional);
        }

        /// <summary>
        /// Writes the end of the current KeyValue object.
        /// </summary>
        public virtual void WriteEnd()
        {
            WriteEnd(Peek());
        }

        /// <summary>
        /// Writes the current <see cref="KeyValueReader"/> token and its children.
        /// </summary>
        /// <param name="reader">The <see cref="KeyValueReader"/> to read the token from.</param>
        public void WriteToken(KeyValueReader reader)
        {
            WriteToken(reader, true);
        }

        /// <summary>
        /// Writes the current <see cref="KeyValueReader"/> token.
        /// </summary>
        /// <param name="reader">The <see cref="KeyValueReader"/> to read the token from.</param>
        /// <param name="writeChildren">A flag indicating whether the current token's children should be written.</param>
        public void WriteToken(KeyValueReader reader, bool writeChildren)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            WriteToken(reader, writeChildren, true);
        }

        /// <summary>
        /// Writes the <see cref="KeyValueToken"/> token and its value.
        /// </summary>
        /// <param name="token">The <see cref="KeyValueToken"/> to write.</param>
        /// <param name="value">
        /// The value to write.
        /// A value is only required for tokens that have an associated value, e.g. the <see cref="String"/> property name for <see cref="KeyValueToken.PropertyName"/>.
        /// <c>null</c> can be passed to the method for tokens that don't have a value, e.g. <see cref="KeyValueToken.Start"/>.
        /// </param>
        public void WriteToken(KeyValueToken token, object value)
        {
            switch (token)
            {
                case KeyValueToken.None:
                    // read to next
                    break;
                case KeyValueToken.Start:
                    WriteStartObject();
                    break;
                case KeyValueToken.PropertyName:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    WritePropertyName(value.ToString());
                    break;
                case KeyValueToken.Comment:
                    WriteComment(value?.ToString());
                    break;
                case KeyValueToken.Int32:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    WriteValue(Convert.ToInt32(value, CultureInfo.InvariantCulture));
                    break;
                case KeyValueToken.Int64:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    WriteValue(Convert.ToInt64(value, CultureInfo.InvariantCulture));
                    break;
                case KeyValueToken.Float32:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    if (value is decimal)
                    {
                        WriteValue((decimal)value);
                    }
                    else if (value is double)
                    {
                        WriteValue((double)value);
                    }
                    else if (value is float)
                    {
                        WriteValue((float)value);
                    }
                    else
                    {
                        WriteValue(Convert.ToDouble(value, CultureInfo.InvariantCulture));
                    }
                    break;
                case KeyValueToken.String:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    WriteValue(value.ToString());
                    break;
                case KeyValueToken.WideString:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    WriteWideString(value.ToString());
                    break;
                case KeyValueToken.UInt64:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    WriteValue(Convert.ToUInt64(value, CultureInfo.InvariantCulture));
                    break;
                case KeyValueToken.Color:
                    ValidationUtils.ArgumentNotNull(value, nameof(value));
                    WriteValue((Color)value);
                    break;
                case KeyValueToken.End:
                    WriteEndObject();
                    break;
                case KeyValueToken.Raw:
                    WriteRawValue(value?.ToString());
                    break;
                case KeyValueToken.Base:
                    WriteBase();
                    break;
                default:
                    throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(token), token, "Unexpected token type.");
            }
        }

        /// <summary>
        /// Writes the <see cref="KeyValueToken"/> token.
        /// </summary>
        /// <param name="token">The <see cref="KeyValueToken"/> to write.</param>
        public void WriteToken(KeyValueToken token)
        {
            WriteToken(token, null);
        }

        internal virtual void WriteToken(KeyValueReader reader, bool writeChildren, bool writeComments)
        {
            int initialDepth = CalculateWriteTokenInitialDepth(reader);

            do
            {
                if (writeComments || reader.TokenType != KeyValueToken.Comment)
                {
                    WriteToken(reader.TokenType, reader.Value);
                }
            } while (
                // stop if we have reached the end of the token being read
                initialDepth - 1 < reader.Depth - (KeyValueTokenUtils.IsEndToken(reader.TokenType) ? 1 : 0)
                && writeChildren
                && reader.Read());

            if (initialDepth < CalculateWriteTokenFinalDepth(reader))
            {
                throw KeyValueWriterException.Create(this, "Unexpected end when reading token.", null);
            }
        }

        private int CalculateWriteTokenInitialDepth(KeyValueReader reader)
        {
            KeyValueToken type = reader.TokenType;
            if (type == KeyValueToken.None)
            {
                return -1;
            }

            return KeyValueTokenUtils.IsStartToken(type) ? reader.Depth : reader.Depth + 1;
        }

        private int CalculateWriteTokenFinalDepth(KeyValueReader reader)
        {
            KeyValueToken type = reader.TokenType;
            if (type == KeyValueToken.None)
            {
                return -1;
            }

            return KeyValueTokenUtils.IsEndToken(type) ? reader.Depth - 1 : reader.Depth;
        }

        private void WriteEnd(KeyValueContainerType type)
        {
            switch (type)
            {
                case KeyValueContainerType.Object:
                    WriteEndObject();
                    break;
                default:
                    throw KeyValueWriterException.Create(this, "Unexpected type when writing end: " + type, null);
            }
        }

        private void AutoCompleteAll()
        {
            while (Top > 0)
            {
                WriteEnd();
            }
        }

        private KeyValueToken GetCloseTokenForType(KeyValueContainerType type)
        {
            switch (type)
            {
                case KeyValueContainerType.Object:
                    return KeyValueToken.End;
                default:
                    throw KeyValueWriterException.Create(this, "No close token for type: " + type, null);
            }
        }

        private void AutoCompleteClose(KeyValueContainerType type)
        {
            int levelsToComplete = CalculateLevelsToComplete(type);

            for (int i = 0; i < levelsToComplete; i++)
            {
                KeyValueToken token = GetCloseTokenForType(Pop());

                if (_currentState == State.Property)
                {
                    WriteValue("");
                }

                if (_formatting == Formatting.Indented)
                {
                    if (_currentState != State.ObjectStart)
                    {
                        WriteIndent();
                    }
                }

                WriteEnd(token);

                UpdateCurrentState();
            }
        }

        private int CalculateLevelsToComplete(KeyValueContainerType type)
        {
            int levelsToComplete = 0;

            if (_currentPosition.Type == type)
            {
                levelsToComplete = 1;
            }
            else
            {
                int top = Top - 2;
                for (int i = top; i >= 0; i--)
                {
                    int currentLevel = top - i;

                    if (_stack[currentLevel].Type == type)
                    {
                        levelsToComplete = i + 2;
                        break;
                    }
                }
            }

            if (levelsToComplete == 0)
            {
                throw KeyValueWriterException.Create(this, "No token to close.", null);
            }

            return levelsToComplete;
        }

        private void UpdateCurrentState()
        {
            KeyValueContainerType currentLevelType = Peek();

            switch (currentLevelType)
            {
                case KeyValueContainerType.Object:
                    _currentState = State.Object;
                    break;
                case KeyValueContainerType.None:
                    _currentState = State.Start;
                    break;
                default:
                    throw KeyValueWriterException.Create(this, "Unknown KeyValueType: " + currentLevelType, null);
            }
        }

        /// <summary>
        /// Writes the specified end token.
        /// </summary>
        /// <param name="token">The end token to write.</param>
        protected virtual void WriteEnd(KeyValueToken token)
        {
        }

        /// <summary>
        /// Writes indent characters.
        /// </summary>
        protected virtual void WriteIndent()
        {
        }

        /// <summary>
        /// Writes the KeyValue value delimiter.
        /// </summary>
        protected virtual void WriteValueDelimiter()
        {
        }

        /// <summary>
        /// Writes an indent space.
        /// </summary>
        protected virtual void WriteIndentSpace()
        {
        }

        internal void AutoComplete(KeyValueToken tokenBeingWritten)
        {
            // gets new state based on the current state and what is being written
            State newState = StateArray[(int)tokenBeingWritten][(int)_currentState];

            if (newState == State.Error)
            {
                throw KeyValueWriterException.Create(this, "Token {0} in state {1} would result in an invalid KeyValue object.".FormatWith(CultureInfo.InvariantCulture, tokenBeingWritten.ToString(), _currentState.ToString()), null);
            }

            if (_currentState == State.Object && tokenBeingWritten != KeyValueToken.Comment)
            {
                WriteValueDelimiter();
            }

            if (_formatting == Formatting.Indented)
            {
                if (_currentState == State.Property)
                {
                    WriteIndentSpace();
                }

                // don't indent a property when it is the first token to be written (i.e. at the start)
                if (tokenBeingWritten == KeyValueToken.PropertyName && _currentState != State.Start)
                {
                    WriteIndent();
                }
            }

            _currentState = newState;
        }

        #region WriteValue methods
        /// <summary>
        /// Writes raw KeyValue without changing the writer's state.
        /// </summary>
        /// <param name="KeyValue">The raw KeyValue to write.</param>
        public virtual void WriteRaw(string KeyValue)
        {
            InternalWriteRaw();
        }

        /// <summary>
        /// Writes raw KeyValue where a value is expected and updates the writer's state.
        /// </summary>
        /// <param name="KeyValue">The raw KeyValue to write.</param>
        public virtual void WriteRawValue(string KeyValue)
        {
            // hack. want writer to change state as if a value had been written
            UpdateScopeWithFinishedValue();
            AutoComplete(KeyValueToken.String);
            WriteRaw(KeyValue);
        }

        /// <summary>
        /// Writes a <see cref="String"/> value.
        /// </summary>
        /// <param name="value">The <see cref="String"/> value to write.</param>
        public virtual void WriteValue(string value)
        {
            InternalWriteValue(KeyValueToken.String);
        }

        /// <summary>
        /// Writes a <see cref="Int32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int32"/> value to write.</param>
        public virtual void WriteValue(int value)
        {
            InternalWriteValue(KeyValueToken.Int32);
        }

        /// <summary>
        /// Writes a <see cref="UInt32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt32"/> value to write.</param>
        [CLSCompliant(false)]
        public virtual void WriteValue(uint value)
        {
            InternalWriteValue(KeyValueToken.Int64);
        }

        /// <summary>
        /// Writes a <see cref="Int64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int64"/> value to write.</param>
        public virtual void WriteValue(long value)
        {
            InternalWriteValue(KeyValueToken.Int64);
        }

        /// <summary>
        /// Writes a <see cref="UInt64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt64"/> value to write.</param>
        [CLSCompliant(false)]
        public virtual void WriteValue(ulong value)
        {
            InternalWriteValue(KeyValueToken.UInt64);
        }

        /// <summary>
        /// Writes a <see cref="Single"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Single"/> value to write.</param>
        public virtual void WriteValue(float value)
        {
            InternalWriteValue(KeyValueToken.Float32);
        }

        /// <summary>
        /// Writes a <see cref="Double"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Double"/> value to write.</param>
        public virtual void WriteValue(double value)
        {
            InternalWriteValue(KeyValueToken.Float32);
        }
        
        /// <summary>
        /// Writes a <see cref="Int16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int16"/> value to write.</param>
        public virtual void WriteValue(short value)
        {
            InternalWriteValue(KeyValueToken.Int32);
        }

        /// <summary>
        /// Writes a <see cref="UInt16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt16"/> value to write.</param>
        [CLSCompliant(false)]
        public virtual void WriteValue(ushort value)
        {
            InternalWriteValue(KeyValueToken.Int32);
        }

        /// <summary>
        /// Writes a <see cref="Char"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Char"/> value to write.</param>
        public virtual void WriteValue(char value)
        {
            InternalWriteValue(KeyValueToken.String);
        }

        /// <summary>
        /// Writes a <see cref="Byte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Byte"/> value to write.</param>
        public virtual void WriteValue(byte value)
        {
            InternalWriteValue(KeyValueToken.Int32);
        }

        /// <summary>
        /// Writes a <see cref="SByte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="SByte"/> value to write.</param>
        [CLSCompliant(false)]
        public virtual void WriteValue(sbyte value)
        {
            InternalWriteValue(KeyValueToken.Int32);
        }

        /// <summary>
        /// Writes a <see cref="Decimal"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Decimal"/> value to write.</param>
        public virtual void WriteValue(decimal value)
        {
            InternalWriteValue(KeyValueToken.Float32);
        }

        /// <summary>
        /// Writes a <see cref="Color"/> value.
        /// </summary>
        /// <param name="value"></param>
        public virtual void WriteValue(Color value)
        {
            InternalWriteValue(KeyValueToken.Color);
        }

        /// <summary>
        /// Writes a <see cref="string"/> value as a <see cref="KeyValueToken.WideString"/> token
        /// </summary>
        /// <param name="value"></param>
        public virtual void WriteWideString(string value)
        {
            InternalWriteValue(KeyValueToken.WideString);
        }

        /// <summary>
        /// Writes a <see cref="IntPtr"/> value.
        /// </summary>
        /// <param name="value"></param>
        public virtual void WriteValue(IntPtr value)
        {
            InternalWriteValue(KeyValueToken.Pointer);
        }

        /// <summary>
        /// Writes a <see cref="UIntPtr"/> value.
        /// </summary>
        /// <param name="value"></param>
        [CLSCompliant(false)]
        public virtual void WriteValue(UIntPtr value)
        {
            InternalWriteValue(KeyValueToken.Pointer);
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Int32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Int32"/> value to write.</param>
        public virtual void WriteValue(int? value)
        {
            if (value != null)
            {
                WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="UInt32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="UInt32"/> value to write.</param>
        [CLSCompliant(false)]
        public virtual void WriteValue(uint? value)
        {
            if (value != null)
            {
                WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Int64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Int64"/> value to write.</param>
        public virtual void WriteValue(long? value)
        {
            if (value != null)
            {
                WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="UInt64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="UInt64"/> value to write.</param>
        [CLSCompliant(false)]
        public virtual void WriteValue(ulong? value)
        {
            if (value != null)
            {
                WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Single"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Single"/> value to write.</param>
        public virtual void WriteValue(float? value)
        {
            if (value != null)
            {
                WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Double"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Double"/> value to write.</param>
        public virtual void WriteValue(double? value)
        {
            if (value != null)
            {
                WriteValue(value.GetValueOrDefault());
            }
        }
        
        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Int16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Int16"/> value to write.</param>
        public virtual void WriteValue(short? value)
        {
            if (value != null)
            {
                WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="UInt16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="UInt16"/> value to write.</param>
        [CLSCompliant(false)]
        public virtual void WriteValue(ushort? value)
        {
            if (value != null)
            {
                WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Char"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Char"/> value to write.</param>
        public virtual void WriteValue(char? value)
        {
            if (value != null)
            {
                WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Byte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Byte"/> value to write.</param>
        public virtual void WriteValue(byte? value)
        {
            if (value != null)
            {
                WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="SByte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="SByte"/> value to write.</param>
        [CLSCompliant(false)]
        public virtual void WriteValue(sbyte? value)
        {
            if (value != null)
            {
                WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Decimal"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Decimal"/> value to write.</param>
        public virtual void WriteValue(decimal? value)
        {
            if (value != null)
            {
                WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Object"/> value.
        /// An error will raised if the value cannot be written as a single KeyValue token.
        /// </summary>
        /// <param name="value">The <see cref="Object"/> value to write.</param>
        public virtual void WriteValue(object value)
        {
            if (value != null)
            {
                WriteValue(this, ConvertUtils.GetTypeCode(value.GetType()), value);
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="Color"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="Color"/> value to write.</param>
        public virtual void WriteValue(Color? value)
        {
            if (value != null)
            {
                WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="IntPtr"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="IntPtr"/> value to write.</param>
        public virtual void WriteValue(IntPtr? value)
        {
            if (value != null)
            {
                WriteValue(value.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{T}"/> of <see cref="UIntPtr"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{T}"/> of <see cref="UIntPtr"/> value to write.</param>
        [CLSCompliant(false)]
        public virtual void WriteValue(UIntPtr? value)
        {
            if (value != null)
            {
                WriteValue(value.GetValueOrDefault());
            }
        }
        #endregion

        /// <summary>
        /// Writes a comment <c>/*...*/</c> containing the specified text.
        /// </summary>
        /// <param name="text">Text to place inside the comment.</param>
        public virtual void WriteComment(string text)
        {
            InternalWriteComment();
        }

        /// <summary>
        /// Writes the given white space.
        /// </summary>
        /// <param name="ws">The string of white space characters.</param>
        public virtual void WriteWhitespace(string ws)
        {
            InternalWriteWhitespace(ws);
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

        internal static void WriteValue(KeyValueWriter writer, PrimitiveTypeCode typeCode, object value)
        {
            switch (typeCode)
            {
                case PrimitiveTypeCode.Char:
                    writer.WriteValue((char)value);
                    break;
                case PrimitiveTypeCode.CharNullable:
                    writer.WriteValue((value == null) ? (char?)null : (char)value);
                    break;
                case PrimitiveTypeCode.SByte:
                    writer.WriteValue((sbyte)value);
                    break;
                case PrimitiveTypeCode.SByteNullable:
                    writer.WriteValue((value == null) ? (sbyte?)null : (sbyte)value);
                    break;
                case PrimitiveTypeCode.Int16:
                    writer.WriteValue((short)value);
                    break;
                case PrimitiveTypeCode.Int16Nullable:
                    writer.WriteValue((value == null) ? (short?)null : (short)value);
                    break;
                case PrimitiveTypeCode.UInt16:
                    writer.WriteValue((ushort)value);
                    break;
                case PrimitiveTypeCode.UInt16Nullable:
                    writer.WriteValue((value == null) ? (ushort?)null : (ushort)value);
                    break;
                case PrimitiveTypeCode.Int32:
                    writer.WriteValue((int)value);
                    break;
                case PrimitiveTypeCode.Int32Nullable:
                    writer.WriteValue((value == null) ? (int?)null : (int)value);
                    break;
                case PrimitiveTypeCode.Byte:
                    writer.WriteValue((byte)value);
                    break;
                case PrimitiveTypeCode.ByteNullable:
                    writer.WriteValue((value == null) ? (byte?)null : (byte)value);
                    break;
                case PrimitiveTypeCode.UInt32:
                    writer.WriteValue((uint)value);
                    break;
                case PrimitiveTypeCode.UInt32Nullable:
                    writer.WriteValue((value == null) ? (uint?)null : (uint)value);
                    break;
                case PrimitiveTypeCode.Int64:
                    writer.WriteValue((long)value);
                    break;
                case PrimitiveTypeCode.Int64Nullable:
                    writer.WriteValue((value == null) ? (long?)null : (long)value);
                    break;
                case PrimitiveTypeCode.UInt64:
                    writer.WriteValue((ulong)value);
                    break;
                case PrimitiveTypeCode.UInt64Nullable:
                    writer.WriteValue((value == null) ? (ulong?)null : (ulong)value);
                    break;
                case PrimitiveTypeCode.Single:
                    writer.WriteValue((float)value);
                    break;
                case PrimitiveTypeCode.SingleNullable:
                    writer.WriteValue((value == null) ? (float?)null : (float)value);
                    break;
                case PrimitiveTypeCode.Double:
                    writer.WriteValue((double)value);
                    break;
                case PrimitiveTypeCode.DoubleNullable:
                    writer.WriteValue((value == null) ? (double?)null : (double)value);
                    break;
                case PrimitiveTypeCode.Decimal:
                    writer.WriteValue((decimal)value);
                    break;
                case PrimitiveTypeCode.DecimalNullable:
                    writer.WriteValue((value == null) ? (decimal?)null : (decimal)value);
                    break;
                case PrimitiveTypeCode.String:
                    writer.WriteValue((string)value);
                    break;
                default:
                    throw CreateUnsupportedTypeException(writer, value);
            }
        }

        private static KeyValueWriterException CreateUnsupportedTypeException(KeyValueWriter writer, object value)
        {
            return KeyValueWriterException.Create(writer, "Unsupported type: {0}. Use the KeyValueSerializer class to get the object's KeyValue representation.".FormatWith(CultureInfo.InvariantCulture, value.GetType()), null);
        }

        /// <summary>
        /// Sets the state of the <see cref="KeyValueWriter"/>.
        /// </summary>
        /// <param name="token">The <see cref="KeyValueToken"/> being written.</param>
        /// <param name="value">The value being written.</param>
        protected void SetWriteState(KeyValueToken token, object value)
        {
            switch (token)
            {
                case KeyValueToken.Start:
                    InternalWriteStart(token, KeyValueContainerType.Object);
                    break;
                case KeyValueToken.PropertyName:
                    if (!(value is string))
                    {
                        throw new ArgumentException("A name is required when setting property name state.", nameof(value));
                    }

                    InternalWritePropertyName((string)value);
                    break;
                case KeyValueToken.Comment:
                    InternalWriteComment();
                    break;
                case KeyValueToken.Raw:
                    InternalWriteRaw();
                    break;
                case KeyValueToken.Color:
                case KeyValueToken.Float32:
                case KeyValueToken.String:
                case KeyValueToken.Int32:
                case KeyValueToken.Int64:
                case KeyValueToken.UInt64:
                case KeyValueToken.Pointer:
                case KeyValueToken.WideString:
                    InternalWriteValue(token);
                    break;
                case KeyValueToken.End:
                    InternalWriteEnd(KeyValueContainerType.Object);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(token));
            }
        }

        internal void InternalWriteEnd(KeyValueContainerType container)
        {
            AutoCompleteClose(container);
        }

        internal void InternalWritePropertyName(string name)
        {
            _currentPosition.PropertyName = name;
            AutoComplete(KeyValueToken.PropertyName);
        }

        internal void InternalWriteRaw()
        {
        }

        internal void InternalWriteStart(KeyValueToken token, KeyValueContainerType container)
        {
            UpdateScopeWithFinishedValue();
            AutoComplete(token);
            Push(container);
        }

        internal void InternalWriteValue(KeyValueToken token)
        {
            UpdateScopeWithFinishedValue();
            AutoComplete(token);
        }

        internal void InternalWriteWhitespace(string ws)
        {
            if (ws != null)
            {
                if (!StringUtils.IsWhiteSpace(ws))
                {
                    throw KeyValueWriterException.Create(this, "Only white space characters should be used.", null);
                }
            }
        }

        internal void InternalWriteComment()
        {
            AutoComplete(KeyValueToken.Comment);
        }
    }
}
