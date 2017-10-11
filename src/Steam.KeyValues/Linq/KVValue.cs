using Steam.KeyValues.Utilities;
using System;
using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;

namespace Steam.KeyValues.Linq
{
    public partial class KVValue : KVToken, IEquatable<KVValue>, IFormattable, IComparable, IComparable<KVValue>
    {
        private KVTokenType _valueType;
        private object _value;

        internal KVValue(object value, KVTokenType type)
        {
            _value = value;
            _valueType = type;
        }

        public KVValue(KVValue other) : this(other.Value, other.Type) { }

        public KVValue(long value) : this(value, KVTokenType.Int64) { }

        public KVValue(decimal value) : this(value, KVTokenType.Float) { }
        
        public KVValue(char value) : this(value, KVTokenType.String) { }

        [CLSCompliant(false)]
        public KVValue(ulong value) : this(value, KVTokenType.UInt64) { }

        public KVValue(double value) : this(value, KVTokenType.Float) { }

        public KVValue(float value) : this(value, KVTokenType.Float) { }

        public KVValue(string value) : this(value, KVTokenType.String) { }

        public KVValue(object value) : this(value ?? "", GetValueType(null, value)) { }

        internal override bool DeepEquals(KVToken node)
        {
            KVValue other = node as KVValue;
            if (other == null)
            {
                return false;
            }
            if (other == this)
            {
                return true;
            }

            return ValuesEquals(this, other);
        }

