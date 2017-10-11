using Steam.KeyValues.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Steam.KeyValues.Linq
{
    /// <summary>
    /// Represents a KeyValue property.
    /// </summary>
    public partial class KVProperty : KVContainer
    {
        #region JPropertyList
        private class KVPropertyList : IList<KVToken>
        {
            internal KVToken _token;

            public IEnumerator<KVToken> GetEnumerator()
            {
                if (_token != null)
                {
                    yield return _token;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(KVToken item)
            {
                _token = item;
            }

            public void Clear()
            {
                _token = null;
            }

            public bool Contains(KVToken item)
            {
                return (_token == item);
            }

            public void CopyTo(KVToken[] array, int arrayIndex)
            {
                if (_token != null)
                {
                    array[arrayIndex] = _token;
                }
            }

            public bool Remove(KVToken item)
            {
                if (_token == item)
                {
                    _token = null;
                    return true;
                }
                return false;
            }

            public int Count
            {
                get { return (_token != null) ? 1 : 0; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public int IndexOf(KVToken item)
            {
                return (_token == item) ? 0 : -1;
            }

            public void Insert(int index, KVToken item)
            {
                if (index == 0)
                {
                    _token = item;
                }
            }

            public void RemoveAt(int index)
            {
                if (index == 0)
                {
                    _token = null;
                }
            }

            public KVToken this[int index]
            {
                get { return (index == 0) ? _token : null; }
                set
                {
                    if (index == 0)
                    {
                        _token = value;
                    }
                }
            }
        }
        #endregion

        private readonly KVPropertyList _content = new KVPropertyList();
        private readonly string _name;
        private Conditional _condition;

        /// <summary>
        /// Gets the container's children tokens.
        /// </summary>
        /// <value>The container's children tokens.</value>
        protected override IList<KVToken> ChildrenTokens
        {
            get { return _content; }
        }

        /// <summary>
        /// Gets the property name.
        /// </summary>
        /// <value>The property name.</value>
        public string Name
        {
            [DebuggerStepThrough]
            get { return _name; }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        /// <value>The property value.</value>
        public virtual KVToken Value
        {
            [DebuggerStepThrough]
            get { return _content._token; }
            set
            {
                CheckReentrancy();

                KVToken newValue = value ?? KVValue.CreateString("");

                if (_content._token == null)
                {
                    InsertItem(0, newValue, false);
                }
                else
                {
                    SetItem(0, newValue);
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the condition for this container
        /// </summary>
        public virtual Conditional Condition
        {
            get => _condition;
            set
            {
                if (value < Conditional.None || value > Conditional.NotWindows)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _condition = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KVProperty"/> class from another <see cref="KVProperty"/> object.
        /// </summary>
        /// <param name="other">A <see cref="KVProperty"/> object to copy from.</param>
        public KVProperty(KVProperty other)
            : base(other)
        {
            _name = other.Name;
        }

        internal override KVToken GetItem(int index)
        {
            if (index != 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            return Value;
        }

        internal override void SetItem(int index, KVToken item)
        {
            if (index != 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (IsTokenUnchanged(Value, item))
            {
                return;
            }
        
            base.SetItem(0, item);

            ((KVObject)Parent)?.InternalPropertyChanged(this);
        }

        internal override bool RemoveItem(KVToken item)
        {
            throw new KeyValueException("Cannot add or remove items from {0}.".FormatWith(CultureInfo.InvariantCulture, typeof(KVProperty)));
        }

        internal override void RemoveItemAt(int index)
        {
            throw new KeyValueException("Cannot add or remove items from {0}.".FormatWith(CultureInfo.InvariantCulture, typeof(KVProperty)));
        }

        internal override int IndexOfItem(KVToken item)
        {
            return _content.IndexOf(item);
        }

        internal override void InsertItem(int index, KVToken item, bool skipParentCheck)
        {
            // don't add comments to JProperty
            if (item != null && item.Type == KVTokenType.Comment)
            {
                return;
            }

            if (Value != null)
            {
                throw new KeyValueException("{0} cannot have multiple values.".FormatWith(CultureInfo.InvariantCulture, typeof(KVProperty)));
            }

            base.InsertItem(0, item, false);
        }

        internal override bool ContainsItem(KVToken item)
        {
            return (Value == item);
        }

        internal override void MergeItem(object content, KeyValueMergeSettings settings)
        {
            KVToken value = (content as KVProperty)?.Value;

            if (value != null)
            {
                Value = value;
            }
        }

        internal override void ClearItems()
        {
            throw new KeyValueException("Cannot add or remove items from {0}.".FormatWith(CultureInfo.InvariantCulture, typeof(KVProperty)));
        }

        internal override bool DeepEquals(KVToken node)
        {
            KVProperty t = node as KVProperty;
            return (t != null && _name == t.Name && ContentsEqual(t));
        }

        internal override KVToken CloneToken()
        {
            return new KVProperty(this);
        }

        /// <summary>
        /// Gets the node type for this <see cref="KVToken"/>.
        /// </summary>
        /// <value>The type.</value>
        public override KVTokenType Type
        {
            [DebuggerStepThrough]
            get { return KVTokenType.Property; }
        }

        internal KVProperty(string name)
        {
            // called from KVTokenWriter
            ValidationUtils.ArgumentNotNull(name, nameof(name));

            _name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KVProperty"/> class.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="content">The property content.</param>
        public KVProperty(string name, params object[] content)
            : this(name, (object)content)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KVProperty"/> class.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="content">The property content.</param>
        public KVProperty(string name, object content)
        {
            ValidationUtils.ArgumentNotNull(name, nameof(name));

            _name = name;

            Value = CreateFromContent(content);
        }

        /// <summary>
        /// Writes this token to a <see cref="KeyValueWriter"/>.
        /// </summary>
        /// <param name="writer">A <see cref="KeyValueWriter"/> into which this method will write.</param>
        /// <param name="converters">A collection of <see cref="KeyValueConverter"/> which will be used when writing the token.</param>
        public override void WriteTo(KeyValueWriter writer, params KeyValueConverter[] converters)
        {
            KVToken value = Value;

            if (value != null)
                writer.WritePropertyName(_name);

            if (value is KVObject)
            {
                if (value != null)
                {
                    writer.WriteConditional(Condition);
                    value.WriteTo(writer, converters);
                }
            }
            else
            {
                if (value != null)
                    value.WriteTo(writer, converters);
                else
                    writer.WriteValue("");

                writer.WriteConditional(Condition);
            }
        }

        internal override int GetDeepHashCode()
        {
            return _name.GetHashCode() ^ ((Value != null) ? Value.GetDeepHashCode() : 0);
        }

        /// <summary>
        /// Loads a <see cref="KVProperty"/> from a <see cref="KeyValueReader"/>.
        /// </summary>
        /// <param name="reader">A <see cref="KeyValueReader"/> that will be read for the content of the <see cref="KVProperty"/>.</param>
        /// <returns>A <see cref="KVProperty"/> that contains the KeyValue that was read from the specified <see cref="KeyValueReader"/>.</returns>
        public new static KVProperty Load(KeyValueReader reader)
        {
            return Load(reader, null);
        }

        /// <summary>
        /// Loads a <see cref="KVProperty"/> from a <see cref="KeyValueReader"/>.
        /// </summary>
        /// <param name="reader">A <see cref="KeyValueReader"/> that will be read for the content of the <see cref="KVProperty"/>.</param>
        /// <param name="settings">The <see cref="KeyValueLoadSettings"/> used to load the KeyValue.
        /// If this is <c>null</c>, default load settings will be used.</param>
        /// <returns>A <see cref="KVProperty"/> that contains the KeyValue that was read from the specified <see cref="KeyValueReader"/>.</returns>
        public new static KVProperty Load(KeyValueReader reader, KeyValueLoadSettings settings)
        {
            if (reader.TokenType == KeyValueToken.None)
            {
                if (!reader.Read())
                {
                    throw KeyValueReaderException.Create(reader, "Error reading JProperty from KeyValueReader.");
                }
            }

            reader.MoveToContent();

            if (reader.TokenType != KeyValueToken.PropertyName)
            {
                throw KeyValueReaderException.Create(reader, "Error reading JProperty from KeyValueReader. Current KeyValueReader item is not a property: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            KVProperty p = new KVProperty((string)reader.Value);
            p.SetLineInfo(reader as IKeyValueLineInfo, settings);

            p.ReadTokenFrom(reader, settings);

            return p;
        }
    }
}
