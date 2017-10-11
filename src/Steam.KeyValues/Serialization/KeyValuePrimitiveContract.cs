using Steam.KeyValues.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Steam.KeyValues.Serialization
{
    /// <summary>
    /// Contract details for a <see cref="Type"/> used by the <see cref="KeyValueSerializer"/>.
    /// </summary>
    public class KeyValuePrimitiveContract : KeyValueContract
    {
        internal PrimitiveTypeCode TypeCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValuePrimitiveContract"/> class.
        /// </summary>
        /// <param name="underlyingType">The underlying type for the contract.</param>
        public KeyValuePrimitiveContract(Type underlyingType)
            : base(underlyingType)
        {
            ContractType = KeyValueContractType.Primitive;

            TypeCode = ConvertUtils.GetTypeCode(underlyingType);
            IsReadOnlyOrFixedSize = true;

            if (ReadTypeMap.TryGetValue(NonNullableUnderlyingType, out ReadType readType))
            {
                InternalReadType = readType;
            }
        }

        private static readonly Dictionary<Type, ReadType> ReadTypeMap = new Dictionary<Type, ReadType>
        {
            [typeof(byte)] = ReadType.ReadAsInt32,
            [typeof(short)] = ReadType.ReadAsInt32,
            [typeof(int)] = ReadType.ReadAsInt32,
            [typeof(uint)] = ReadType.ReadAsInt64,
            [typeof(IntPtr)] = ReadType.ReadAsPointer,
            [typeof(UIntPtr)] = ReadType.ReadAsPointer,
            [typeof(Color)] = ReadType.ReadAsColor,
            [typeof(long)] = ReadType.ReadAsInt64,
            [typeof(ulong)] = ReadType.ReadAsUInt64,
            [typeof(decimal)] = ReadType.ReadAsDecimal,
            [typeof(string)] = ReadType.ReadAsString,
            [typeof(float)] = ReadType.ReadAsFloat,
            [typeof(double)] = ReadType.ReadAsDouble
        };
    }
}
