using System.Reflection;

namespace Steam.Web.Interface
{
    public class WebParameterContract : WebContract
    {
        public WebParameterContract(ParameterInfo parameter) : base(WebContractType.Parameter)
        {
            UnderlyingParameter = parameter;
        }

        public ParameterInfo UnderlyingParameter { get; }

        public string Name { get; set; }

        public bool Optional { get; set; }

        public StringSerializer Serializer { get; set; }
    }
}