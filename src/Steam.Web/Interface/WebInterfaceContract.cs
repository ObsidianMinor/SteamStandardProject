using System;

namespace Steam.Web.Interface
{
    public class WebInterfaceContract : WebContract
    {
        public Type UnderlyingType { get; }

        public string Name { get; set; }

        public bool IsService { get; set; }

        public WebInterfaceContract(Type contractedType) : base(WebContractType.Interface)
        {
            UnderlyingType = contractedType;
        }
    }
}
