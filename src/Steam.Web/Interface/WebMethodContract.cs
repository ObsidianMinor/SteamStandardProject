using Steam.Rest;
using System.Reflection;

namespace Steam.Web.Interface
{
    public class WebMethodContract : WebContract
    {
        public WebMethodContract(MethodInfo method) : base(WebContractType.Method)
        {
            UnderlyingMethod = method;
        }

        public MethodInfo UnderlyingMethod { get; }

        public bool CanInvoke { get; set; }

        public string Name { get; set; }

        public HttpMethod Method { get; set; }

        public int Version { get; set; }
        
        public WebParameterContract[] Parameters { get; set; }

        public WebReturnContract Return { get; set; }

        public bool RequiresKey { get; set; }

        /// <summary>
        /// Gets whether the specified method has request options
        /// </summary>
        public bool HasOptions { get; set; }
    }
}