using Steam.KeyValues.Linq;
using Steam.KeyValues.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;

namespace Steam.KeyValues.Serialization
{
    internal class KeyValueSerializerInternalReader : KeyValueSerializerInternalBase
    {
        internal enum PropertyPresence
        {
            None = 0,
            Null = 1,
            Value = 2
        }

        public KeyValueSerializerInternalReader(KeyValueSerializer serializer)
            : base(serializer)
        {
        }

        public void Populate(KeyValueReader reader, object target)
        {
            throw new NotImplementedException();
        }
        
        public object Deserialize(KeyValueReader reader, Type objectType, bool checkAdditionalContent)
        {
            throw new NotImplementedException();
        }

        internal string GetExpectedDescription(KeyValueContract contract)
        {
            switch (contract.ContractType)
            {
                case KeyValueContractType.Object:
                case KeyValueContractType.Dictionary:
                case KeyValueContractType.Serializable:
                case KeyValueContractType.Dynamic:
                    return @"KeyValue object (e.g. ""name""{""name"" ""value""})";
                case KeyValueContractType.Array:
                    return @"KeyValue array (e.g. ""name"" { ""1"" ""value"" ""2"" ""value"" ""3"" ""value"" })";
                case KeyValueContractType.Primitive:
                    return @"KeyValue primitive value (e.g. string, number)";
                case KeyValueContractType.String:
                    return @"KeyValue string value";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object CreateNewObject(KeyValueReader reader, KeyValueObjectContract objectContract, KeyValueProperty containerMember, KeyValueProperty containerProperty, out bool createdFromNonDefaultCreator)
        {
            throw new NotImplementedException();
        }
    }
}
