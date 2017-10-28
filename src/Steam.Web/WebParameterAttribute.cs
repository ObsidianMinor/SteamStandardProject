using System;

namespace Steam.Web
{
    /// <summary>
    /// Defines whether a parameter is optional and the type of the string converter
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class WebParameterAttribute : WebAttribute
    {
        /// <summary>
        /// Sets the name of this parameter
        /// </summary>
        public string Name { get; set; }

        /// <summary> 
        /// Sets whether a null value removes this parameter from the query 
        /// </summary>
        public bool Optional { get; set; }

        /// <summary> 
        /// Sets the type of the <see cref="StringSerializer"/> for this parameter 
        /// </summary>
        public Type SerializerType { get; set; }

        public object[] SerializerArgs { get; set; }
    }
}
