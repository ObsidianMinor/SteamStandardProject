using Steam.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Net.Utilities
{
    public static class TaskUtils
    {
        public static Task TimeoutWrap(this Task task, int timeout, Logger log)
        {
            CancellationTokenSource cancellationToken = new CancellationTokenSource(timeout);
            var _ = Task.Run(() => task, cancellationToken.Token).ContinueWith(async (t) =>
            {
                if (t.IsCanceled)
                    await log.ErrorAsync($"A task took too long to complete execution and was cancelled prematurely", new TaskCanceledException(t)).ConfigureAwait(false);

                if (t.IsFaulted)
                    await log.ErrorAsync($"A task threw an exception", t.Exception).ConfigureAwait(false);

            }).ContinueWith(SwallowExceptions);

            return Task.CompletedTask;
        }

        private static void SwallowExceptions(Task task)
        {
            if (task.IsFaulted)
            {
                Exception e = task.Exception; // if you get here you should rethink life
            }
        }

        /// <summary>
        /// Invokes the specified async event on another thread
        /// </summary>
        public static async Task TimedInvokeAsync(this AsyncEventHandler eventHandler, object source, EventArgs args, int timeout, Logger log)
        {
            if (eventHandler != null)
            {
                if (timeout >= -1)
                    await eventHandler.InvokeAsync(source, args).TimeoutWrap(timeout, log).ConfigureAwait(false);
                else
                    await eventHandler.InvokeAsync(source, args).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Invokes the specified async event on another thread
        /// </summary>
        public static async Task TimedInvokeAsync<TArgs>(this AsyncEventHandler<TArgs> eventHandler, object source, TArgs arg, int timeout, Logger log) where TArgs : EventArgs
        {
            if (eventHandler != null)
            {
                if (timeout >= -1)
                    await eventHandler.InvokeAsync(source, arg).TimeoutWrap(timeout, log).ConfigureAwait(false);
                else
                    await eventHandler.InvokeAsync(source, arg).ConfigureAwait(false);
            }
        }
    }
}