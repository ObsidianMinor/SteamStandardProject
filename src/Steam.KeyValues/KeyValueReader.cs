using System;
using System.Globalization;

namespace Steam.KeyValues
{
    /// <summary>
    /// Represents a reader that provides fast, non-cached, forward-only access to serialized KeyValues
    /// </summary>
    public abstract class KeyValueReader : IDisposable
    {
        protected internal enum State
        {
            Start,
            End,
            KeyValue,
            SubkeysStart,
            Subkeys,
            Closed,
            PostValue,
            Condition,
            Error
        }

        private KeyValueToken _tokenType;
        private object _value;
        internal char _quoteChar;
        internal State _state;
        private CultureInfo _culture;

        protected State CurrentState => _state;

        public bool CloseInput { get; set; }

        public bool SupportMultipleContent { get; set; }
        
        public virtual KeyValueToken TokenType => _tokenType;

        public virtual object Value => _value;

        public virtual Type ValueType => _value?.GetType();
        
        public CultureInfo Culture
        {
            get => _culture ?? CultureInfo.InvariantCulture;
            set => _culture = value;
        }

        protected KeyValueReader()
        {
            _state = State.Start;
            CloseInput = true;
        }
        
        public abstract bool Read();
        
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_state != State.Closed && disposing)
            {
                Close();
            }
        }

        public virtual void Close()
        {
            _state = State.Closed;
            _tokenType = KeyValueToken.None;
            _value = null;
        }
    }
}
