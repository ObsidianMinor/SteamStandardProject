using System;
using Steam.Rest;

namespace Steam.Web
{
    /// <summary>
    /// Defines a method's version, HttpMethod, and whether a key is required to use it
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class WebMethodAttribute : WebAttribute
    {
        public string Name { get; set; }

        private int _version = 1;
        public int Version
        {
            get => _version;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        public HttpMethod Method { get; set; } = HttpMethod.Get;

        public bool RequireKey { get; set; } = false;
    }
}
