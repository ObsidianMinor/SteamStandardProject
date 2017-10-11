﻿using Steam.KeyValues.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Steam.KeyValues.Utilities
{
    internal enum PrimitiveTypeCode
    {
        Empty = 0,
        Object = 1,
        Char = 2,
        CharNullable = 3,
        SByte = 6,
        SByteNullable = 7,
        Int16 = 8,
        Int16Nullable = 9,
        UInt16 = 10,
        UInt16Nullable = 11,
        Int32 = 12,
        Int32Nullable = 13,
        Byte = 14,
        ByteNullable = 15,
        UInt32 = 16,
        UInt32Nullable = 17,
        Int64 = 18,
        Int64Nullable = 19,
        UInt64 = 20,
        UInt64Nullable = 21,
        Single = 22,
        SingleNullable = 23,
        Double = 24,
        DoubleNullable = 25,
        Decimal = 30,
        DecimalNullable = 31,
        String = 39,
    }

    internal class TypeInformation
    {
        public Type Type { get; set; }
        public PrimitiveTypeCode TypeCode { get; set; }
    }

    internal enum ParseResult
    {
        None = 0,
        Success = 1,
        Overflow = 2,
        Invalid = 3
    }

    internal static class ConvertUtils
    {
        private static readonly Dictionary<Type, PrimitiveTypeCode> TypeCodeMap =
            new Dictionary<Type, PrimitiveTypeCode>
            {
                { typeof(char), PrimitiveTypeCode.Char },
                { typeof(char?), PrimitiveTypeCode.CharNullable },
                { typeof(sbyte), PrimitiveTypeCode.SByte },
                { typeof(sbyte?), PrimitiveTypeCode.SByteNullable },
                { typeof(short), PrimitiveTypeCode.Int16 },
                { typeof(short?), PrimitiveTypeCode.Int16Nullable },
                { typeof(ushort), PrimitiveTypeCode.UInt16 },
                { typeof(ushort?), PrimitiveTypeCode.UInt16Nullable },
                { typeof(int), PrimitiveTypeCode.Int32 },
                { typeof(int?), PrimitiveTypeCode.Int32Nullable },
                { typeof(byte), PrimitiveTypeCode.Byte },
                { typeof(byte?), PrimitiveTypeCode.ByteNullable },
                { typeof(uint), PrimitiveTypeCode.UInt32 },
                { typeof(uint?), PrimitiveTypeCode.UInt32Nullable },
                { typeof(long), PrimitiveTypeCode.Int64 },
                { typeof(long?), PrimitiveTypeCode.Int64Nullable },
                { typeof(ulong), PrimitiveTypeCode.UInt64 },
                { typeof(ulong?), PrimitiveTypeCode.UInt64Nullable },
                { typeof(float), PrimitiveTypeCode.Single },
                { typeof(float?), PrimitiveTypeCode.SingleNullable },
                { typeof(double), PrimitiveTypeCode.Double },
                { typeof(double?), PrimitiveTypeCode.DoubleNullable },
                { typeof(decimal), PrimitiveTypeCode.Decimal },
                { typeof(decimal?), PrimitiveTypeCode.DecimalNullable },
                { typeof(string), PrimitiveTypeCode.String },
            };

        public static PrimitiveTypeCode GetTypeCode(Type t)
        {
            return GetTypeCode(t, out bool isEnum);
        }

        public static PrimitiveTypeCode GetTypeCode(Type t, out bool isEnum)
        {
            if (TypeCodeMap.TryGetValue(t, out PrimitiveTypeCode typeCode))
            {
                isEnum = false;
                return typeCode;
            }

            if (t.IsEnum())
            {
                isEnum = true;
                return GetTypeCode(Enum.GetUnderlyingType(t));
            }

            // performance?
            if (ReflectionUtils.IsNullableType(t))
            {
                Type nonNullable = Nullable.GetUnderlyingType(t);
                if (nonNullable.IsEnum())
                {
                    Type nullableUnderlyingType = typeof(Nullable<>).MakeGenericType(Enum.GetUnderlyingType(nonNullable));
                    isEnum = true;
                    return GetTypeCode(nullableUnderlyingType);
                }
            }

            isEnum = false;
            return PrimitiveTypeCode.Object;
        }

        public static bool IsConvertible(Type t)
        {
            return (
                t == typeof(bool) || t == typeof(byte) || t == typeof(char) || t == typeof(DateTime) || t == typeof(decimal) || t == typeof(double) || t == typeof(short) || t == typeof(int) ||
                t == typeof(long) || t == typeof(sbyte) || t == typeof(float) || t == typeof(string) || t == typeof(ushort) || t == typeof(uint) || t == typeof(ulong) || t.IsEnum());
        }

        public static TimeSpan ParseTimeSpan(string input)
        {
            return TimeSpan.Parse(input, CultureInfo.InvariantCulture);
        }


        internal struct TypeConvertKey : IEquatable<TypeConvertKey>
        {
            private readonly Type _initialType;
            private readonly Type _targetType;

            public Type InitialType
            {
                get { return _initialType; }
            }

            public Type TargetType
            {
                get { return _targetType; }
            }

            public TypeConvertKey(Type initialType, Type targetType)
            {
                _initialType = initialType;
                _targetType = targetType;
            }

            public override int GetHashCode()
            {
                return _initialType.GetHashCode() ^ _targetType.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (!(obj is TypeConvertKey))
                {
                    return false;
                }

                return Equals((TypeConvertKey)obj);
            }

            public bool Equals(TypeConvertKey other)
            {
                return (_initialType == other._initialType && _targetType == other._targetType);
            }
        }

        private static readonly ThreadSafeStore<TypeConvertKey, Func<object, object>> CastConverters =
            new ThreadSafeStore<TypeConvertKey, Func<object, object>>(CreateCastConverter);

        private static Func<object, object> CreateCastConverter(TypeConvertKey t)
        {
            MethodInfo castMethodInfo = t.TargetType.GetMethod("op_Implicit", new[] { t.InitialType });
            if (castMethodInfo == null)
            {
                castMethodInfo = t.TargetType.GetMethod("op_Explicit", new[] { t.InitialType });
            }

            if (castMethodInfo == null)
            {
                return null;
            }

            MethodCall<object, object> call = KeyValueTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(castMethodInfo);

            return o => call(null, o);
        }

        #region TryConvert
        internal enum ConvertResult
        {
            Success = 0,
            CannotConvertNull = 1,
            NotInstantiableType = 2,
            NoValidConversion = 3
        }

        public static object Convert(object initialValue, CultureInfo culture, Type targetType)
        {
            switch (TryConvertInternal(initialValue, culture, targetType, out object value))
            {
                case ConvertResult.Success:
                    return value;
                case ConvertResult.CannotConvertNull:
                    throw new Exception("Can not convert null {0} into non-nullable {1}.".FormatWith(CultureInfo.InvariantCulture, initialValue.GetType(), targetType));
                case ConvertResult.NotInstantiableType:
                    throw new ArgumentException("Target type {0} is not a value type or a non-abstract class.".FormatWith(CultureInfo.InvariantCulture, targetType), nameof(targetType));
                case ConvertResult.NoValidConversion:
                    throw new InvalidOperationException("Can not convert from {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, initialValue.GetType(), targetType));
                default:
                    throw new InvalidOperationException("Unexpected conversion result.");
            }
        }

        private static bool TryConvert(object initialValue, CultureInfo culture, Type targetType, out object value)
        {
            try
            {
                if (TryConvertInternal(initialValue, culture, targetType, out value) == ConvertResult.Success)
                {
                    return true;
                }

                value = null;
                return false;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        private static ConvertResult TryConvertInternal(object initialValue, CultureInfo culture, Type targetType, out object value)
        {
            if (initialValue == null)
            {
                throw new ArgumentNullException(nameof(initialValue));
            }

            if (ReflectionUtils.IsNullableType(targetType))
            {
                targetType = Nullable.GetUnderlyingType(targetType);
            }

            Type initialType = initialValue.GetType();

            if (targetType == initialType)
            {
                value = initialValue;
                return ConvertResult.Success;
            }

            // use Convert.ChangeType if both types are IConvertible
            if (ConvertUtils.IsConvertible(initialValue.GetType()) && ConvertUtils.IsConvertible(targetType))
            {
                if (targetType.IsEnum())
                {
                    if (initialValue is string)
                    {
                        value = Enum.Parse(targetType, initialValue.ToString(), true);
                        return ConvertResult.Success;
                    }
                    else if (IsInteger(initialValue))
                    {
                        value = Enum.ToObject(targetType, initialValue);
                        return ConvertResult.Success;
                    }
                }

                value = System.Convert.ChangeType(initialValue, targetType, culture);
                return ConvertResult.Success;
            }
            
            if (initialValue is DateTime && targetType == typeof(DateTimeOffset))
            {
                value = new DateTimeOffset((DateTime)initialValue);
                return ConvertResult.Success;
            }

            if (initialValue is byte[] bytes && targetType == typeof(Guid))
            {
                value = new Guid(bytes);
                return ConvertResult.Success;
            }

            if (initialValue is Guid && targetType == typeof(byte[]))
            {
                value = ((Guid)initialValue).ToByteArray();
                return ConvertResult.Success;
            }

            if (initialValue is string s)
            {
                if (targetType == typeof(Guid))
                {
                    value = new Guid(s);
                    return ConvertResult.Success;
                }
                if (targetType == typeof(Uri))
                {
                    value = new Uri(s, UriKind.RelativeOrAbsolute);
                    return ConvertResult.Success;
                }
                if (targetType == typeof(TimeSpan))
                {
                    value = ParseTimeSpan(s);
                    return ConvertResult.Success;
                }
                if (targetType == typeof(byte[]))
                {
                    value = System.Convert.FromBase64String(s);
                    return ConvertResult.Success;
                }
                if (targetType == typeof(Version))
                {
                    if (VersionTryParse(s, out Version result))
                    {
                        value = result;
                        return ConvertResult.Success;
                    }
                    value = null;
                    return ConvertResult.NoValidConversion;
                }
                if (typeof(Type).IsAssignableFrom(targetType))
                {
                    value = Type.GetType(s, true);
                    return ConvertResult.Success;
                }
            }

            // see if source or target types have a TypeConverter that converts between the two
            TypeConverter toConverter = TypeDescriptor.GetConverter(initialType);

            if (toConverter != null && toConverter.CanConvertTo(targetType))
            {
                value = toConverter.ConvertTo(null, culture, initialValue, targetType);
                return ConvertResult.Success;
            }

            TypeConverter fromConverter = TypeDescriptor.GetConverter(targetType);

            if (fromConverter != null && fromConverter.CanConvertFrom(initialType))
            {
                value = fromConverter.ConvertFrom(null, culture, initialValue);
                return ConvertResult.Success;
            }

            if (targetType.IsInterface() || targetType.IsGenericTypeDefinition() || targetType.IsAbstract())
            {
                value = null;
                return ConvertResult.NotInstantiableType;
            }

            value = null;
            return ConvertResult.NoValidConversion;
        }
        #endregion

        #region ConvertOrCast
        /// <summary>
        /// Converts the value to the specified type. If the value is unable to be converted, the
        /// value is checked whether it assignable to the specified type.
        /// </summary>
        /// <param name="initialValue">The value to convert.</param>
        /// <param name="culture">The culture to use when converting.</param>
        /// <param name="targetType">The type to convert or cast the value to.</param>
        /// <returns>
        /// The converted type. If conversion was unsuccessful, the initial value
        /// is returned if assignable to the target type.
        /// </returns>
        public static object ConvertOrCast(object initialValue, CultureInfo culture, Type targetType)
        {

            if (targetType == typeof(object))
            {
                return initialValue;
            }

            if (initialValue == null && ReflectionUtils.IsNullable(targetType))
            {
                return null;
            }

            if (TryConvert(initialValue, culture, targetType, out object convertedValue))
            {
                return convertedValue;
            }

            return EnsureTypeAssignable(initialValue, ReflectionUtils.GetObjectType(initialValue), targetType);
        }
        #endregion

        private static object EnsureTypeAssignable(object value, Type initialType, Type targetType)
        {
            Type valueType = value?.GetType();

            if (value != null)
            {
                if (targetType.IsAssignableFrom(valueType))
                {
                    return value;
                }

                Func<object, object> castConverter = CastConverters.Get(new TypeConvertKey(valueType, targetType));
                if (castConverter != null)
                {
                    return castConverter(value);
                }
            }
            else
            {
                if (ReflectionUtils.IsNullable(targetType))
                {
                    return null;
                }
            }

            throw new ArgumentException("Could not cast or convert from {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, initialType?.ToString() ?? "{null}", targetType));
        }

        public static bool VersionTryParse(string input, out Version result)
        {
            return Version.TryParse(input, out result);
        }

        public static bool IsInteger(object value)
        {
            switch (GetTypeCode(value.GetType()))
            {
                case PrimitiveTypeCode.SByte:
                case PrimitiveTypeCode.Byte:
                case PrimitiveTypeCode.Int16:
                case PrimitiveTypeCode.UInt16:
                case PrimitiveTypeCode.Int32:
                case PrimitiveTypeCode.UInt32:
                case PrimitiveTypeCode.Int64:
                case PrimitiveTypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        public static ParseResult Int32TryParse(char[] chars, int start, int length, out int value)
        {
            value = 0;

            if (length == 0)
            {
                return ParseResult.Invalid;
            }

            bool isNegative = (chars[start] == '-');

            if (isNegative)
            {
                // text just a negative sign
                if (length == 1)
                {
                    return ParseResult.Invalid;
                }

                start++;
                length--;
            }

            int end = start + length;

            // Int32.MaxValue and MinValue are 10 chars
            // Or is 10 chars and start is greater than two
            // Need to improve this!
            if (length > 10 || (length == 10 && chars[start] - '0' > 2))
            {
                // invalid result takes precedence over overflow
                for (int i = start; i < end; i++)
                {
                    int c = chars[i] - '0';

                    if (c < 0 || c > 9)
                    {
                        return ParseResult.Invalid;
                    }
                }

                return ParseResult.Overflow;
            }

            for (int i = start; i < end; i++)
            {
                int c = chars[i] - '0';

                if (c < 0 || c > 9)
                {
                    return ParseResult.Invalid;
                }

                int newValue = (10 * value) - c;

                // overflow has caused the number to loop around
                if (newValue > value)
                {
                    i++;

                    // double check the rest of the string that there wasn't anything invalid
                    // invalid result takes precedence over overflow result
                    for (; i < end; i++)
                    {
                        c = chars[i] - '0';

                        if (c < 0 || c > 9)
                        {
                            return ParseResult.Invalid;
                        }
                    }

                    return ParseResult.Overflow;
                }

                value = newValue;
            }

            // go from negative to positive to avoids overflow
            // negative can be slightly bigger than positive
            if (!isNegative)
            {
                // negative integer can be one bigger than positive
                if (value == int.MinValue)
                {
                    return ParseResult.Overflow;
                }

                value = -value;
            }

            return ParseResult.Success;
        }

        public static ParseResult Int64TryParse(char[] chars, int start, int length, out long value)
        {
            value = 0;

            if (length == 0)
            {
                return ParseResult.Invalid;
            }

            bool isNegative = (chars[start] == '-');

            if (isNegative)
            {
                // text just a negative sign
                if (length == 1)
                {
                    return ParseResult.Invalid;
                }

                start++;
                length--;
            }

            int end = start + length;

            // Int64.MaxValue and MinValue are 19 chars
            if (length > 19)
            {
                // invalid result takes precedence over overflow
                for (int i = start; i < end; i++)
                {
                    int c = chars[i] - '0';

                    if (c < 0 || c > 9)
                    {
                        return ParseResult.Invalid;
                    }
                }

                return ParseResult.Overflow;
            }

            for (int i = start; i < end; i++)
            {
                int c = chars[i] - '0';

                if (c < 0 || c > 9)
                {
                    return ParseResult.Invalid;
                }

                long newValue = (10 * value) - c;

                // overflow has caused the number to loop around
                if (newValue > value)
                {
                    i++;

                    // double check the rest of the string that there wasn't anything invalid
                    // invalid result takes precedence over overflow result
                    for (; i < end; i++)
                    {
                        c = chars[i] - '0';

                        if (c < 0 || c > 9)
                        {
                            return ParseResult.Invalid;
                        }
                    }

                    return ParseResult.Overflow;
                }

                value = newValue;
            }

            // go from negative to positive to avoids overflow
            // negative can be slightly bigger than positive
            if (!isNegative)
            {
                // negative integer can be one bigger than positive
                if (value == long.MinValue)
                {
                    return ParseResult.Overflow;
                }

                value = -value;
            }

            return ParseResult.Success;
        }

        public static ParseResult DecimalTryParse(char[] chars, int start, int length, out decimal value)
        {
            value = 0M;
            const decimal decimalMaxValueHi28 = 7922816251426433759354395033M;
            const ulong decimalMaxValueHi19 = 7922816251426433759UL;
            const ulong decimalMaxValueLo9 = 354395033UL;
            const char decimalMaxValueLo1 = '5';

            if (length == 0)
            {
                return ParseResult.Invalid;
            }

            bool isNegative = (chars[start] == '-');
            if (isNegative)
            {
                // text just a negative sign
                if (length == 1)
                {
                    return ParseResult.Invalid;
                }

                start++;
                length--;
            }

            int i = start;
            int end = start + length;
            int numDecimalStart = end;
            int numDecimalEnd = end;
            int exponent = 0;
            ulong hi19 = 0UL;
            ulong lo10 = 0UL;
            int mantissaDigits = 0;
            int exponentFromMantissa = 0;
            bool? roundUp = null;
            bool? storeOnly28Digits = null;
            for (; i < end; i++)
            {
                char c = chars[i];
                switch (c)
                {
                    case '.':
                        if (i == start)
                        {
                            return ParseResult.Invalid;
                        }
                        if (i + 1 == end)
                        {
                            return ParseResult.Invalid;
                        }

                        if (numDecimalStart != end)
                        {
                            // multiple decimal points
                            return ParseResult.Invalid;
                        }

                        numDecimalStart = i + 1;
                        break;
                    case 'e':
                    case 'E':
                        if (i == start)
                        {
                            return ParseResult.Invalid;
                        }
                        if (i == numDecimalStart)
                        {
                            // E follows decimal point		
                            return ParseResult.Invalid;
                        }
                        i++;
                        if (i == end)
                        {
                            return ParseResult.Invalid;
                        }

                        if (numDecimalStart < end)
                        {
                            numDecimalEnd = i - 1;
                        }

                        c = chars[i];
                        bool exponentNegative = false;
                        switch (c)
                        {
                            case '-':
                                exponentNegative = true;
                                i++;
                                break;
                            case '+':
                                i++;
                                break;
                        }

                        // parse 3 digit 
                        for (; i < end; i++)
                        {
                            c = chars[i];
                            if (c < '0' || c > '9')
                            {
                                return ParseResult.Invalid;
                            }

                            int newExponent = (10 * exponent) + (c - '0');
                            // stops updating exponent when overflowing
                            if (exponent < newExponent)
                            {
                                exponent = newExponent;
                            }
                        }

                        if (exponentNegative)
                        {
                            exponent = -exponent;
                        }
                        break;
                    default:
                        if (c < '0' || c > '9')
                        {
                            return ParseResult.Invalid;
                        }

                        if (i == start && c == '0')
                        {
                            i++;
                            if (i != end)
                            {
                                c = chars[i];
                                if (c == '.')
                                {
                                    goto case '.';
                                }
                                if (c == 'e' || c == 'E')
                                {
                                    goto case 'E';
                                }

                                return ParseResult.Invalid;
                            }
                        }

                        if (mantissaDigits < 29 && (mantissaDigits != 28 || !(storeOnly28Digits ?? (storeOnly28Digits = (hi19 > decimalMaxValueHi19 || (hi19 == decimalMaxValueHi19 && (lo10 > decimalMaxValueLo9 || (lo10 == decimalMaxValueLo9 && c > decimalMaxValueLo1))))).GetValueOrDefault())))
                        {
                            if (mantissaDigits < 19)
                            {
                                hi19 = (hi19 * 10UL) + (ulong)(c - '0');
                            }
                            else
                            {
                                lo10 = (lo10 * 10UL) + (ulong)(c - '0');
                            }
                            ++mantissaDigits;
                        }
                        else
                        {
                            if (!roundUp.HasValue)
                            {
                                roundUp = c >= '5';
                            }
                            ++exponentFromMantissa;
                        }
                        break;
                }
            }

            exponent += exponentFromMantissa;

            // correct the decimal point
            exponent -= (numDecimalEnd - numDecimalStart);

            if (mantissaDigits <= 19)
            {
                value = hi19;
            }
            else
            {
                value = (hi19 * DecimalFactors[mantissaDigits - 20]) + lo10;
            }

            if (exponent > 0)
            {
                mantissaDigits += exponent;
                if (mantissaDigits > 29)
                {
                    return ParseResult.Overflow;
                }
                if (mantissaDigits == 29)
                {
                    if (exponent > 1)
                    {
                        value *= DecimalFactors[exponent - 2];
                        if (value > decimalMaxValueHi28)
                        {
                            return ParseResult.Overflow;
                        }
                    }
                    value *= 10M;
                }
                else
                {
                    value *= DecimalFactors[exponent - 1];
                }
            }
            else
            {
                if (roundUp == true && exponent >= -28)
                {
                    ++value;
                }
                if (exponent < 0)
                {
                    if (mantissaDigits + exponent + 28 <= 0)
                    {
                        value = 0M;
                        return ParseResult.Success;
                    }
                    if (exponent >= -28)
                    {
                        value /= DecimalFactors[-exponent - 1];
                    }
                    else
                    {
                        decimal[] decimalFactors = DecimalFactors;
                        value /= decimalFactors[27];
                        value /= decimalFactors[-exponent - 29];
                    }
                }
            }

            if (isNegative)
            {
                value = -value;
            }

            return ParseResult.Success;
        }

        private static decimal[] _decimalFactors;

        private static decimal[] DecimalFactors
        {
            get
            {
                decimal[] decimalFactors = _decimalFactors;
                if (decimalFactors == null)
                {
                    decimalFactors = new decimal[28];
                    decimal last = 1M;
                    for (int i = 0; i < decimalFactors.Length; ++i)
                    {
                        decimalFactors[i] = last *= 10M;
                    }
                    _decimalFactors = decimalFactors;
                }
                return decimalFactors;
            }
        }

        public static bool TryConvertGuid(string s, out Guid g)
        {
            return Guid.TryParseExact(s, "D", out g);
        }

        public static bool TryHexTextToInt(char[] text, int start, int end, out int value)
        {
            value = 0;

            for (int i = start; i < end; i++)
            {
                char ch = text[i];
                int chValue;

                if (ch <= 57 && ch >= 48)
                {
                    chValue = ch - 48;
                }
                else if (ch <= 70 && ch >= 65)
                {
                    chValue = ch - 55;
                }
                else if (ch <= 102 && ch >= 97)
                {
                    chValue = ch - 87;
                }
                else
                {
                    value = 0;
                    return false;
                }

                value += chValue << ((end - 1 - i) * 4);
            }

            return true;
        }
    }
}