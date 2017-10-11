using Steam.KeyValues.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Steam.KeyValues.Linq
{
    /// <summary>
    /// Represents a KeyValue object.
    /// </summary>
    /// <example>
    ///   <code lang="cs" source="..\Src\Newtonsoft.KeyValue.Tests\Documentation\LinqToKeyValueTests.cs" region="LinqToKeyValueCreateParse" title="Parsing a KeyValue Object from Text" />
    /// </example>
    public partial class KVObject : KVContainer, IDictionary<string, KVToken>, INotifyPropertyChanged
    {
        private readonly KVPropertyKeyedCollection _properties = new KVPropertyKeyedCollection();

        /// <summary>
        /// Gets the container's children tokens.
        /// </summary>
        /// <value>The container's children tokens.</value>
        protected override IList<KVToken> ChildrenTokens
        {
            get { return _properties; }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="KVObject"/> class.
        /// </summary>
        public KVObject()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KVObject"/> class from another <see cref="KVObject"/> object.
        /// </summary>
        /// <param name="other">A <see cref="KVObject"/> object to copy from.</param>
        public KVObject(KVObject other)
            : base(other)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KVObject"/> class with the specified content.
        /// </summary>
        /// <param name="content">The contents of the object.</param>
        public KVObject(params object[] content)
            : this((object)content)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KVObject"/> class with the specified content.
        /// </summary>
        /// <param name="content">The contents of the object.</param>
        public KVObject(object content)
        {
            Add(content);
        }

        internal override bool DeepEquals(KVToken node)
        {
            KVObject t = node as KVObject;
            if (t == null)
            {
                return false;
            }

            return _properties.Compare(t._properties);
        }

        internal override int IndexOfItem(KVToken item)
        {
            return _properties.IndexOfReference(item);
        }

        internal override void InsertItem(int index, KVToken item, bool skipParentCheck)
        {
            // don't add comments to KVObject, no name to reference comment by
            if (item != null && item.Type == KVTokenType.Comment)
            {
                return;
            }

            base.InsertItem(index, item, skipParentCheck);
        }

        internal override void ValidateToken(KVToken o, KVToken existing)
        {
            ValidationUtils.ArgumentNotNull(o, nameof(o));

            if (o.Type != KVTokenType.Property)
            {
                throw new ArgumentException("Can not add {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, o.GetType(), GetType()));
            }

            KVProperty newProperty = (KVProperty)o;

            if (existing != null)
            {
                KVProperty existingProperty = (KVProperty)existing;

                if (newProperty.Name == existingProperty.Name)
                {
                    return;
                }
            }

            if (_properties.TryGetValue(newProperty.Name, out existing))
            {
                throw new ArgumentException("Can not add property {0} to {1}. Property with the same name already exists on object.".FormatWith(CultureInfo.InvariantCulture, newProperty.Name, GetType()));
            }
        }

        internal override void MergeItem(object content, KeyValueMergeSettings settings)
        {
            KVObject o = content as KVObject;
            if (o == null)
            {
                return;
            }

            foreach (KeyValuePair<string, KVToken> contentItem in o)
            {
                KVProperty existingProperty = Property(contentItem.Key);

                if (existingProperty == null)
                {
                    Add(contentItem.Key, contentItem.Value);
                }
                else if (contentItem.Value != null)
                {
                    KVContainer existingContainer = existingProperty.Value as KVContainer;
                    if (existingContainer == null || existingContainer.Type != contentItem.Value.Type)
                    {
                        if (settings?.MergeNullValueHandling == MergeNullValueHandling.Merge)
                        {
                            existingProperty.Value = contentItem.Value;
                        }
                    }
                    else
                    {
                        existingContainer.Merge(contentItem.Value, settings);
                    }
                }
            }
        }

        internal void InternalPropertyChanged(KVProperty childProperty)
        {
            OnPropertyChanged(childProperty.Name);

            if (_collectionChanged != null)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, childProperty, childProperty, IndexOfItem(childProperty)));
            }
        }

        internal override KVToken CloneToken()
        {
            return new KVObject(this);
        }

        /// <summary>
        /// Gets the node type for this <see cref="KVToken"/>.
        /// </summary>
        /// <value>The type.</value>
        public override KVTokenType Type
        {
            get { return KVTokenType.Object; }
        }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> of <see cref="KVProperty"/> of this object's properties.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="KVProperty"/> of this object's properties.</returns>
        public IEnumerable<KVProperty> Properties()
        {
            return _properties.Cast<KVProperty>();
        }

        /// <summary>
        /// Gets a <see cref="KVProperty"/> the specified name.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <returns>A <see cref="KVProperty"/> with the specified name or <c>null</c>.</returns>
        public KVProperty Property(string name)
        {
            if (name == null)
            {
                return null;
            }

            _properties.TryGetValue(name, out KVToken property);
            return (KVProperty)property;
        }

        /// <summary>
        /// Gets a <see cref="KVEnumerable{T}"/> of <see cref="KVToken"/> of this object's property values.
        /// </summary>
        /// <returns>A <see cref="KVEnumerable{T}"/> of <see cref="KVToken"/> of this object's property values.</returns>
        public KVEnumerable<KVToken> PropertyValues()
        {
            return new KVEnumerable<KVToken>(Properties().Select(p => p.Value));
        }

        /// <summary>
        /// Gets the <see cref="KVToken"/> with the specified key.
        /// </summary>
        /// <value>The <see cref="KVToken"/> with the specified key.</value>
        public override KVToken this[object key]
        {
            get
            {
                ValidationUtils.ArgumentNotNull(key, nameof(key));

                string propertyName = key as string;
                if (propertyName == null)
                {
                    throw new ArgumentException("Accessed KVObject values with invalid key value: {0}. Object property name expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
                }

                return this[propertyName];
            }
            set
            {
                ValidationUtils.ArgumentNotNull(key, nameof(key));

                string propertyName = key as string;
                if (propertyName == null)
                {
                    throw new ArgumentException("Set KVObject values with invalid key value: {0}. Object property name expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
                }

                this[propertyName] = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="KVToken"/> with the specified property name.
        /// </summary>
        /// <value></value>
        public KVToken this[string propertyName]
        {
            get
            {
                ValidationUtils.ArgumentNotNull(propertyName, nameof(propertyName));

                KVProperty property = Property(propertyName);

                return property?.Value;
            }
            set
            {
                KVProperty property = Property(propertyName);
                if (property != null)
                {
                    property.Value = value;
                }
                else
                {
                    Add(new KVProperty(propertyName, value));
                    OnPropertyChanged(propertyName);
                }
            }
        }

        /// <summary>
        /// Loads a <see cref="KVObject"/> from a <see cref="KeyValueReader"/>.
        /// </summary>
        /// <param name="reader">A <see cref="KeyValueReader"/> that will be read for the content of the <see cref="KVObject"/>.</param>
        /// <returns>A <see cref="KVObject"/> that contains the KeyValue that was read from the specified <see cref="KeyValueReader"/>.</returns>
        /// <exception cref="KeyValueReaderException">
        ///     <paramref name="reader"/> is not valid KeyValue.
        /// </exception>
        public new static KVObject Load(KeyValueReader reader)
        {
            return Load(reader, null);
        }

        /// <summary>
        /// Loads a <see cref="KVObject"/> from a <see cref="KeyValueReader"/>.
        /// </summary>
        /// <param name="reader">A <see cref="KeyValueReader"/> that will be read for the content of the <see cref="KVObject"/>.</param>
        /// <param name="settings">The <see cref="KeyValueLoadSettings"/> used to load the KeyValue.
        /// If this is <c>null</c>, default load settings will be used.</param>
        /// <returns>A <see cref="KVObject"/> that contains the KeyValue that was read from the specified <see cref="KeyValueReader"/>.</returns>
        /// <exception cref="KeyValueReaderException">
        ///     <paramref name="reader"/> is not valid KeyValue.
        /// </exception>
        public new static KVObject Load(KeyValueReader reader, KeyValueLoadSettings settings)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            if (reader.TokenType == KeyValueToken.None)
            {
                if (!reader.Read())
                {
                    throw KeyValueReaderException.Create(reader, "Error reading KVObject from KeyValueReader.");
                }
            }

            reader.MoveToContent();

            if (reader.TokenType != KeyValueToken.Start)
            {
                throw KeyValueReaderException.Create(reader, "Error reading KVObject from KeyValueReader. Current KeyValueReader item is not an object: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            KVObject o = new KVObject();
            o.SetLineInfo(reader as IKeyValueLineInfo, settings);

            o.ReadTokenFrom(reader, settings);

            return o;
        }

        /// <summary>
        /// Creates a <see cref="KVObject"/> from an object.
        /// </summary>
        /// <param name="o">The object that will be used to create <see cref="KVObject"/>.</param>
        /// <returns>A <see cref="KVObject"/> with the values of the specified object.</returns>
        public new static KVObject FromObject(object o)
        {
            return FromObject(o, KeyValueSerializer.CreateDefault());
        }

        /// <summary>
        /// Creates a <see cref="KVObject"/> from an object.
        /// </summary>
        /// <param name="o">The object that will be used to create <see cref="KVObject"/>.</param>
        /// <param name="KeyValueSerializer">The <see cref="KeyValueSerializer"/> that will be used to read the object.</param>
        /// <returns>A <see cref="KVObject"/> with the values of the specified object.</returns>
        public new static KVObject FromObject(object o, KeyValueSerializer KeyValueSerializer)
        {
            KVToken token = FromObjectInternal(o, KeyValueSerializer);

            if (token != null && token.Type != KVTokenType.Object)
            {
                throw new ArgumentException("Object serialized to {0}. KVObject instance expected.".FormatWith(CultureInfo.InvariantCulture, token.Type));
            }

            return (KVObject)token;
        }

        /// <summary>
        /// Writes this token to a <see cref="KeyValueWriter"/>.
        /// </summary>
        /// <param name="writer">A <see cref="KeyValueWriter"/> into which this method will write.</param>
        /// <param name="converters">A collection of <see cref="KeyValueConverter"/> which will be used when writing the token.</param>
        public override void WriteTo(KeyValueWriter writer, params KeyValueConverter[] converters)
        {
            writer.WriteStartObject();

            for (int i = 0; i < _properties.Count; i++)
            {
                _properties[i].WriteTo(writer, converters);
            }

            writer.WriteEndObject();
        }

        /// <summary>
        /// Gets the <see cref="KVToken"/> with the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The <see cref="KVToken"/> with the specified property name.</returns>
        public KVToken GetValue(string propertyName)
        {
            return GetValue(propertyName, StringComparison.Ordinal);
        }

        /// <summary>
        /// Gets the <see cref="KVToken"/> with the specified property name.
        /// The exact property name will be searched for first and if no matching property is found then
        /// the <see cref="StringComparison"/> will be used to match a property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
        /// <returns>The <see cref="KVToken"/> with the specified property name.</returns>
        public KVToken GetValue(string propertyName, StringComparison comparison)
        {
            if (propertyName == null)
            {
                return null;
            }

            // attempt to get value via dictionary first for performance
            KVProperty property = Property(propertyName);
            if (property != null)
            {
                return property.Value;
            }

            // test above already uses this comparison so no need to repeat
            if (comparison != StringComparison.Ordinal)
            {
                foreach (KVProperty p in _properties)
                {
                    if (string.Equals(p.Name, propertyName, comparison))
                    {
                        return p.Value;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Tries to get the <see cref="KVToken"/> with the specified property name.
        /// The exact property name will be searched for first and if no matching property is found then
        /// the <see cref="StringComparison"/> will be used to match a property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        /// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
        /// <returns><c>true</c> if a value was successfully retrieved; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(string propertyName, StringComparison comparison, out KVToken value)
        {
            value = GetValue(propertyName, comparison);
            return (value != null);
        }

        #region IDictionary<string,KVToken> Members
        /// <summary>
        /// Adds the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        public void Add(string propertyName, KVToken value)
        {
            Add(new KVProperty(propertyName, value));
        }

        bool IDictionary<string, KVToken>.ContainsKey(string key)
        {
            return _properties.Contains(key);
        }

        ICollection<string> IDictionary<string, KVToken>.Keys
        {
            // todo: make order of the collection returned match KVObject order
            get { return _properties.Keys; }
        }

        /// <summary>
        /// Removes the property with the specified name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns><c>true</c> if item was successfully removed; otherwise, <c>false</c>.</returns>
        public bool Remove(string propertyName)
        {
            KVProperty property = Property(propertyName);
            if (property == null)
            {
                return false;
            }

            property.Remove();
            return true;
        }

        /// <summary>
        /// Tries to get the <see cref="KVToken"/> with the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if a value was successfully retrieved; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(string propertyName, out KVToken value)
        {
            KVProperty property = Property(propertyName);
            if (property == null)
            {
                value = null;
                return false;
            }

            value = property.Value;
            return true;
        }

        ICollection<KVToken> IDictionary<string, KVToken>.Values
        {
            get
            {
                // todo: need to wrap _properties.Values with a collection to get the KVProperty value
                throw new NotImplementedException();
            }
        }
        #endregion

        #region ICollection<KeyValuePair<string,KVToken>> Members
        void ICollection<KeyValuePair<string, KVToken>>.Add(KeyValuePair<string, KVToken> item)
        {
            Add(new KVProperty(item.Key, item.Value));
        }

        void ICollection<KeyValuePair<string, KVToken>>.Clear()
        {
            RemoveAll();
        }

        bool ICollection<KeyValuePair<string, KVToken>>.Contains(KeyValuePair<string, KVToken> item)
        {
            KVProperty property = Property(item.Key);
            if (property == null)
            {
                return false;
            }

            return (property.Value == item.Value);
        }

        void ICollection<KeyValuePair<string, KVToken>>.CopyTo(KeyValuePair<string, KVToken>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "arrayIndex is less than 0.");
            }
            if (arrayIndex >= array.Length && arrayIndex != 0)
            {
                throw new ArgumentException("arrayIndex is equal to or greater than the length of array.");
            }
            if (Count > array.Length - arrayIndex)
            {
                throw new ArgumentException("The number of elements in the source KVObject is greater than the available space from arrayIndex to the end of the destination array.");
            }

            int index = 0;
            foreach (KVProperty property in _properties)
            {
                array[arrayIndex + index] = new KeyValuePair<string, KVToken>(property.Name, property.Value);
                index++;
            }
        }

        bool ICollection<KeyValuePair<string, KVToken>>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<KeyValuePair<string, KVToken>>.Remove(KeyValuePair<string, KVToken> item)
        {
            if (!((ICollection<KeyValuePair<string, KVToken>>)this).Contains(item))
            {
                return false;
            }

            ((IDictionary<string, KVToken>)this).Remove(item.Key);
            return true;
        }
        #endregion

        internal override int GetDeepHashCode()
        {
            return ContentsHashCode();
        }

        /// <summary>
        /// Returns an enumerator that can be used to iterate through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<string, KVToken>> GetEnumerator()
        {
            foreach (KVProperty property in _properties)
            {
                yield return new KeyValuePair<string, KVToken>(property.Name, property.Value);
            }
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event with the provided arguments.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Returns the <see cref="DynamicMetaObject"/> responsible for binding operations performed on this object.
        /// </summary>
        /// <param name="parameter">The expression tree representation of the runtime value.</param>
        /// <returns>
        /// The <see cref="DynamicMetaObject"/> to bind this object.
        /// </returns>
        protected override DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new DynamicProxyMetaObject<KVObject>(parameter, this, new KVObjectDynamicProxy());
        }

        private class KVObjectDynamicProxy : DynamicProxy<KVObject>
        {
            public override bool TryGetMember(KVObject instance, GetMemberBinder binder, out object result)
            {
                // result can be null
                result = instance[binder.Name];
                return true;
            }

            public override bool TrySetMember(KVObject instance, SetMemberBinder binder, object value)
            {
                KVToken v = value as KVToken;

                // this can throw an error if value isn't a valid for a JValue
                if (v == null)
                {
                    v = new KVValue(value);
                }

                instance[binder.Name] = v;
                return true;
            }

            public override IEnumerable<string> GetDynamicMemberNames(KVObject instance)
            {
                return instance.Properties().Select(p => p.Name);
            }
        }
    }
}
