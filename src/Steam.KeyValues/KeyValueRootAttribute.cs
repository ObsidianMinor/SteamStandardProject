using System;

namespace Steam.KeyValues
{
    /// <summary>
    /// Instructs the <see cref="KeyValueSerializer"/> how to serialize this object as the root KeyValue
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false)]
    public sealed class KeyValueRootAttribute : KeyValueObjectAttribute
    {
        /// <summary>
        /// Gets or sets the root KeyValue name
        /// </summary>
        /// <value>The root name</value>
        public string RootName { get; set; }

        /// <summary>
        /// Gets or sets the base file
        /// </summary>
        /// <value>The base file</value>
        public string BaseFile { get; set; }
    }
}
