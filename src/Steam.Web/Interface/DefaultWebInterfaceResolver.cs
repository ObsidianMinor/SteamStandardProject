using Steam.Rest;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Steam.Web.Interface
{
    public class DefaultWebInterfaceResolver : IWebInterfaceContractResolver
    {
        private static ConcurrentDictionary<MemberInfo, WebContract> _contracts = new ConcurrentDictionary<MemberInfo, WebContract>();

        private static DefaultWebInterfaceResolver _instance;
        public static IWebInterfaceContractResolver Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DefaultWebInterfaceResolver();

                return _instance;
            }
        }
        
        public virtual WebInterfaceContract ResolveInterface(Type type)
        {
            if (_contracts.ContainsKey(type))
                return _contracts[type] as WebInterfaceContract;

            if (!type.IsInterface)
                throw new ArgumentException($"The provided type {type} is not an interface");

            WebInterfaceContract contract = new WebInterfaceContract(type);
            WebInterfaceAttribute attribute = type.GetCustomAttribute<WebInterfaceAttribute>();
            if (attribute == null || string.IsNullOrWhiteSpace(attribute.Name))
                contract.Name = type.Name;
            else
                contract.Name = attribute.Name;

            contract.IsService = attribute?.IsService ?? false;

            _contracts[type] = contract;

            return contract;
        }

        public virtual WebMethodContract ResolveMethod(MethodInfo method)
        {
            if (_contracts.ContainsKey(method))
                return _contracts[method] as WebMethodContract;

            WebMethodAttribute attribute = method.GetCustomAttribute<WebMethodAttribute>() ?? new WebMethodAttribute();
            ParameterInfo[] parameters = method.GetParameters();
            WebMethodContract contract = new WebMethodContract(method)
            {
                Method = attribute.Method,
                Name = string.IsNullOrWhiteSpace(attribute.Name) ? method.Name : attribute.Name,
                Version = attribute.Version,
                Return = ResolveReturn(method.ReturnParameter),
                Parameters = method.GetParameters().Select(p => ResolveParameter(p)).ToArray(),
                RequiresKey = attribute.RequireKey,
                HasOptions = parameters.LastOrDefault()?.ParameterType == typeof(RequestOptions),
                CanInvoke = method.ReturnType == typeof(void) 
                    || method.ReturnType == typeof(Task) 
                    || method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)
            };
            
            _contracts[method] = contract;
            return contract;
        }

        protected virtual WebParameterContract ResolveParameter(ParameterInfo parameter)
        {
            WebParameterContract contract = new WebParameterContract(parameter);
            WebParameterAttribute attribute = parameter.GetCustomAttribute<WebParameterAttribute>();
            contract.Name = string.IsNullOrWhiteSpace(attribute?.Name) ? parameter.Name : attribute.Name;

            contract.Optional = 
                attribute?.Optional ?? 
                parameter.ParameterType.IsValueType || Nullable.GetUnderlyingType(parameter.ParameterType) != null;

            contract.Serializer = attribute?.SerializerType == null 
                ? StringSerializer.Instance 
                : WebContractReflector.GetParameterSerializer(attribute.SerializerType, attribute.SerializerArgs);

            return contract;
        }

        protected virtual WebReturnContract ResolveReturn(ParameterInfo returnParameter)
        {
            WebReturnContract contract = new WebReturnContract(returnParameter);
            WebReturnAttribute attribute = returnParameter.GetCustomAttribute<WebReturnAttribute>();

            contract.Converter = attribute?.ResponseConverterType == null 
                ? ResponseConverter.Instance 
                : WebContractReflector.GetResponseConverter(attribute.ResponseConverterType, attribute.ResponseConverterParameters);

            return contract;
        }
    }
}