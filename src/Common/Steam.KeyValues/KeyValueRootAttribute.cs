using System;

namespace Steam.KeyValues
{
    /// <summary>
    /// Instructs the <see cref="KeyValueSerializer"/> how to serialize this object as the root KeyValue
    /// </summary>
    public class KeyValueRootAttribute : Attribute
    {
        public string RootName { get; set; }
    }
}
