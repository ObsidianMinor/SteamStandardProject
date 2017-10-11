using System;

namespace Steam.Web
{
    /// <summary>
    /// Defines a Steam interface
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class WebInterfaceAttribute : WebAttribute
    {
        /// <summary> Gets the name of this interface </summary>
        public string Name { get; set; }

        /// <summary> Gets whether this interface is a service </summary>
        public bool IsService { get; set; }
    }
}
