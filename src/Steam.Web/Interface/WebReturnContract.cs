using System.Reflection;

namespace Steam.Web.Interface
{
    public class WebReturnContract : WebContract
    {
        private ResponseConverter _converter;

        public WebReturnContract(ParameterInfo returnInfo) : base(WebContractType.Return)
        {
            ParameterInfo = returnInfo;
        }

        public ParameterInfo ParameterInfo { get; }

        public ResponseConverter Converter
        {
            get => _converter ?? ResponseConverter.Instance;
            set => _converter = value;
        }
    }
}
