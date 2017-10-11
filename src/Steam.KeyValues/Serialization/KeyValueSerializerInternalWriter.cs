using Steam.KeyValues.Linq;
using Steam.KeyValues.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Steam.KeyValues.Serialization
{
    internal class KeyValueSerializerInternalWriter : KeyValueSerializerInternalBase
    {
        private Type _rootType;
        private int _rootLevel;
        private readonly List<object> _serializeStack = new List<object>();

        public KeyValueSerializerInternalWriter(KeyValueSerializer serializer)
            : base(serializer)
        {
        }

        public void Serialize(KeyValueWriter KeyValueWriter, object value, Type objectType)
        {
            throw new NotImplementedException();
        }
    }
}
