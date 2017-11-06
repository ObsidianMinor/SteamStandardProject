using System;
using System.Threading.Tasks;

namespace Steam
{
    public delegate Task AsyncEventHandler(object source, EventArgs args);

    public delegate Task AsyncEventHandler<in TArgs>(object source, TArgs args) where TArgs : EventArgs;
    
    public static class AsyncEventExtensions
    {
        public static async Task InvokeAsync(this AsyncEventHandler handler, object source, EventArgs args)
        {
            if (handler == null)
                return;

            Delegate[] invocations = handler.GetInvocationList();
            for (int i = 0; i < invocations.Length; i++)
                await ((AsyncEventHandler)invocations[i])(source, args).ConfigureAwait(false);
        }

        public static async Task InvokeAsync<TArgs>(this AsyncEventHandler<TArgs> handler, object source, TArgs args) where TArgs : EventArgs
        {
            if (handler == null)
                return;

            Delegate[] invocations = handler.GetInvocationList();
            for (int i = 0; i < invocations.Length; i++)
                await ((AsyncEventHandler<TArgs>)invocations[i])(source, args).ConfigureAwait(false);
        }
    }
}