        internal static int Compare(KVTokenType valueType, object objA, object objB)
        {
            if (objA == objB)
            {
                return 0;
            }
            if (objB == null)
            {
                return 1;
            }
            if (objA == null)
            {
                return -1;
            }

            switch (valueType)
            {
                case KVTokenType.Int32:
                case KVTokenType.Int64:
                case KVTokenType.UInt64:
                    if (objA is ulong || objB is ulong || objA is decimal || objB is decimal)
                    {
                        return Convert.ToDecimal(objA, CultureInfo.InvariantCulture).CompareTo(Convert.ToDecimal(objB, CultureInfo.InvariantCulture));
                    }
                    else if (objA is float || objB is float || objA is double || objB is double)
                    {
                        return CompareFloat(objA, objB);
                    }
                    else
                    {
                        return Convert.ToInt64(objA, CultureInfo.InvariantCulture).CompareTo(Convert.ToInt64(objB, CultureInfo.InvariantCulture));
                    }
                case KVTokenType.Float:
                    if (objA is ulong || objB is ulong || objA is decimal || objB is decimal)
                    {
                        return Convert.ToDecimal(objA, CultureInfo.InvariantCulture).CompareTo(Convert.ToDecimal(objB, CultureInfo.InvariantCulture));
                    }
                    return CompareFloat(objA, objB);
                case KVTokenType.Pointer:
                    if(objA is IntPtr || objB is IntPtr || objA is UIntPtr || objB is UIntPtr)
                    {
                        return ComparePointers(objA, objB);
                    }
                    else if (objA is int || objB is int || objA is long || objB is long)
                    {
                        return Convert.ToInt64(objA, CultureInfo.InvariantCulture).CompareTo(Convert.ToInt64(objB, CultureInfo.InvariantCulture));
                    }
                    else if (objA is uint || objB is uint || objA is ulong || objB is ulong)
                    {
                        return Convert.ToUInt64(objA, CultureInfo.InvariantCulture).CompareTo(Convert.ToUInt64(objB, CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        return Convert.ToInt64(objA, CultureInfo.InvariantCulture).CompareTo(Convert.ToInt64(objB, CultureInfo.InvariantCulture));
                    }
                case KVTokenType.Comment:
                case KVTokenType.String:
                case KVTokenType.Raw:
                    string s1 = Convert.ToString(objA, CultureInfo.InvariantCulture);
                    string s2 = Convert.ToString(objB, CultureInfo.InvariantCulture);

                    return string.CompareOrdinal(s1, s2);
                default:
                    throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(valueType), valueType, "Unexpected value type: {0}".FormatWith(CultureInfo.InvariantCulture, valueType));
            }
        }

        private static int CompareFloat(object objA, object objB)
        {
            double d1 = Convert.ToDouble(objA, CultureInfo.InvariantCulture);
            double d2 = Convert.ToDouble(objB, CultureInfo.InvariantCulture);

            // take into account possible floating point errors
            if (MathUtils.ApproxEquals(d1, d2))
            {
                return 0;
            }

            return d1.CompareTo(d2);
        }

        private static int ComparePointers(object objA, object objB)
        {
            throw new NotImplementedException();
        }

        private static bool Operation(ExpressionType operation, object objA, object objB, out object result)
        {
            if (objA is string || objB is string)
            {
                if (operation == ExpressionType.Add || operation == ExpressionType.AddAssign)
                {
                    result = objA?.ToString() + objB?.ToString();
                    return true;
                }
            }

            if (objA is ulong || objB is ulong || objA is decimal || objB is decimal)
            {
                if (objA == null || objB == null)
                {
                    result = null;
                    return true;
                }

                decimal d1 = Convert.ToDecimal(objA, CultureInfo.InvariantCulture);
                decimal d2 = Convert.ToDecimal(objB, CultureInfo.InvariantCulture);

                switch (operation)
                {
                    case ExpressionType.Add:
                    case ExpressionType.AddAssign:
                        result = d1 + d2;
                        return true;
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractAssign:
                        result = d1 - d2;
                        return true;
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyAssign:
                        result = d1 * d2;
                        return true;
                    case ExpressionType.Divide:
                    case ExpressionType.DivideAssign:
                        result = d1 / d2;
                        return true;
                }
            }
            else if (objA is float || objB is float || objA is double || objB is double)
            {
                if (objA == null || objB == null)
                {
                    result = null;
                    return true;
                }

                double d1 = Convert.ToDouble(objA, CultureInfo.InvariantCulture);
                double d2 = Convert.ToDouble(objB, CultureInfo.InvariantCulture);

                switch (operation)
                {
                    case ExpressionType.Add:
                    case ExpressionType.AddAssign:
                        result = d1 + d2;
                        return true;
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractAssign:
                        result = d1 - d2;
                        return true;
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyAssign:
                        result = d1 * d2;
                        return true;
                    case ExpressionType.Divide:
                    case ExpressionType.DivideAssign:
                        result = d1 / d2;
                        return true;
                }
            }
            else if (objA is int || objA is uint || objA is long || objA is short || objA is ushort || objA is sbyte || objA is byte ||
                     objB is int || objB is uint || objB is long || objB is short || objB is ushort || objB is sbyte || objB is byte)
            {
                if (objA == null || objB == null)
                {
                    result = null;
                    return true;
                }

                long l1 = Convert.ToInt64(objA, CultureInfo.InvariantCulture);
                long l2 = Convert.ToInt64(objB, CultureInfo.InvariantCulture);

                switch (operation)
                {
                    case ExpressionType.Add:
                    case ExpressionType.AddAssign:
                        result = l1 + l2;
                        return true;
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractAssign:
                        result = l1 - l2;
                        return true;
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyAssign:
                        result = l1 * l2;
                        return true;
                    case ExpressionType.Divide:
                    case ExpressionType.DivideAssign:
                        result = l1 / l2;
                        return true;
                }
            }

            result = null;
            return false;
        }

        internal override KVToken CloneToken()
        {
            return new KVValue(this);
        }

        /// <summary>
        /// Gets a value indicating whether this token has child tokens
        /// </summary>
        public override bool HasValues => false;

        /// <summary>
        /// Creates a <see cref="KVValue"/> comment with the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="KVValue"/> comment with the given value.</returns>
        public static KVValue CreateComment(string value)
        {
            return new KVValue(value, KVTokenType.Comment);
        }

        /// <summary>
        /// Creates a <see cref="KVValue"/> string with the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="KVValue"/> string with the given value.</returns>
        public static KVValue CreateString(string value)
        {
            return new KVValue(value, KVTokenType.String);
        }

        private static KVTokenType GetValueType(KVTokenType? current, object value)
        {
            if (value == null)
            {
                return KVTokenType.String;
            }
            else if (value is string)
            {
                return GetStringValueType(current);
            }
            else if (value is int || value is short || value is sbyte || value is ushort || value is byte)
            {
                return KVTokenType.Int32;
            }
            else if(value is long || value is uint)
            {
                return KVTokenType.Int64;
            }
            else if(value is ulong)
            {
                return KVTokenType.UInt64;
            }
            else if (value is IntPtr || value is UIntPtr)
            {
                return KVTokenType.Pointer;
            }
            else if (value is double || value is float || value is decimal)
            {
                return KVTokenType.Float;
            }

            throw new ArgumentException("Could not determine KeyValue object type for type {0}.".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
        }

        private static KVTokenType GetStringValueType(KVTokenType? current)
        {
            if (current == null)
            {
                return KVTokenType.String;
            }

            switch (current.GetValueOrDefault())
            {
                case KVTokenType.Comment:
                case KVTokenType.String:
                case KVTokenType.Raw:
                    return current.GetValueOrDefault();
                default:
                    return KVTokenType.String;
            }
        }

        /// <summary>
        /// Gets the node type for this <see cref="KVToken"/>.
        /// </summary>
        /// <value>The type.</value>
        public override KVTokenType Type
        {
            get { return _valueType; }
        }

        /// <summary>
        /// Gets or sets the underlying token value.
        /// </summary>
        /// <value>The underlying token value.</value>
        public object Value
        {
            get { return _value; }
            set
            {
                Type currentType = _value?.GetType();
                Type newType = value?.GetType();

                if (currentType != newType)
                {
                    _valueType = GetValueType(_valueType, value);
                }

                _value = value;
            }
        }

        /// <summary>
        /// Writes this token to a <see cref="KeyValueWriter"/>.
        /// </summary>
        /// <param name="writer">A <see cref="KeyValueWriter"/> into which this method will write.</param>
        /// <param name="converters">A collection of <see cref="KeyValueConverter"/>s which will be used when writing the token.</param>
        public override void WriteTo(KeyValueWriter writer, params KeyValueConverter[] converters)
        {
            if (converters != null && converters.Length > 0 && _value != null)
            {
                KeyValueConverter matchingConverter = KeyValueSerializer.GetMatchingConverter(converters, _value.GetType());
                if (matchingConverter != null && matchingConverter.CanWrite)
                {
                    matchingConverter.WriteKeyValue(writer, _value, KeyValueSerializer.CreateDefault());
                    return;
                }
            }

            switch (_valueType)
            {
                case KVTokenType.Comment:
                    writer.WriteComment(_value?.ToString());
                    break;
                case KVTokenType.Raw:
                    writer.WriteRawValue(_value?.ToString());
                    break;
                case KVTokenType.Int32:
                    writer.WriteValue((int)_value);
                    break;
                case KVTokenType.Int64:
                    writer.WriteValue((long)_value);
                    break;
                case KVTokenType.UInt64:
                    writer.WriteValue(Convert.ToInt64(_value, CultureInfo.InvariantCulture));
                    break;
                case KVTokenType.Float:
                    if (_value is decimal)
                    {
                        writer.WriteValue((decimal)_value);
                    }
                    else if (_value is double)
                    {
                        writer.WriteValue((double)_value);
                    }
                    else if (_value is float)
                    {
                        writer.WriteValue((float)_value);
                    }
                    else
                    {
                        writer.WriteValue(Convert.ToDouble(_value, CultureInfo.InvariantCulture));
                    }
                    break;
                case KVTokenType.String:
                    writer.WriteValue(_value?.ToString());
                    break;
                default:
                    throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(Type), _valueType, "Unexpected token type.");
            }
            
        }

        internal override int GetDeepHashCode()
        {
            int valueHashCode = (_value != null) ? _value.GetHashCode() : 0;

            // GetHashCode on an enum boxes so cast to int
            return ((int)_valueType).GetHashCode() ^ valueHashCode;
        }

        private static bool ValuesEquals(KVValue v1, KVValue v2)
        {
            return (v1 == v2 || (v1._valueType == v2._valueType && Compare(v1._valueType, v1._value, v2._value) == 0));
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(KVValue other)
        {
            if (other == null)
            {
                return false;
            }

            return ValuesEquals(this, other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Object"/>.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Object"/> is equal to the current <see cref="Object"/>; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as KVValue);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            if (_value == null)
            {
                return 0;
            }

            return _value.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (_value == null)
            {
                return string.Empty;
            }

            return _value.ToString();
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="String"/> that represents this instance.
        /// </returns>
        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this instance.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="String"/> that represents this instance.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return ToString(null, formatProvider);
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="String"/> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (_value == null)
            {
                return string.Empty;
            }

            if (_value is IFormattable formattable)
            {
                return formattable.ToString(format, formatProvider);
            }
            else
            {
                return _value.ToString();
            }
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
            return new DynamicProxyMetaObject<KVValue>(parameter, this, new KVValueDynamicProxy());
        }

        private class KVValueDynamicProxy : DynamicProxy<KVValue>
        {
            public override bool TryConvert(KVValue instance, ConvertBinder binder, out object result)
            {
                if (binder.Type == typeof(KVValue) || binder.Type == typeof(KVToken))
                {
                    result = instance;
                    return true;
                }

                object value = instance.Value;

                if (value == null)
                {
                    result = null;
                    return ReflectionUtils.IsNullable(binder.Type);
                }

                result = ConvertUtils.Convert(value, CultureInfo.InvariantCulture, binder.Type);
                return true;
            }

            public override bool TryBinaryOperation(KVValue instance, BinaryOperationBinder binder, object arg, out object result)
            {
                KVValue value = arg as KVValue;
                object compareValue = value != null ? value.Value : arg;

                switch (binder.Operation)
                {
                    case ExpressionType.Equal:
                        result = (Compare(instance.Type, instance.Value, compareValue) == 0);
                        return true;
                    case ExpressionType.NotEqual:
                        result = (Compare(instance.Type, instance.Value, compareValue) != 0);
                        return true;
                    case ExpressionType.GreaterThan:
                        result = (Compare(instance.Type, instance.Value, compareValue) > 0);
                        return true;
                    case ExpressionType.GreaterThanOrEqual:
                        result = (Compare(instance.Type, instance.Value, compareValue) >= 0);
                        return true;
                    case ExpressionType.LessThan:
                        result = (Compare(instance.Type, instance.Value, compareValue) < 0);
                        return true;
                    case ExpressionType.LessThanOrEqual:
                        result = (Compare(instance.Type, instance.Value, compareValue) <= 0);
                        return true;
                    case ExpressionType.Add:
                    case ExpressionType.AddAssign:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractAssign:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyAssign:
                    case ExpressionType.Divide:
                    case ExpressionType.DivideAssign:
                        if (Operation(binder.Operation, instance.Value, compareValue, out result))
                        {
                            result = new KVValue(result);
                            return true;
                        }
                        break;
                }

                result = null;
                return false;
            }
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            KVValue value = obj as KVValue;
            object otherValue = value != null ? value.Value : obj;

            return Compare(_valueType, _value, otherValue);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings:
        /// Value
        /// Meaning
        /// Less than zero
        /// This instance is less than <paramref name="obj"/>.
        /// Zero
        /// This instance is equal to <paramref name="obj"/>.
        /// Greater than zero
        /// This instance is greater than <paramref name="obj"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// 	<paramref name="obj"/> is not of the same type as this instance.
        /// </exception>
        public int CompareTo(KVValue obj)
        {
            if (obj == null)
            {
                return 1;
            }

            return Compare(_valueType, _value, obj._value);
        }
    }
}