using Steam.Net.GameCoordinators;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Steam.Net
{
    public class DefaultReceiveMethodResolver : IReceiveMethodResolver
    {
        protected virtual bool ResolveVoid(MethodInfo method, object target, out MessageReceiver receiver)
        {
            throw new NotImplementedException();
        }

        protected virtual bool ResolveTask(MethodInfo method, object target, out MessageReceiver receiver)
        {
            throw new NotImplementedException();
        }

        public virtual bool TryResolve(MethodInfo method, object target, out MessageReceiver receiver)
        {
            receiver = null;
            if (method.ReturnType == typeof(Task))
            {
                return ResolveTask(method, target, out receiver);
            }
            else if (method.ReturnType == typeof(void))
            {
                return ResolveVoid(method, target, out receiver);
            }
            else
                return false;
        }

        protected virtual bool ResolveVoid(MethodInfo method, object gc, out GameCoordinatorReceiver receiver)
        {
            throw new NotImplementedException();
        }

        protected virtual bool ResolveTask(MethodInfo method, object gc, out GameCoordinatorReceiver receiver)
        {
            throw new NotImplementedException();
        }

        public virtual bool TryResolve(MethodInfo method, object gc, out GameCoordinatorReceiver receiver)
        {
            receiver = null;
            if (method.ReturnType == typeof(Task))
            {
                return ResolveTask(method, gc, out receiver);
            }
            else if (method.ReturnType == typeof(void))
            {
                return ResolveVoid(method, gc, out receiver);
            }
            else
                return false;
        }
    }
}
