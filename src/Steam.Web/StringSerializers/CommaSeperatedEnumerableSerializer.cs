using System;
using System.Collections;
using System.Collections.Generic;

namespace Steam.Web.StringSerializers
{
    public class CommaSeperatedEnumerableSerializer : StringSerializer
    {
        public override bool CanConvert(Type t) => typeof(IEnumerable).IsAssignableFrom(t);

        public override string ToString(object value)
        {
            return string.Join(",", EnumerableToStrings(value as IEnumerable));
        }

        private IEnumerable<string> EnumerableToStrings(IEnumerable e)
        {
            foreach(object o in e)
                yield return o?.ToString() ?? "";
        }
    }
}