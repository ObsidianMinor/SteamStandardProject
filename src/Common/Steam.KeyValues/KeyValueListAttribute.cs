using System;

namespace Steam.KeyValues
{
    public class KeyValueListAttribute : Attribute
    {
        public string PropertyName { get; set; }

        public KeyValueListAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public KeyValueListAttribute()
        {

        }
    }
}
