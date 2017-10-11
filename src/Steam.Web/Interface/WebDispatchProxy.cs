using System;
using System.Reflection;

namespace Steam.Web.Interface
{
    public class WebDispatchProxy : DispatchProxy
    {
        private IWebInterfaceContractResolver _resolver;
        private WebInvoker _invoker;

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            WebInterfaceContract @interface = _resolver.ResolveInterface(targetMethod.DeclaringType);
            WebMethodContract method = _resolver.ResolveMethod(targetMethod);
            if (!method.CanInvoke)
                throw new MissingMethodException(targetMethod.DeclaringType.Name, targetMethod.Name);

            return _invoker.InvokeMethod(@interface, method, args);
        }

        internal static T Create<T>(IWebInterfaceContractResolver resolver, WebInvoker invoker)
        {
            T value = Create<T, WebDispatchProxy>();
            WebDispatchProxy proxy = value as WebDispatchProxy;
            proxy._invoker = invoker;
            proxy._resolver = resolver;
            return value;
        }
    }
}
