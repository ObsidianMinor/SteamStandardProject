using System;
using System.IO;

namespace Steam.Net.Utilities
{
    internal static class BinaryStreamUtils
    {
        internal static byte[] ReadToEnd(this BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            long count = reader.BaseStream.Length - reader.BaseStream.Position;
            if (count > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(count), "The resulting stream would be beyond the max value of Int32");

            return reader.ReadBytes((int)count);
        }

        internal static void Write(this BinaryWriter writer, object value)
        {
            switch(value)
            {
                case double doubleValue:
                    writer.Write(doubleValue);
                    break;
                case ulong ulongValue:
                    writer.Write(ulongValue);
                    break;
                case uint uintValue:
                    writer.Write(uintValue);
                    break;
                case ushort ushortValue:
                    writer.Write(ushortValue);
                    break;
                case string stringValue:
                    writer.Write(stringValue);
                    break;
                case float floatValue:
                    writer.Write(floatValue);
                    break;
                case sbyte sbyteValue:
                    writer.Write(sbyteValue);
                    break;
                case long longValue:
                    writer.Write(longValue);
                    break;
                case int intValue:
                    writer.Write(intValue);
                    break;
                case char charValue:
                    writer.Write(charValue);
                    break;
                case decimal decimalValue:
                    writer.Write(decimalValue);
                    break;
                case short shortValue:
                    writer.Write(shortValue);
                    break;
                case byte byteValue:
                    writer.Write(byteValue);
                    break;
                case byte[] byteArrayValue:
                    writer.Write(byteArrayValue);
                    break;
                case Enum enumValue:
                    Type underlyingType = Enum.GetUnderlyingType(value.GetType());
                    if (underlyingType == typeof(byte))
                        writer.Write((byte)value);
                    else if (underlyingType == typeof(sbyte))
                        writer.Write((sbyte)value);
                    else if (underlyingType == typeof(short))
                        writer.Write((short)value);
                    else if (underlyingType == typeof(ushort))
                        writer.Write((ushort)value);
                    else if (underlyingType == typeof(int))
                        writer.Write((int)value);
                    else if (underlyingType == typeof(uint))
                        writer.Write((uint)value);
                    else if (underlyingType == typeof(long))
                        writer.Write((long)value);
                    else
                        writer.Write((ulong)value);
                    break;
                default:
                    throw new InvalidCastException(); // it doesn't implement all the write types but fuck it
            }
        }
    }
}
