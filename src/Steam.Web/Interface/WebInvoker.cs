using Steam.Rest;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Steam.Web.Interface
{
    public class WebInvoker : RestEntity
    {
        static readonly ConcurrentDictionary<Type, Func<Task<RestResponse>, ResponseConverter, object>> _continuationFunctions = new ConcurrentDictionary<Type, Func<Task<RestResponse>, ResponseConverter, object>>();
        static readonly MethodInfo ContinuationMethodInfo = typeof(WebInvoker).GetMethod(nameof(ContinuationFunction), BindingFlags.Static | BindingFlags.NonPublic);

        public WebInvoker(SteamWebClient client) : base(client)
        {
        }

        public virtual object InvokeMethod(WebInterfaceContract @interface, WebMethodContract method, object[] parameters)
        {
            Type returnType = method.Return.ParameterInfo.ParameterType;
            if (returnType != typeof(void) && returnType != typeof(Task) && returnType.GetGenericTypeDefinition() != typeof(Task<>))
                throw new InvalidOperationException("Cannot invoke method without a return type of void, Task, or Task<T>");

            var interfaceName = @interface.Name;
            var methodName = method.Name;
            var httpMethod = method.Method;
            var query = new KeyValuePair<string, string>[method.HasOptions ? parameters.Length - 1 : parameters.Length];
            var options = method.HasOptions ? parameters.Last() as RequestOptions : null;

            for(int i = 0; i < query.Length; i++)
            {
                object param = parameters[i];
                WebParameterContract contract = method.Parameters[i];

                if (param == null && contract.Optional)
                    continue;
                string value = null;
                if (contract.Serializer.CanConvert(param.GetType()))
                {
                    value = contract.Serializer.ToString(param);
                    if (value == null && contract.Optional)
                        continue;
                    else
                        value = value ?? StringSerializer.Instance.ToString(param);
                }
                else
                    value = StringSerializer.Instance.ToString(param);

                query[i] = new KeyValuePair<string, string>(contract.Name, value);
            }
            
            var send = SendAsync(httpMethod, interfaceName, methodName, method.Version, method.RequiresKey, options, query.Select(q => (q.Key, q.Value)).ToArray());

            if (returnType == typeof(void))
            {
                send.RunSynchronously();
                if (send.IsFaulted)
                    throw send.Exception;
                else if (send.IsCanceled)
                    throw new OperationCanceledException();

                return null;
            }
            else if (returnType == typeof(Task) || (returnType == typeof(Task<RestResponse>)))
            {
                return send; // so what if they cast back, not my problem
            }
            else if (returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                return GetCachedContinuationFunction(returnType.GetGenericArguments().First())(send, method.Return.Converter);
            }
            else
                throw new InvalidOperationException("Cannot invoke method without a return type of void, Task, or Task<T>");
        }

        protected Func<Task<RestResponse>, ResponseConverter, object> GetCachedContinuationFunction(Type returnType)
        {
            return _continuationFunctions.GetOrAdd(returnType, (t) =>
            {
                var genericMethod = ContinuationMethodInfo.MakeGenericMethod(t);
                var delegateMethod = genericMethod.CreateDelegate(typeof(Func<Task<RestResponse>, ResponseConverter, object>));
                return delegateMethod as Func<Task<RestResponse>, ResponseConverter, object>;
            });
        }

        protected internal Task<RestResponse> SendAsync(HttpMethod httpMethod, string interfaceName, string method, int version, bool requireKey, RequestOptions options = null, params (string, string)[] parameters)
            => (Client as SteamWebClient).SendAsync(httpMethod, interfaceName, method, version, requireKey, options, parameters);

        private static object ContinuationFunction<TResult>(Task<RestResponse> response, ResponseConverter converter)
        {
            return response.ContinueWith((t) => converter.ReadResponse<TResult>(t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}
