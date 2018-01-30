using System;
using System.Globalization;

namespace Steam.KeyValues
{
    /// <summary>
    /// Represents a writer that provides a fast, non-cached, foward-only way of generating KeyValue data.
    /// </summary>
    public abstract class KeyValueWriter : IDisposable
    {
        internal enum State
        {
            Start,
            KeyValue,
            SubkeysStart,
            Subkeys,
            Closed,
            Error
        }

        private State _currentState;
        private CultureInfo _culture;
        private int _depth;

        public bool CloseOutput { get; set; }

        public bool AutoCompleteOnClose { get; set; }
        
        public CultureInfo Culture
        {
            get => _culture ?? CultureInfo.InvariantCulture;
            set => _culture = value;
        }

        protected int Top => _depth;

        protected KeyValueWriter()
        {
            _currentState = State.Start;

            CloseOutput = true;
            AutoCompleteOnClose = true;
        }

        public abstract void Flush();
        
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_currentState != State.Closed && disposing)
            {
                Close();
            }
        }

        protected virtual void WriteStart() { }

        protected virtual void WriteEnd() { }

        protected virtual void WriteIndent() { }

        protected virtual void WriteValueDelimiter() { }

        protected virtual void WriteIndentSpace() { }

        protected void SetWriteState(KeyValueToken token)
        {
            switch(token)
            {
                case KeyValueToken.StartSubkeys:

                    break;
                case KeyValueToken.EndSubkeys:

                    break;
                case KeyValueToken.Key:

                    break;
                case KeyValueToken.Value:

                    break;
                case KeyValueToken.Conditional:

                    break;
                case KeyValueToken.Comment:

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(token));
            }
        }

        public virtual void Close()
        {
            if (AutoCompleteOnClose)
            {
                AutoCompleteAll();
            }
        }

        private void AutoComplete(KeyValueToken token)
        {

        }

        private void AutoCompleteAll()
        {
            while (Top > 0)
            {
                WriteEnd();
            }
        }
    }
}
