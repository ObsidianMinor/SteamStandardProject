using Steam.Net.GameCoordinators.Messages;
using Steam.Net.Messages;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Steam.Net
{
    public class DefaultReceiveMethodResolver : IReceiveMethodResolver
    {
        private static readonly MethodInfo InvokeBodyAsyncInfo = 
            typeof(DefaultReceiveMethodResolver).GetMethod(nameof(InvokeBodyAsync), BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo InvokeGCBodyAsyncInfo =
            typeof(DefaultReceiveMethodResolver).GetMethod(nameof(InvokeBodyAsync), BindingFlags.NonPublic | BindingFlags.Static);

        public virtual bool TryResolve(MethodInfo method, object target, out MessageReceiver receiver)
        {
            receiver = null;
            if (method.ReturnType == typeof(Task))
                return ResolveTask(method, target, out receiver);
            else
                return false;
        }
        
        protected virtual bool ResolveTask(MethodInfo method, object target, out MessageReceiver receiver)
        {
            receiver = null;

            var parameters = method.GetParameters();
            if (parameters.Length != 1)
                return false;

            var param = parameters.Single().ParameterType;

            if (param == typeof(NetworkMessage))
            {
                receiver = method.CreateDelegate(typeof(MessageReceiver), target) as MessageReceiver;
                return true;
            }
            else if (param == typeof(Header))
            {
                var inbetween = method.CreateDelegate(typeof(Func<Header, Task>), target) as Func<Header, Task>;
                receiver = (message) => inbetween(message.Header);
                return true;
            }
            else if (param == typeof(ClientHeader))
            {
                var inbetween = method.CreateDelegate(typeof(Func<ClientHeader, Task>), target) as Func<ClientHeader, Task>;
                receiver = (message) => inbetween(message.Header as ClientHeader);
                return true;
            }
            else if (param == typeof(ProtobufClientHeader))
            {
                var inbetween = method.CreateDelegate(typeof(Func<ClientHeader, Task>), target) as Func<ClientHeader, Task>;
                receiver = (message) => inbetween(message.Header as ClientHeader);
                return true;
            }
            else
            {
                var invoke = InvokeBodyAsyncInfo
                    .MakeGenericMethod(param)
                    .CreateDelegate(typeof(Func<NetworkMessage, Delegate, Task>)) as Func<NetworkMessage, Delegate, Task>;
                Delegate d = method.CreateDelegate(typeof(Func<,>).MakeGenericType(param, typeof(Task)), target);
                receiver = (message) => invoke(message, d);
                return true;
            }
        }

        public virtual bool TryResolve(MethodInfo method, object gc, out GameCoordinatorReceiver receiver)
        {
            receiver = null;
            if (method.ReturnType == typeof(Task))
                return ResolveTask(method, gc, out receiver);
            else
                return false;
        }
        
        protected virtual bool ResolveTask(MethodInfo method, object gc, out GameCoordinatorReceiver receiver)
        {
            receiver = null;

            var parameters = method.GetParameters();
            if (parameters.Length != 1)
                return false;

            var param = parameters.Single().ParameterType;

            if (param == typeof(GameCoordinatorMessage))
            {
                receiver = method.CreateDelegate(typeof(GameCoordinatorReceiver), gc) as GameCoordinatorReceiver;
                return true;
            }
            else if (param == typeof(Header))
            {
                var inbetween = method.CreateDelegate(typeof(Func<Header, Task>), gc) as Func<Header, Task>;
                receiver = (message) => inbetween(message.Header);
                return true;
            }
            else if (param == typeof(GameCoordinatorProtobufHeader))
            {
                var inbetween = method.CreateDelegate(typeof(Func<GameCoordinatorProtobufHeader, Task>), gc) as Func<GameCoordinatorProtobufHeader, Task>;
                receiver = (message) => inbetween(message.Header as GameCoordinatorProtobufHeader);
                return true;
            }
            else
            {
                var invoke = InvokeGCBodyAsyncInfo
                    .MakeGenericMethod(param)
                    .CreateDelegate(typeof(Func<GameCoordinatorMessage, Delegate, Task>)) as Func<GameCoordinatorMessage, Delegate, Task>;
                Delegate d = method.CreateDelegate(typeof(Func<,>).MakeGenericType(param, typeof(Task)), gc);
                receiver = (message) => invoke(message, d);
                return true;
            }
        }

        private static Task InvokeBodyAsync<T>(NetworkMessage message, Delegate d)
        {
            return (d as Func<T, Task>)(message.Deserialize<T>());
        }

        private static Task InvokeGCBodyAsync<T>(GameCoordinatorMessage message, Delegate d)
        {
            return (d as Func<T, Task>)(message.Deserialize<T>());
        }
    }
}
