using Steam.KeyValues.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Steam.KeyValues.Linq
{
    /// <summary>
    /// Represents a reader that provides fast, non-cached, forward-only access to serialized KeyValue data.
    /// </summary>
    public class KVTokenReader : KeyValueReader, IKeyValueLineInfo
    {
        private readonly KVToken _root;
        private string _initialPath;
        private KVToken _parent;
        private KVToken _current;

        /// <summary>
        /// Gets the <see cref="KVToken"/> at the reader's current position.
        /// </summary>
        public KVToken CurrentToken => _current;

        /// <summary>
        /// Initializes a new instance of the <see cref="KVTokenReader"/> class.
        /// </summary>
        /// <param name="token">The token to read from</param>
        public KVTokenReader(KVToken token)
        {
            ValidationUtils.ArgumentNotNull(token, nameof(token));

            _root = token;
        }

        /// <summary>
        /// Reads the next JSON from the underlying <see cref="KVToken"/>
        /// </summary>
        /// <returns>
        /// <c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.
        /// </returns>
        protected override bool ReadInternal()
        {
            if (CurrentState != State.Start)
            {
                if (_current == null)
                {
                    return false;
                }

                if(_current is KVContainer container && _parent != container)
                {
                    return ReadInto(container);
                }
                else
                {
                    return ReadOver(_current);
                }
            }

            _current = _root;
            SetToken(_current);
            return true;
        }

        private bool ReadOver(KVToken t)
        {
            if (t == _root)
            {
                return ReadToEnd();
            }

            KVToken next = t.Next;
            if ((next == null || next == t) || t == t.Parent.Last)
            {
                if (t.Parent == null)
                {
                    return ReadToEnd();
                }

                return SetEnd(t.Parent);
            }
            else
            {
                _current = next;
                SetToken(_current);
                return true;
            }
        }

        private bool SetEnd(KVContainer c)
        {
            KeyValueToken? endToken = GetEndToken(c);
            if(endToken != null)
            {
                SetToken(endToken.GetValueOrDefault());
                _current = c;
                _parent = c;
                return true;
            }
            else
            {
                return ReadOver(c);
            }
        }

        private void SetToken(KVToken token)
        {
            switch (token.Type)
            {
                case KVTokenType.Color:
                    SetToken(KeyValueToken.Color, ((KVValue)token).Value);
                    break;
                case KVTokenType.Comment:
                    SetToken(KeyValueToken.Comment, ((KVValue)token).Value);
                    break;
                case KVTokenType.Float:
                    SetToken(KeyValueToken.Float32, ((KVValue)token).Value);
                    break;
                case KVTokenType.Int32:
                    SetToken(KeyValueToken.Int32, ((KVValue)token).Value);
                    break;
                case KVTokenType.Int64:
                    SetToken(KeyValueToken.Int64, ((KVValue)token).Value);
                    break;
                case KVTokenType.Object:
                    SetToken(KeyValueToken.Start);
                    break;
                case KVTokenType.Pointer:
                    SetToken(KeyValueToken.Pointer, ((KVValue)token).Value);
                    break;
                case KVTokenType.Property:
                    SetToken(KeyValueToken.PropertyName, ((KVProperty)token).Name);
                    break;
                case KVTokenType.Raw:
                    SetToken(KeyValueToken.Raw, ((KVValue)token).Value);
                    break;
                case KVTokenType.String:
                    SetToken(KeyValueToken.String, SafeToString(((KVValue)token).Value));
                    break;
                case KVTokenType.UInt64:
                    SetToken(KeyValueToken.UInt64, ((KVValue)token).Value);
                    break;
                case KVTokenType.WideString:
                    SetToken(KeyValueToken.WideString, ((KVValue)token).Value);
                    break;
                default:
                    throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(token.Type), token.Type, "Unexpected KVTokenType");
            }
        }

        private bool ReadToEnd()
        {
            _current = null;
            SetToken(KeyValueToken.None);
            return false;
        }

        private KeyValueToken? GetEndToken(KVContainer c)
        {
            switch(c.Type)
            {
                case KVTokenType.Object:
                    return KeyValueToken.End;
                case KVTokenType.Property:
                    return null;
                default:
                    throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(c.Type), c.Type, "Unexpected KVContainer type.");
            }
        }

        private bool ReadInto(KVContainer c)
        {
            KVToken firstChild = c.First;
            if (firstChild == null)
            {
                return SetEnd(c);
            }
            else
            {
                SetToken(firstChild);
                _current = firstChild;
                _parent = c;
                return true;
            }
        }

        private string SafeToString(object value)
        {
            return value?.ToString();
        }

        bool IKeyValueLineInfo.HasLineInfo()
        {
            if (CurrentState == State.Start)
            {
                return false;
            }

            IKeyValueLineInfo info = _current;
            return (info != null && info.HasLineInfo());
        }

        int IKeyValueLineInfo.LineNumber
        {
            get
            {
                if (CurrentState == State.Start)
                {
                    return 0;
                }

                IKeyValueLineInfo info = _current;
                if (info != null)
                {
                    return info.LineNumber;
                }

                return 0;
            }
        }

        int IKeyValueLineInfo.LinePosition
        {
            get
            {
                if (CurrentState == State.Start)
                {
                    return 0;
                }

                IKeyValueLineInfo info = _current;
                if (info != null)
                {
                    return info.LinePosition;
                }

                return 0;
            }
        }

        /// <summary>
        /// Gets the path of the current KeyValue token. 
        /// </summary>
        public override string Path
        {
            get
            {
                string path = base.Path;

                if (_initialPath == null)
                {
                    _initialPath = _root.Path;
                }

                if (!string.IsNullOrEmpty(_initialPath))
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        return _initialPath;
                    }

                    if (path.StartsWith('['))
                    {
                        path = _initialPath + path;
                    }
                    else
                    {
                        path = _initialPath + "." + path;
                    }
                }

                return path;
            }
        }
    }
}
