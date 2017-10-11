using System;

namespace Steam.Web
{
    /// <summary>
    /// Controls how the returned response is converted to the specified type
    /// </summary>
    [AttributeUsage(AttributeTargets.ReturnValue, AllowMultiple = false)]
    public class WebReturnAttribute : Attribute
    {
        public object[] ResponseConverterParameters { get; set; }

        public Type ResponseConverterType { get; set; }
    }
}
