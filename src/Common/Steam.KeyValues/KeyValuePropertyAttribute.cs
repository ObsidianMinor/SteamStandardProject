using System;

namespace Steam.KeyValues
{
    /// <summary>
    /// Instructs the <see cref="KeyValueSerializer"/> to always serialize this property with the specified name
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
    public class KeyValuePropertyAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets whether this field is required during serialization
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets the property name of this member
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Initializes the attribute with the default values
        /// </summary>
        public KeyValuePropertyAttribute() { }

        /// <summary>
        /// Initializes the attribute with the default values and the specified property name
        /// </summary>
        /// <param name="propertyName"></param>
        public KeyValuePropertyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}
