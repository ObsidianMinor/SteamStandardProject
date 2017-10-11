using Steam.KeyValues.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading;

namespace Steam.KeyValues.Linq
{
    /// <summary>
    /// Represents a atoken that can contain other tokesn.
    /// </summary>
    public abstract partial class KVContainer : KVToken, IList<KVToken>, IList, INotifyCollectionChanged
    {
        internal NotifyCollectionChangedEventHandler _collectionChanged;

        /// <summary>
        /// Occurs when the items list of the collection has changed, or the collection is reset.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { _collectionChanged += value; }
            remove { _collectionChanged -= value; }
        }

        /// <summary>
        /// Gets the container's children tokens.
        /// </summary>
        /// <value>The container's children tokens.</value>
        protected abstract IList<KVToken> ChildrenTokens { get; }

        private object _syncRoot;
        private bool _busy;

        internal KVContainer()
        {
        }

        internal KVContainer(KVContainer other)
            : this()
        {
            ValidationUtils.ArgumentNotNull(other, nameof(other));

            int i = 0;
            foreach (KVToken child in other)
            {
                AddInternal(i, child, false);
                i++;
            }
        }

        internal void CheckReentrancy()
        {
            if (_busy)
            {
                throw new InvalidOperationException("Cannot change {0} during a collection change event.".FormatWith(CultureInfo.InvariantCulture, GetType()));
            }
        }

        internal virtual IList<KVToken> CreateChildrenCollection()
        {
            return new List<KVToken>();
        }

        /// <summary>
        /// Raises the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = _collectionChanged;

