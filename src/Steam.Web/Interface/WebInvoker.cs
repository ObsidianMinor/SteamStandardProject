using Steam.Rest;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Steam.Web.Interface
{
    public class WebInvoker : WebEntity<SteamWebClient>
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
            var query = new (string, string)[method.HasOptions ? parameters.Length - 1 : parameters.Length];
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

                query[i] = (contract.Name, value);
            }
            
            var send = SendAsync(httpMethod, interfaceName, methodName, method.Version, method.RequiresKey, options, query);

            if (returnType == typeof(void))
            {
                send.Wait();
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

        /// <summary>
        /// Returns a function to transform the specified send task and response converter into the proper return type for a method
        /// </summary>
        /// <param name="returnType"></param>
        /// <returns></returns>
        protected Func<Task<RestResponse>, ResponseConverter, object> GetCachedContinuationFunction(Type returnType)
        {
            return _continuationFunctions.GetOrAdd(returnType, (t) =>
            {
                var genericMethod = ContinuationMethodInfo.MakeGenericMethod(t);
                var delegateMethod = genericMethod.CreateDelegate(typeof(Func<Task<RestResponse>, ResponseConverter, object>));
                return delegateMethod as Func<Task<RestResponse>, ResponseConverter, object>;
            });
        }
        
        private static object ContinuationFunction<TResult>(Task<RestResponse> response, ResponseConverter converter)
        {
            return response.ContinueWith((t) => 
            { 
                if (t.IsFaulted)
                    throw t.Exception.GetBaseException();

                if (t.IsCanceled)
                    throw new TaskCanceledException(t);
                    
                return converter.ReadResponse<TResult>(t.Result);
            });
        }
    }
}
