using System.Diagnostics;

namespace Steam.KeyValues
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal sealed class KeyValueResultsView
    {
        public KeyValueResultsView(ImmutableKeyValue value)
        {
            object[] items = new object[value.Length];
            int i = 0;
            foreach (var subKey in value)
            {
                items[i] = subKey.Type == 0 ? (object)new ValuesTypeProxy(subKey) : new ValueTypeProxy(subKey);
                i++;
            }

            Items = items;
        }

        public KeyValueResultsView(KeyValue value)
        {
            object[] items = new object[value.Length];
            int i = 0;
            foreach (var subKey in value)
            {
                items[i] = subKey.Type == 0 ? (object)new ValuesTypeProxy(subKey) : new ValueTypeProxy(subKey);
                i++;
            }

            Items = items;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object[] Items { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => "";

        [DebuggerDisplay("{_value}", Name = "{_key,nq}", Type = "{_type,nq}")]
        private class ValueTypeProxy
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly string _key;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly object _value;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly KeyValueType _type;

            public ValueTypeProxy(ImmutableKeyValue kv)
            {
                _key = kv.Key;
                _type = kv.Type;
                _value = kv.GetValue(true);
            }

            public ValueTypeProxy(KeyValue kv)
            {
                _key = kv.Key;
                _type = kv.Type;
                _value = kv.GetValue(true);
            }
        }

        [DebuggerDisplay("Length = {Items.Length}", Name = "{_key,nq}", Type = "None")]
        private class ValuesTypeProxy
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly string _key;

            public ValuesTypeProxy(ImmutableKeyValue kv)
            {
                _key = kv.Key;

                object[] items = new object[kv.Length];
                int i = 0;
                foreach (var subKey in kv)
                {
                    items[i] = subKey.Type == 0 ? (object)new ValuesTypeProxy(subKey) : new ValueTypeProxy(subKey);
                    i++;
                }

                Items = items;
            }

            public ValuesTypeProxy(KeyValue kv)
            {
                _key = kv.Key;

                object[] items = new object[kv.Length];
                int i = 0;
                foreach (var subKey in kv)
                {
                    items[i] = subKey.Type == 0 ? (object)new ValuesTypeProxy(subKey) : new ValueTypeProxy(subKey);
                    i++;
                }

                Items = items;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public object[] Items { get; }
        }
    }
}