            if (handler != null)
            {
                _busy = true;
                try
                {
                    handler(this, e);
                }
                finally
                {
                    _busy = false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this token has child tokens.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this token has child values; otherwise, <c>false</c>.
        /// </value>
        public override bool HasValues
        {
            get { return ChildrenTokens.Count > 0; }
        }

        internal bool ContentsEqual(KVContainer container)
        {
            if (container == this)
            {
                return true;
            }

            IList<KVToken> t1 = ChildrenTokens;
            IList<KVToken> t2 = container.ChildrenTokens;

            if (t1.Count != t2.Count)
            {
                return false;
            }

            for (int i = 0; i < t1.Count; i++)
            {
                if (!t1[i].DeepEquals(t2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get the first child token of this token.
        /// </summary>
        /// <value>
        /// A <see cref="KVToken"/> containing the first child token of the <see cref="KVToken"/>.
        /// </value>
        public override KVToken First
        {
            get
            {
                IList<KVToken> children = ChildrenTokens;
                return (children.Count > 0) ? children[0] : null;
            }
        }

        /// <summary>
        /// Get the last child token of this token.
        /// </summary>
        /// <value>
        /// A <see cref="KVToken"/> containing the last child token of the <see cref="KVToken"/>.
        /// </value>
        public override KVToken Last
        {
            get
            {
                IList<KVToken> children = ChildrenTokens;
                int count = children.Count;
                return (count > 0) ? children[count - 1] : null;
            }
        }

        /// <summary>
        /// Returns a collection of the child tokens of this token, in document order.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> containing the child tokens of this <see cref="KVToken"/>, in document order.
        /// </returns>
        public override KVEnumerable<KVToken> Children()
        {
            return new KVEnumerable<KVToken>(ChildrenTokens);
        }

        /// <summary>
        /// Returns a collection of the child values of this token, in document order.
        /// </summary>
        /// <typeparam name="T">The type to convert the values to.</typeparam>
        /// <returns>
        /// A <see cref="IEnumerable{T}"/> containing the child values of this <see cref="KVToken"/>, in document order.
        /// </returns>
        public override IEnumerable<T> Values<T>()
        {
            return ChildrenTokens.Convert<KVToken, T>();
        }

        /// <summary>
        /// Returns a collection of the descendant tokens for this token in document order.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> containing the descendant tokens of the <see cref="KVToken"/>.</returns>
        public IEnumerable<KVToken> Descendants()
        {
            return GetDescendants(false);
        }

        /// <summary>
        /// Returns a collection of the tokens that contain this token, and all descendant tokens of this token, in document order.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> containing this token, and all the descendant tokens of the <see cref="KVToken"/>.</returns>
        public IEnumerable<KVToken> DescendantsAndSelf()
        {
            return GetDescendants(true);
        }

        internal IEnumerable<KVToken> GetDescendants(bool self)
        {
            if (self)
            {
                yield return this;
            }

            foreach (KVToken o in ChildrenTokens)
            {
                yield return o;
                if (o is KVContainer c)
                {
                    foreach (KVToken d in c.Descendants())
                    {
                        yield return d;
                    }
                }
            }
        }

        internal KVToken EnsureParentToken(KVToken item, bool skipParentCheck)
        {
            if (item == null)
            {
                return KVValue.CreateString(""); // idk, but it works for now
            }

            if (skipParentCheck)
            {
                return item;
            }

            // to avoid a token having multiple parents or creating a recursive loop, create a copy if...
            // the item already has a parent
            // the item is being added to itself
            // the item is being added to the root parent of itself
            if (item.Parent != null || item == this || (item.HasValues && Root == item))
            {
                item = item.CloneToken();
            }

            return item;
        }

        internal abstract int IndexOfItem(KVToken item);

        internal virtual void InsertItem(int index, KVToken item, bool skipParentCheck)
        {
            IList<KVToken> children = ChildrenTokens;

            if (index > children.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index must be within the bounds of the List.");
            }

            CheckReentrancy();

            item = EnsureParentToken(item, skipParentCheck);

            KVToken previous = (index == 0) ? null : children[index - 1];
            // haven't inserted new token yet so next token is still at the inserting index
            KVToken next = (index == children.Count) ? null : children[index];

            ValidateToken(item, null);

            item.Parent = this;

            item.Previous = previous;
            if (previous != null)
            {
                previous.Next = item;
            }

            item.Next = next;
            if (next != null)
            {
                next.Previous = item;
            }

            children.Insert(index, item);
            
            if (_collectionChanged != null)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }
        }


        internal virtual void RemoveItemAt(int index)
        {
            IList<KVToken> children = ChildrenTokens;

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is less than 0.");
            }
            if (index >= children.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is equal to or greater than Count.");
            }

            CheckReentrancy();

            KVToken item = children[index];
            KVToken previous = (index == 0) ? null : children[index - 1];
            KVToken next = (index == children.Count - 1) ? null : children[index + 1];

            if (previous != null)
            {
                previous.Next = next;
            }
            if (next != null)
            {
                next.Previous = previous;
            }

            item.Parent = null;
            item.Previous = null;
            item.Next = null;

            children.RemoveAt(index);
            
            if (_collectionChanged != null)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            }
        }

        internal virtual bool RemoveItem(KVToken item)
        {
            int index = IndexOfItem(item);
            if (index >= 0)
            {
                RemoveItemAt(index);
                return true;
            }

            return false;
        }

        internal virtual KVToken GetItem(int index)
        {
            return ChildrenTokens[index];
        }

        internal virtual void SetItem(int index, KVToken item)
        {
            IList<KVToken> children = ChildrenTokens;

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is less than 0.");
            }
            if (index >= children.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is equal to or greater than Count.");
            }

            KVToken existing = children[index];

            if (IsTokenUnchanged(existing, item))
            {
                return;
            }

            CheckReentrancy();

            item = EnsureParentToken(item, false);

            ValidateToken(item, existing);

            KVToken previous = (index == 0) ? null : children[index - 1];
            KVToken next = (index == children.Count - 1) ? null : children[index + 1];

            item.Parent = this;

            item.Previous = previous;
            if (previous != null)
            {
                previous.Next = item;
            }

            item.Next = next;
            if (next != null)
            {
                next.Previous = item;
            }

            children[index] = item;

            existing.Parent = null;
            existing.Previous = null;
            existing.Next = null;
            
            if (_collectionChanged != null)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, existing, index));
            }
        }

        internal virtual void ClearItems()
        {
            CheckReentrancy();

            IList<KVToken> children = ChildrenTokens;

            foreach (KVToken item in children)
            {
                item.Parent = null;
                item.Previous = null;
                item.Next = null;
            }

            children.Clear();
            
            if (_collectionChanged != null)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        internal virtual void ReplaceItem(KVToken existing, KVToken replacement)
        {
            if (existing == null || existing.Parent != this)
            {
                return;
            }

            int index = IndexOfItem(existing);
            SetItem(index, replacement);
        }

        internal virtual bool ContainsItem(KVToken item)
        {
            return (IndexOfItem(item) != -1);
        }

        internal virtual void CopyItemsTo(Array array, int arrayIndex)
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
            foreach (KVToken token in ChildrenTokens)
            {
                array.SetValue(token, arrayIndex + index);
                index++;
            }
        }

        internal static bool IsTokenUnchanged(KVToken currentValue, KVToken newValue)
        {
            if (currentValue is KVValue v1)
            {
                return v1.Equals(newValue);
            }

            return false;
        }

        internal virtual void ValidateToken(KVToken o, KVToken existing)
        {
            ValidationUtils.ArgumentNotNull(o, nameof(o));

            if (o.Type == KVTokenType.Property)
            {
                throw new ArgumentException("Can not add {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, o.GetType(), GetType()));
            }
        }

        /// <summary>
        /// Adds the specified content as children of this <see cref="KVToken"/>.
        /// </summary>
        /// <param name="content">The content to be added.</param>
        public virtual void Add(object content)
        {
            AddInternal(ChildrenTokens.Count, content, false);
        }

        internal void AddAndSkipParentCheck(KVToken token)
        {
            AddInternal(ChildrenTokens.Count, token, true);
        }

        /// <summary>
        /// Adds the specified content as the first children of this <see cref="KVToken"/>.
        /// </summary>
        /// <param name="content">The content to be added.</param>
        public void AddFirst(object content)
        {
            AddInternal(0, content, false);
        }

        internal void AddInternal(int index, object content, bool skipParentCheck)
        {
            KVToken item = CreateFromContent(content);

            InsertItem(index, item, skipParentCheck);
        }

        internal static KVToken CreateFromContent(object content)
        {
            if (content is KVToken token)
            {
                return token;
            }

            return new KVValue(content);
        }

        /// <summary>
        /// Creates a <see cref="KeyValueWriter"/> that can be used to add tokens to the <see cref="KVToken"/>.
        /// </summary>
        /// <returns>A <see cref="KeyValueWriter"/> that is ready to have content written to it.</returns>
        public KeyValueWriter CreateWriter()
        {
            return new KVTokenWriter(this);
        }

        /// <summary>
        /// Replaces the child nodes of this token with the specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        public void ReplaceAll(object content)
        {
            ClearItems();
            Add(content);
        }

        /// <summary>
        /// Removes the child nodes from this token.
        /// </summary>
        public void RemoveAll()
        {
            ClearItems();
        }

        internal abstract void MergeItem(object content, KeyValueMergeSettings settings);

        /// <summary>
        /// Merge the specified content into this <see cref="KVToken"/>.
        /// </summary>
        /// <param name="content">The content to be merged.</param>
        public void Merge(object content)
        {
            MergeItem(content, new KeyValueMergeSettings());
        }

        /// <summary>
        /// Merge the specified content into this <see cref="KVToken"/> using <see cref="KeyValueMergeSettings"/>.
        /// </summary>
        /// <param name="content">The content to be merged.</param>
        /// <param name="settings">The <see cref="KeyValueMergeSettings"/> used to merge the content.</param>
        public void Merge(object content, KeyValueMergeSettings settings)
        {
            MergeItem(content, settings);
        }

        internal void ReadTokenFrom(KeyValueReader reader, KeyValueLoadSettings options)
        {
            int startDepth = reader.Depth;

            if (!reader.Read())
            {
                throw KeyValueReaderException.Create(reader, "Error reading {0} from KeyValueReader.".FormatWith(CultureInfo.InvariantCulture, GetType().Name));
            }

            ReadContentFrom(reader, options);

            int endDepth = reader.Depth;

            if (endDepth > startDepth)
            {
                throw KeyValueReaderException.Create(reader, "Unexpected end of content while loading {0}.".FormatWith(CultureInfo.InvariantCulture, GetType().Name));
            }
        }

        internal void ReadContentFrom(KeyValueReader r, KeyValueLoadSettings settings)
        {
            ValidationUtils.ArgumentNotNull(r, nameof(r));
            IKeyValueLineInfo lineInfo = r as IKeyValueLineInfo;

            KVContainer parent = this;

            do
            {
                if ((parent as KVProperty)?.Value != null)
                {
                    if (parent == this)
                    {
                        return;
                    }

                    parent = parent.Parent;
                }

                switch (r.TokenType)
                {
                    case KeyValueToken.None:
                        // new reader. move to actual content
                        break;
                    case KeyValueToken.Start:
                        KVObject o = new KVObject();
                        o.SetLineInfo(lineInfo, settings);
                        parent.Add(o);
                        parent = o;
                        break;
                    case KeyValueToken.End:
                        if (parent == this)
                        {
                            return;
                        }

                        parent = parent.Parent;
                        break;
                    case KeyValueToken.String:
                    case KeyValueToken.Int32:
                    case KeyValueToken.Float32:
                    case KeyValueToken.Int64:
                    case KeyValueToken.UInt64:
                    case KeyValueToken.Color:
                    case KeyValueToken.Pointer:
                        KVValue v = new KVValue(r.Value);
                        v.SetLineInfo(lineInfo, settings);
                        parent.Add(v);
                        break;
                    case KeyValueToken.Comment:
                        if (settings != null && settings.CommentHandling == CommentHandling.Load)
                        {
                            v = KVValue.CreateComment(r.Value.ToString());
                            v.SetLineInfo(lineInfo, settings);
                            parent.Add(v);
                        }
                        break;
                    case KeyValueToken.PropertyName:
                        string propertyName = r.Value.ToString();
                        KVProperty property = new KVProperty(propertyName);
                        property.SetLineInfo(lineInfo, settings);
                        KVObject parentObject = (KVObject)parent;
                        // handle multiple properties with the same name in KeyValue
                        KVProperty existingPropertyWithName = parentObject.Property(propertyName);
                        if (existingPropertyWithName == null)
                        {
                            parent.Add(property);
                        }
                        else
                        {
                            existingPropertyWithName.Replace(property);
                        }
                        parent = property;
                        break;
                    default:
                        throw new InvalidOperationException("The KeyValueReader should not be on a token of type {0}.".FormatWith(CultureInfo.InvariantCulture, r.TokenType));
                }
            } while (r.Read());
        }

        internal int ContentsHashCode()
        {
            int hashCode = 0;
            foreach (KVToken item in ChildrenTokens)
            {
                hashCode ^= item.GetDeepHashCode();
            }
            return hashCode;
        }

        #region IList<KVToken> Members
        int IList<KVToken>.IndexOf(KVToken item)
        {
            return IndexOfItem(item);
        }

        void IList<KVToken>.Insert(int index, KVToken item)
        {
            InsertItem(index, item, false);
        }

        void IList<KVToken>.RemoveAt(int index)
        {
            RemoveItemAt(index);
        }

        KVToken IList<KVToken>.this[int index]
        {
            get { return GetItem(index); }
            set { SetItem(index, value); }
        }
        #endregion

        #region ICollection<KVToken> Members
        void ICollection<KVToken>.Add(KVToken item)
        {
            Add(item);
        }

        void ICollection<KVToken>.Clear()
        {
            ClearItems();
        }

        bool ICollection<KVToken>.Contains(KVToken item)
        {
            return ContainsItem(item);
        }

        void ICollection<KVToken>.CopyTo(KVToken[] array, int arrayIndex)
        {
            CopyItemsTo(array, arrayIndex);
        }

        bool ICollection<KVToken>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<KVToken>.Remove(KVToken item)
        {
            return RemoveItem(item);
        }
        #endregion

        private KVToken EnsureValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is KVToken token)
            {
                return token;
            }

            throw new ArgumentException("Argument is not a KVToken.");
        }

        #region IList Members
        int IList.Add(object value)
        {
            Add(EnsureValue(value));
            return Count - 1;
        }

        void IList.Clear()
        {
            ClearItems();
        }

        bool IList.Contains(object value)
        {
            return ContainsItem(EnsureValue(value));
        }

        int IList.IndexOf(object value)
        {
            return IndexOfItem(EnsureValue(value));
        }

        void IList.Insert(int index, object value)
        {
            InsertItem(index, EnsureValue(value), false);
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return false; }
        }

        void IList.Remove(object value)
        {
            RemoveItem(EnsureValue(value));
        }

        void IList.RemoveAt(int index)
        {
            RemoveItemAt(index);
        }

        object IList.this[int index]
        {
            get { return GetItem(index); }
            set { SetItem(index, EnsureValue(value)); }
        }
        #endregion

        #region ICollection Members
        void ICollection.CopyTo(Array array, int index)
        {
            CopyItemsTo(array, index);
        }

        /// <summary>
        /// Gets the count of child KeyValue tokens.
        /// </summary>
        /// <value>The count of child KeyValue tokens.</value>
        public int Count
        {
            get { return ChildrenTokens.Count; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    Interlocked.CompareExchange(ref _syncRoot, new object(), null);
                }

                return _syncRoot;
            }
        }
        #endregion
        
    }
}
