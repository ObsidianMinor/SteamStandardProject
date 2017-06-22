using System;

namespace Steam.KeyValues
{
    public class KeyValueDictionaryAttribute : Attribute
    {
        public string PropertyName { get; set; }

        public KeyValueDictionaryAttribute()
        {

        }

        public KeyValueDictionaryAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}
