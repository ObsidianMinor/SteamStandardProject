using Steam.KeyValues.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Steam.KeyValues.Linq
{
    /// <summary>
    /// Represents an abstract KeyValue token
    /// </summary>
    public abstract class KVToken : IKVEnumerable<KVToken>, IKeyValueLineInfo, IDynamicMetaObjectProvider
    {
        private KVContainer _parent;
        private KVToken _previous;
        private KVToken _next;
        private object _annotations;

        /// <summary>
        /// Gets or sets the parent
        /// </summary>
        /// <value>The parent.</value>
        public KVContainer Parent
        {
            [DebuggerStepThrough]
            get => _parent;
            internal set => _parent = value;
        }

        /// <summary>
        /// Gets the root <see cref="KVToken"/> of this <see cref="KVToken"/>.
        /// </summary>
        public KVToken Root
        {
            get
            {
                KVContainer parent = Parent;
                if (parent == null)
                {
                    return this;
                }

                while (parent.Parent != null)
                {
                    parent = parent.Parent;
                }

                return parent;
            }
        }

        internal abstract KVToken CloneToken();
        internal abstract bool DeepEquals(KVToken node);

        /// <summary>
        /// Gets the node type for this <see cref="KVToken"/>
        /// </summary>
        public abstract KVTokenType Type { get; }
        
        /// <summary>
        /// Gets a value indicating whether this token has child tokens
        /// </summary>
        public abstract bool HasValues { get; }

        /// <summary>
        /// Compares the values of two tokens, including the values of all descendant tokens.
        /// </summary>
        /// <param name="t1">The first <see cref="KVToken"/> to compare.</param>
        /// <param name="t2">The second <see cref="KVToken"/> to compare.</param>
        /// <returns><c>true</c> if the tokens are equal; otherwise <c>false</c>.</returns>
        public static bool DeepEquals(KVToken t1, KVToken t2)
        {
            return (t1 == t2 || (t1 != null && t2 != null && t1.DeepEquals(t2)));
        }

        /// <summary>
        /// Gets the next sibling token of this node.
        /// </summary>
        /// <value>The <see cref="KVToken"/> that contains the next sibling token.</value>
        public KVToken Next
        {
            get { return _next; }
            internal set { _next = value; }
        }

        /// <summary>
        /// Gets the previous sibling token of this node.
        /// </summary>
        /// <value>The <see cref="KVToken"/> that contains the previous sibling token.</value>
        public KVToken Previous
        {
            get { return _previous; }
            internal set { _previous = value; }
        }

        /// <summary>
        /// Gets the path of the KeyValue token. 
        /// </summary>
        public string Path
        {
            get
            {
                if (Parent == null)
                {
                    return string.Empty;
                }

                List<KeyValuePosition> positions = new List<KeyValuePosition>();
                KVToken previous = null;
                for (KVToken current = this; current != null; current = current.Parent)
                {
                    KVProperty property = (KVProperty)current;
                    positions.Add(new KeyValuePosition(KeyValueContainerType.Object) { PropertyName = property.Name });

                    previous = current;
                }

                positions.Reverse();

                return KeyValuePosition.BuildPath(positions, null);
            }
        }

        internal KVToken()
        {
        }

        /// <summary>
        /// Adds the specified content immediately after this token.
        /// </summary>
        /// <param name="content">A content object that contains simple content or a collection of content objects to be added after this token.</param>
        public void AddAfterSelf(object content)
        {
            if (_parent == null)
            {
                throw new InvalidOperationException("The parent is missing.");
            }

            int index = _parent.IndexOfItem(this);
            _parent.AddInternal(index + 1, content, false);
        }

        /// <summary>
        /// Adds the specified content immediately before this token.
        /// </summary>
        /// <param name="content">A content object that contains simple content or a collection of content objects to be added before this token.</param>
        public void AddBeforeSelf(object content)
        {
            if (_parent == null)
            {
                throw new InvalidOperationException("The parent is missing.");
            }

            int index = _parent.IndexOfItem(this);
            _parent.AddInternal(index, content, false);
        }

        /// <summary>
        /// Returns a collection of the ancestor tokens of this token.
        /// </summary>
        /// <returns>A collection of the ancestor tokens of this token.</returns>
        public IEnumerable<KVToken> Ancestors()
        {
            return GetAncestors(false);
        }

        /// <summary>
        /// Returns a collection of tokens that contain this token, and the ancestors of this token.
        /// </summary>
        /// <returns>A collection of tokens that contain this token, and the ancestors of this token.</returns>
        public IEnumerable<KVToken> AncestorsAndSelf()
        {
            return GetAncestors(true);
        }

        internal IEnumerable<KVToken> GetAncestors(bool self)
        {
            for (KVToken current = self ? this : Parent; current != null; current = current.Parent)
            {
                yield return current;
            }
        }

        /// <summary>
        /// Returns a collection of the sibling tokens after this token, in document order.
        /// </summary>
        /// <returns>A collection of the sibling tokens after this tokens, in document order.</returns>
        public IEnumerable<KVToken> AfterSelf()
        {
            if (Parent == null)
            {
                yield break;
            }

            for (KVToken o = Next; o != null; o = o.Next)
            {
                yield return o;
            }
        }

        /// <summary>
        /// Returns a collection of the sibling tokens before this token, in document order.
        /// </summary>
        /// <returns>A collection of the sibling tokens before this token, in document order.</returns>
        public IEnumerable<KVToken> BeforeSelf()
        {
            for (KVToken o = Parent.First; o != this; o = o.Next)
            {
                yield return o;
            }
        }

        /// <summary>
        /// Gets the <see cref="KVToken"/> with the specified key.
        /// </summary>
        /// <value>The <see cref="KVToken"/> with the specified key.</value>
        public virtual KVToken this[object key]
        {
            get { throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType())); }
            set { throw new InvalidOperationException("Cannot set child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType())); }
        }

        /// <summary>
        /// Gets the <see cref="KVToken"/> with the specified key converted to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to convert the token to.</typeparam>
        /// <param name="key">The token key.</param>
        /// <returns>The converted token value.</returns>
        public virtual T Value<T>(object key)
        {
            KVToken token = this[key];

            // null check to fix MonoTouch issue - https://github.com/dolbz/Newtonsoft.Json/commit/a24e3062846b30ee505f3271ac08862bb471b822
            return token == null ? default : Extensions.Convert<KVToken, T>(token);
        }

        /// <summary>
        /// Get the first child token of this token.
        /// </summary>
        /// <value>A <see cref="KVToken"/> containing the first child token of the <see cref="KVToken"/>.</value>
        public virtual KVToken First
        {
            get { throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType())); }
        }

        /// <summary>
        /// Get the last child token of this token.
        /// </summary>
        /// <value>A <see cref="KVToken"/> containing the last child token of the <see cref="KVToken"/>.</value>
        public virtual KVToken Last
        {
            get { throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType())); }
        }

        /// <summary>
        /// Returns a collection of the child tokens of this token, in document order.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> containing the child tokens of this <see cref="KVToken"/>, in document order.</returns>
        public virtual KVEnumerable<KVToken> Children()
        {
            return KVEnumerable<KVToken>.Empty;
        }

        /// <summary>
        /// Returns a collection of the child tokens of this token, in document order, filtered by the specified type.
        /// </summary>
        /// <typeparam name="T">The type to filter the child tokens on.</typeparam>
        /// <returns>A <see cref="KVEnumerable{T}"/> containing the child tokens of this <see cref="KVToken"/>, in document order.</returns>
        public KVEnumerable<T> Children<T>() where T : KVToken
        {
            return new KVEnumerable<T>(Children().OfType<T>());
        }

        /// <summary>
        /// Returns a collection of the child values of this token, in document order.
        /// </summary>
        /// <typeparam name="T">The type to convert the values to.</typeparam>
        /// <returns>A <see cref="IEnumerable{T}"/> containing the child values of this <see cref="KVToken"/>, in document order.</returns>
        public virtual IEnumerable<T> Values<T>()
        {
            throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType()));
        }

        /// <summary>
        /// Removes this token from its parent.
        /// </summary>
        public void Remove()
        {
            if (_parent == null)
            {
                throw new InvalidOperationException("The parent is missing.");
            }

            _parent.RemoveItem(this);
        }

        /// <summary>
        /// Replaces this token with the specified token.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Replace(KVToken value)
        {
            if (_parent == null)
            {
                throw new InvalidOperationException("The parent is missing.");
            }

            _parent.ReplaceItem(this, value);
        }

        /// <summary>
        /// Writes this token to a <see cref="KeyValueWriter"/>.
        /// </summary>
        /// <param name="writer">A <see cref="KeyValueWriter"/> into which this method will write.</param>
        /// <param name="converters">A collection of <see cref="KeyValueConverter"/> which will be used when writing the token.</param>
        public abstract void WriteTo(KeyValueWriter writer, params KeyValueConverter[] converters);

        /// <summary>
        /// Returns the indented KeyValue for this token.
        /// </summary>
        /// <returns>
        /// The indented KeyValue for this token.
        /// </returns>
        public override string ToString()
        {
            return ToString(Formatting.Indented);
        }

        /// <summary>
        /// Returns the KeyValue for this token using the given formatting and converters.
        /// </summary>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="converters">A collection of <see cref="KeyValueConverter"/>s which will be used when writing the token.</param>
        /// <returns>The KeyValue for this token using the given formatting and converters.</returns>
        public string ToString(Formatting formatting, params KeyValueConverter[] converters)
        {
            using (StringWriter sw = new StringWriter(CultureInfo.InvariantCulture))
            {
                KeyValueTextWriter jw = new KeyValueTextWriter(sw)
                {
                    Formatting = formatting
                };

                WriteTo(jw, converters);

                return sw.ToString();
            }
        }

        private static KVValue EnsureValue(KVToken value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value is KVProperty)
            {
                value = ((KVProperty)value).Value;
            }

            KVValue v = value as KVValue;

            return v;
        }

        private static string GetType(KVToken token)
        {
            ValidationUtils.ArgumentNotNull(token, nameof(token));

            if (token is KVProperty)
            {
                token = ((KVProperty)token).Value;
            }

            return token.Type.ToString();
        }

        private static bool ValidateToken(KVToken o, KVTokenType[] validTypes)
        {
            return (Array.IndexOf(validTypes, o.Type) != -1);
        }
        
        // todo; casts

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KVToken>)this).GetEnumerator();
        }

        IEnumerator<KVToken> IEnumerable<KVToken>.GetEnumerator()
        {
            return Children().GetEnumerator();
        }

        internal abstract int GetDeepHashCode();

        IKVEnumerable<KVToken> IKVEnumerable<KVToken>.this[object key]
        {
            get { return this[key]; }
        }

        /// <summary>
        /// Creates a <see cref="KeyValueReader"/> for this token.
        /// </summary>
        /// <returns>A <see cref="KeyValueReader"/> that can be used to read this token and its descendants.</returns>
        public KeyValueReader CreateReader()
        {
            return new KVTokenReader(this);
        }

        internal static KVToken FromObjectInternal(object o, KeyValueSerializer KeyValueSerializer)
        {
            ValidationUtils.ArgumentNotNull(o, nameof(o));
            ValidationUtils.ArgumentNotNull(KeyValueSerializer, nameof(KeyValueSerializer));

            KVToken token;
            using (KVTokenWriter KeyValueWriter = new KVTokenWriter())
            {
                KeyValueSerializer.Serialize(KeyValueWriter, o);
                token = KeyValueWriter.Token;
            }

            return token;
        }

        /// <summary>
        /// Creates a <see cref="KVToken"/> from an object.
        /// </summary>
        /// <param name="o">The object that will be used to create <see cref="KVToken"/>.</param>
        /// <returns>A <see cref="KVToken"/> with the value of the specified object.</returns>
        public static KVToken FromObject(object o)
        {
            return FromObjectInternal(o, KeyValueSerializer.CreateDefault());
        }

        /// <summary>
        /// Creates a <see cref="KVToken"/> from an object using the specified <see cref="KeyValueSerializer"/>.
        /// </summary>
        /// <param name="o">The object that will be used to create <see cref="KVToken"/>.</param>
        /// <param name="KeyValueSerializer">The <see cref="KeyValueSerializer"/> that will be used when reading the object.</param>
        /// <returns>A <see cref="KVToken"/> with the value of the specified object.</returns>
        public static KVToken FromObject(object o, KeyValueSerializer KeyValueSerializer)
        {
            return FromObjectInternal(o, KeyValueSerializer);
        }

        /// <summary>
        /// Creates an instance of the specified .NET type from the <see cref="KVToken"/>.
        /// </summary>
        /// <typeparam name="T">The object type that the token will be deserialized to.</typeparam>
        /// <returns>The new object created from the KeyValue value.</returns>
        public T ToObject<T>()
        {
            return (T)ToObject(typeof(T));
        }

        /// <summary>
        /// Creates an instance of the specified .NET type from the <see cref="KVToken"/>.
        /// </summary>
        /// <param name="objectType">The object type that the token will be deserialized to.</param>
        /// <returns>The new object created from the KeyValue value.</returns>
        public object ToObject(Type objectType)
        {
            if (KeyValueConvert.DefaultSettings == null)
            {
                PrimitiveTypeCode typeCode = ConvertUtils.GetTypeCode(objectType, out bool isEnum);

                if (isEnum)
                {
                    if (Type == KVTokenType.String)
                    {
                        try
                        {
                            // use serializer so KeyValueConverter(typeof(StringEnumConverter)) + EnumMemberAttributes are respected
                            return ToObject(objectType, KeyValueSerializer.CreateDefault());
                        }
                        catch (Exception ex)
                        {
                            Type enumType = objectType.IsEnum() ? objectType : Nullable.GetUnderlyingType(objectType);
                            throw new ArgumentException("Could not convert '{0}' to {1}.".FormatWith(CultureInfo.InvariantCulture, (string)this, enumType.Name), ex);
                        }
                    }

                    if (Type == KVTokenType.Int32 || Type == KVTokenType.Int64 || Type == KVTokenType.UInt64)
                    {
                        Type enumType = objectType.IsEnum() ? objectType : Nullable.GetUnderlyingType(objectType);
                        return Enum.ToObject(enumType, ((KVValue)this).Value);
                    }
                }

                switch (typeCode)
                {
                    case PrimitiveTypeCode.CharNullable:
                        return (char?)this;
                    case PrimitiveTypeCode.Char:
                        return (char)this;
                    case PrimitiveTypeCode.SByte:
                        return (sbyte)this;
                    case PrimitiveTypeCode.SByteNullable:
                        return (sbyte?)this;
                    case PrimitiveTypeCode.ByteNullable:
                        return (byte?)this;
                    case PrimitiveTypeCode.Byte:
                        return (byte)this;
                    case PrimitiveTypeCode.Int16Nullable:
                        return (short?)this;
                    case PrimitiveTypeCode.Int16:
                        return (short)this;
                    case PrimitiveTypeCode.UInt16Nullable:
                        return (ushort?)this;
                    case PrimitiveTypeCode.UInt16:
                        return (ushort)this;
                    case PrimitiveTypeCode.Int32Nullable:
                        return (int?)this;
                    case PrimitiveTypeCode.Int32:
                        return (int)this;
                    case PrimitiveTypeCode.UInt32Nullable:
                        return (uint?)this;
                    case PrimitiveTypeCode.UInt32:
                        return (uint)this;
                    case PrimitiveTypeCode.Int64Nullable:
                        return (long?)this;
                    case PrimitiveTypeCode.Int64:
                        return (long)this;
                    case PrimitiveTypeCode.UInt64Nullable:
                        return (ulong?)this;
                    case PrimitiveTypeCode.UInt64:
                        return (ulong)this;
                    case PrimitiveTypeCode.SingleNullable:
                        return (float?)this;
                    case PrimitiveTypeCode.Single:
                        return (float)this;
                    case PrimitiveTypeCode.DoubleNullable:
                        return (double?)this;
                    case PrimitiveTypeCode.Double:
                        return (double)this;
                    case PrimitiveTypeCode.DecimalNullable:
                        return (decimal?)this;
                    case PrimitiveTypeCode.Decimal:
                        return (decimal)this;
                    case PrimitiveTypeCode.String:
                        return (string)this;
                }
            }

            return ToObject(objectType, KeyValueSerializer.CreateDefault());
        }

        /// <summary>
        /// Creates an instance of the specified .NET type from the <see cref="KVToken"/> using the specified <see cref="KeyValueSerializer"/>.
        /// </summary>
        /// <typeparam name="T">The object type that the token will be deserialized to.</typeparam>
        /// <param name="KeyValueSerializer">The <see cref="KeyValueSerializer"/> that will be used when creating the object.</param>
        /// <returns>The new object created from the KeyValue value.</returns>
        public T ToObject<T>(KeyValueSerializer KeyValueSerializer)
        {
            return (T)ToObject(typeof(T), KeyValueSerializer);
        }

        /// <summary>
        /// Creates an instance of the specified .NET type from the <see cref="KVToken"/> using the specified <see cref="KeyValueSerializer"/>.
        /// </summary>
        /// <param name="objectType">The object type that the token will be deserialized to.</param>
        /// <param name="KeyValueSerializer">The <see cref="KeyValueSerializer"/> that will be used when creating the object.</param>
        /// <returns>The new object created from the KeyValue value.</returns>
        public object ToObject(Type objectType, KeyValueSerializer KeyValueSerializer)
        {
            ValidationUtils.ArgumentNotNull(KeyValueSerializer, nameof(KeyValueSerializer));

            using (KVTokenReader KeyValueReader = new KVTokenReader(this))
            {
                return KeyValueSerializer.Deserialize(KeyValueReader, objectType);
            }
        }

        /// <summary>
        /// Creates a <see cref="KVToken"/> from a <see cref="KeyValueReader"/>.
        /// </summary>
        /// <param name="reader">A <see cref="KeyValueReader"/> positioned at the token to read into this <see cref="KVToken"/>.</param>
        /// <returns>
        /// A <see cref="KVToken"/> that contains the token and its descendant tokens
        /// that were read from the reader. The runtime type of the token is determined
        /// by the token type of the first token encountered in the reader.
        /// </returns>
        public static KVToken ReadFrom(KeyValueReader reader)
        {
            return ReadFrom(reader, null);
        }

        /// <summary>
        /// Creates a <see cref="KVToken"/> from a <see cref="KeyValueReader"/>.
        /// </summary>
        /// <param name="reader">An <see cref="KeyValueReader"/> positioned at the token to read into this <see cref="KVToken"/>.</param>
        /// <param name="settings">The <see cref="KeyValueLoadSettings"/> used to load the KeyValue.
        /// If this is <c>null</c>, default load settings will be used.</param>
        /// <returns>
        /// A <see cref="KVToken"/> that contains the token and its descendant tokens
        /// that were read from the reader. The runtime type of the token is determined
        /// by the token type of the first token encountered in the reader.
        /// </returns>
        public static KVToken ReadFrom(KeyValueReader reader, KeyValueLoadSettings settings)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            bool hasContent;
            if (reader.TokenType == KeyValueToken.None)
            {
                hasContent = (settings != null && settings.CommentHandling == CommentHandling.Ignore)
                    ? reader.ReadAndMoveToContent()
                    : reader.Read();
            }
            else if (reader.TokenType == KeyValueToken.Comment && settings?.CommentHandling == CommentHandling.Ignore)
            {
                hasContent = reader.ReadAndMoveToContent();
            }
            else
            {
                hasContent = true;
            }

            if (!hasContent)
            {
                throw KeyValueReaderException.Create(reader, "Error reading KVToken from KeyValueReader.");
            }

            IKeyValueLineInfo lineInfo = reader as IKeyValueLineInfo;

            switch (reader.TokenType)
            {
                case KeyValueToken.Start:
                case KeyValueToken.Base:
                    return KVObject.Load(reader, settings);
                case KeyValueToken.PropertyName:
                    return KVProperty.Load(reader, settings);
                case KeyValueToken.String:
                case KeyValueToken.Int32:
                case KeyValueToken.Float32:
                case KeyValueToken.Int64:
                case KeyValueToken.UInt64:
                case KeyValueToken.Pointer:
                case KeyValueToken.Color:
                case KeyValueToken.WideString:
                    KVValue v = new KVValue(reader.Value);
                    v.SetLineInfo(lineInfo, settings);
                    return v;
                case KeyValueToken.Comment:
                    v = KVValue.CreateComment(reader.Value.ToString());
                    v.SetLineInfo(lineInfo, settings);
                    return v;
                default:
                    throw KeyValueReaderException.Create(reader, "Error reading KVToken from KeyValueReader. Unexpected token: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }
        }

        /// <summary>
        /// Load a <see cref="KVToken"/> from a string that contains KeyValue.
        /// </summary>
        /// <param name="KeyValue">A <see cref="String"/> that contains KeyValue.</param>
        /// <returns>A <see cref="KVToken"/> populated from the string that contains KeyValue.</returns>
        public static KVToken Parse(string KeyValue)
        {
            return Parse(KeyValue, null);
        }

        /// <summary>
        /// Load a <see cref="KVToken"/> from a string that contains KeyValue.
        /// </summary>
        /// <param name="keyValue">A <see cref="String"/> that contains KeyValue.</param>
        /// <param name="settings">The <see cref="KeyValueLoadSettings"/> used to load the KeyValue.
        /// If this is <c>null</c>, default load settings will be used.</param>
        /// <returns>A <see cref="KVToken"/> populated from the string that contains KeyValue.</returns>
        public static KVToken Parse(string keyValue, KeyValueLoadSettings settings)
        {
            using (KeyValueReader reader = new KeyValueTextReader(new StringReader(keyValue)))
            {
                KVToken t = Load(reader, settings);

                while (reader.Read())
                {
                    // Any content encountered here other than a comment will throw in the reader.
                }


                return t;
            }
        }

        /// <summary>
        /// Creates a <see cref="KVToken"/> from a <see cref="KeyValueReader"/>.
        /// </summary>
        /// <param name="reader">A <see cref="KeyValueReader"/> positioned at the token to read into this <see cref="KVToken"/>.</param>
        /// <param name="settings">The <see cref="KeyValueLoadSettings"/> used to load the KeyValue.
        /// If this is <c>null</c>, default load settings will be used.</param>
        /// <returns>
        /// A <see cref="KVToken"/> that contains the token and its descendant tokens
        /// that were read from the reader. The runtime type of the token is determined
        /// by the token type of the first token encountered in the reader.
        /// </returns>
        public static KVToken Load(KeyValueReader reader, KeyValueLoadSettings settings)
        {
            return ReadFrom(reader, settings);
        }

        /// <summary>
        /// Creates a <see cref="KVToken"/> from a <see cref="KeyValueReader"/>.
        /// </summary>
        /// <param name="reader">A <see cref="KeyValueReader"/> positioned at the token to read into this <see cref="KVToken"/>.</param>
        /// <returns>
        /// A <see cref="KVToken"/> that contains the token and its descendant tokens
        /// that were read from the reader. The runtime type of the token is determined
        /// by the token type of the first token encountered in the reader.
        /// </returns>
        public static KVToken Load(KeyValueReader reader)
        {
            return Load(reader, null);
        }

        internal void SetLineInfo(IKeyValueLineInfo lineInfo, KeyValueLoadSettings settings)
        {
            if (settings != null && settings.LineInfoHandling != LineInfoHandling.Load)
            {
                return;
            }

            if (lineInfo == null || !lineInfo.HasLineInfo())
            {
                return;
            }

            SetLineInfo(lineInfo.LineNumber, lineInfo.LinePosition);
        }

        private class LineInfoAnnotation
        {
            internal readonly int LineNumber;
            internal readonly int LinePosition;

            public LineInfoAnnotation(int lineNumber, int linePosition)
            {
                LineNumber = lineNumber;
                LinePosition = linePosition;
            }
        }

        internal void SetLineInfo(int lineNumber, int linePosition)
        {
            AddAnnotation(new LineInfoAnnotation(lineNumber, linePosition));
        }

        bool IKeyValueLineInfo.HasLineInfo()
        {
            return (Annotation<LineInfoAnnotation>() != null);
        }

        int IKeyValueLineInfo.LineNumber
        {
            get
            {
                LineInfoAnnotation annotation = Annotation<LineInfoAnnotation>();
                if (annotation != null)
                {
                    return annotation.LineNumber;
                }

                return 0;
            }
        }

        int IKeyValueLineInfo.LinePosition
        {
            get
            {
                LineInfoAnnotation annotation = Annotation<LineInfoAnnotation>();
                if (annotation != null)
                {
                    return annotation.LinePosition;
                }

                return 0;
            }
        }

        /// <summary>
        /// Selects a <see cref="KVToken"/> using a KVPath expression. Selects the token that matches the object path.
        /// </summary>
        /// <param name="path">
        /// A <see cref="String"/> that contains a KVPath expression.
        /// </param>
        /// <returns>A <see cref="KVToken"/>, or <c>null</c>.</returns>
        public KVToken SelectToken(string path)
        {
            return SelectToken(path, false);
        }

        /// <summary>
        /// Selects a <see cref="KVToken"/> using a KVPath expression. Selects the token that matches the object path.
        /// </summary>
        /// <param name="path">
        /// A <see cref="String"/> that contains a KVPath expression.
        /// </param>
        /// <param name="errorWhenNoMatch">A flag to indicate whether an error should be thrown if no tokens are found when evaluating part of the expression.</param>
        /// <returns>A <see cref="KVToken"/>.</returns>
        public KVToken SelectToken(string path, bool errorWhenNoMatch)
        {
            KVPath p = new KVPath(path);

            KVToken token = null;
            foreach (KVToken t in p.Evaluate(this, this, errorWhenNoMatch))
            {
                if (token != null)
                {
                    throw new KeyValueException("Path returned multiple tokens.");
                }

                token = t;
            }

            return token;
        }

        /// <summary>
        /// Selects a collection of elements using a KVPath expression.
        /// </summary>
        /// <param name="path">
        /// A <see cref="String"/> that contains a KVPath expression.
        /// </param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains the selected elements.</returns>
        public IEnumerable<KVToken> SelectTokens(string path)
        {
            return SelectTokens(path, false);
        }

        /// <summary>
        /// Selects a collection of elements using a KVPath expression.
        /// </summary>
        /// <param name="path">
        /// A <see cref="String"/> that contains a KVPath expression.
        /// </param>
        /// <param name="errorWhenNoMatch">A flag to indicate whether an error should be thrown if no tokens are found when evaluating part of the expression.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="KVToken"/> that contains the selected elements.</returns>
        public IEnumerable<KVToken> SelectTokens(string path, bool errorWhenNoMatch)
        {
            KVPath p = new KVPath(path);
            return p.Evaluate(this, this, errorWhenNoMatch);
        }
        
        /// <summary>
        /// Returns the <see cref="DynamicMetaObject"/> responsible for binding operations performed on this object.
        /// </summary>
        /// <param name="parameter">The expression tree representation of the runtime value.</param>
        /// <returns>
        /// The <see cref="DynamicMetaObject"/> to bind this object.
        /// </returns>
        protected virtual DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new DynamicProxyMetaObject<KVToken>(parameter, this, new DynamicProxy<KVToken>());
        }

        /// <summary>
        /// Returns the <see cref="DynamicMetaObject"/> responsible for binding operations performed on this object.
        /// </summary>
        /// <param name="parameter">The expression tree representation of the runtime value.</param>
        /// <returns>
        /// The <see cref="DynamicMetaObject"/> to bind this object.
        /// </returns>
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return GetMetaObject(parameter);
        }
        
        /// <summary>
        /// Creates a new instance of the <see cref="KVToken"/>. All child tokens are recursively cloned.
        /// </summary>
        /// <returns>A new instance of the <see cref="KVToken"/>.</returns>
        public KVToken DeepClone()
        {
            return CloneToken();
        }

        /// <summary>
        /// Adds an object to the annotation list of this <see cref="KVToken"/>.
        /// </summary>
        /// <param name="annotation">The annotation to add.</param>
        public void AddAnnotation(object annotation)
        {
            if (annotation == null)
            {
                throw new ArgumentNullException(nameof(annotation));
            }

            if (_annotations == null)
            {
                _annotations = (annotation is object[]) ? new[] { annotation } : annotation;
            }
            else
            {
                object[] annotations = _annotations as object[];
                if (annotations == null)
                {
                    _annotations = new[] { _annotations, annotation };
                }
                else
                {
                    int index = 0;
                    while (index < annotations.Length && annotations[index] != null)
                    {
                        index++;
                    }
                    if (index == annotations.Length)
                    {
                        Array.Resize(ref annotations, index * 2);
                        _annotations = annotations;
                    }
                    annotations[index] = annotation;
                }
            }
        }

        /// <summary>
        /// Get the first annotation object of the specified type from this <see cref="KVToken"/>.
        /// </summary>
        /// <typeparam name="T">The type of the annotation to retrieve.</typeparam>
        /// <returns>The first annotation object that matches the specified type, or <c>null</c> if no annotation is of the specified type.</returns>
        public T Annotation<T>() where T : class
        {
            if (_annotations != null)
            {
                object[] annotations = _annotations as object[];
                if (annotations == null)
                {
                    return (_annotations as T);
                }
                for (int i = 0; i < annotations.Length; i++)
                {
                    object annotation = annotations[i];
                    if (annotation == null)
                    {
                        break;
                    }

                    if (annotation is T local)
                    {
                        return local;
                    }
                }
            }

            return default;
        }

        /// <summary>
        /// Gets the first annotation object of the specified type from this <see cref="KVToken"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of the annotation to retrieve.</param>
        /// <returns>The first annotation object that matches the specified type, or <c>null</c> if no annotation is of the specified type.</returns>
        public object Annotation(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (_annotations != null)
            {
                object[] annotations = _annotations as object[];
                if (annotations == null)
                {
                    if (type.IsInstanceOfType(_annotations))
                    {
                        return _annotations;
                    }
                }
                else
                {
                    for (int i = 0; i < annotations.Length; i++)
                    {
                        object o = annotations[i];
                        if (o == null)
                        {
                            break;
                        }

                        if (type.IsInstanceOfType(o))
                        {
                            return o;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a collection of annotations of the specified type for this <see cref="KVToken"/>.
        /// </summary>
        /// <typeparam name="T">The type of the annotations to retrieve.</typeparam>
        /// <returns>An <see cref="IEnumerable{T}"/> that contains the annotations for this <see cref="KVToken"/>.</returns>
        public IEnumerable<T> Annotations<T>() where T : class
        {
            if (_annotations == null)
            {
                yield break;
            }

            if (_annotations is object[] annotations)
            {
                for (int i = 0; i < annotations.Length; i++)
                {
                    object o = annotations[i];
                    if (o == null)
                    {
                        break;
                    }

                    if (o is T casted)
                    {
                        yield return casted;
                    }
                }
                yield break;
            }

            T annotation = _annotations as T;
            if (annotation == null)
            {
                yield break;
            }

            yield return annotation;
        }

        /// <summary>
        /// Gets a collection of annotations of the specified type for this <see cref="KVToken"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of the annotations to retrieve.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Object"/> that contains the annotations that match the specified type for this <see cref="KVToken"/>.</returns>
        public IEnumerable<object> Annotations(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (_annotations == null)
            {
                yield break;
            }

            if (_annotations is object[] annotations)
            {
                for (int i = 0; i < annotations.Length; i++)
                {
                    object o = annotations[i];
                    if (o == null)
                    {
                        break;
                    }

                    if (type.IsInstanceOfType(o))
                    {
                        yield return o;
                    }
                }
                yield break;
            }

            if (!type.IsInstanceOfType(_annotations))
            {
                yield break;
            }

            yield return _annotations;
        }

        /// <summary>
        /// Removes the annotations of the specified type from this <see cref="KVToken"/>.
        /// </summary>
        /// <typeparam name="T">The type of annotations to remove.</typeparam>
        public void RemoveAnnotations<T>() where T : class
        {
            if (_annotations != null)
            {
                object[] annotations = _annotations as object[];
                if (annotations == null)
                {
                    if (_annotations is T)
                    {
                        _annotations = null;
                    }
                }
                else
                {
                    int index = 0;
                    int keepCount = 0;
                    while (index < annotations.Length)
                    {
                        object obj2 = annotations[index];
                        if (obj2 == null)
                        {
                            break;
                        }

                        if (!(obj2 is T))
                        {
                            annotations[keepCount++] = obj2;
                        }

                        index++;
                    }

                    if (keepCount != 0)
                    {
                        while (keepCount < index)
                        {
                            annotations[keepCount++] = null;
                        }
                    }
                    else
                    {
                        _annotations = null;
                    }
                }
            }
        }

        /// <summary>
        /// Removes the annotations of the specified type from this <see cref="KVToken"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of annotations to remove.</param>
        public void RemoveAnnotations(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (_annotations != null)
            {
                object[] annotations = _annotations as object[];
                if (annotations == null)
                {
                    if (type.IsInstanceOfType(_annotations))
                    {
                        _annotations = null;
                    }
                }
                else
                {
                    int index = 0;
                    int keepCount = 0;
                    while (index < annotations.Length)
                    {
                        object o = annotations[index];
                        if (o == null)
                        {
                            break;
                        }

                        if (!type.IsInstanceOfType(o))
                        {
                            annotations[keepCount++] = o;
                        }

                        index++;
                    }

                    if (keepCount != 0)
                    {
                        while (keepCount < index)
                        {
                            annotations[keepCount++] = null;
                        }
                    }
                    else
                    {
                        _annotations = null;
                    }
                }
            }
        }
    }
}