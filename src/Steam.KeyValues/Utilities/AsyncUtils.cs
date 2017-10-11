﻿using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.KeyValues.Utilities
{
    internal static class AsyncUtils
    {
        // Pre-allocate to avoid wasted allocations.
        public static readonly Task<bool> False = Task.FromResult(false);
        public static readonly Task<bool> True = Task.FromResult(true);

        internal static Task<bool> ToAsync(this bool value) => value ? True : False;

        public static Task CancelIfRequestedAsync(this CancellationToken cancellationToken)
        {
            return cancellationToken.IsCancellationRequested ? FromCanceled(cancellationToken) : null;
        }

        public static Task<T> CancelIfRequestedAsync<T>(this CancellationToken cancellationToken)
        {
            return cancellationToken.IsCancellationRequested ? FromCanceled<T>(cancellationToken) : null;
        }

        // From 4.6 on we could use Task.FromCanceled(), but we need an equivalent for
        // previous frameworks.
        public static Task FromCanceled(this CancellationToken cancellationToken)
        {
            Debug.Assert(cancellationToken.IsCancellationRequested);
            return new Task(() => { }, cancellationToken);
        }

        public static Task<T> FromCanceled<T>(this CancellationToken cancellationToken)
        {
            Debug.Assert(cancellationToken.IsCancellationRequested);
            return new Task<T>(() => default(T), cancellationToken);
        }

        // Task.Delay(0) is optimised as a cached task within the framework, and indeed
        // the same cached task that Task.CompletedTask returns as of 4.6, but we'll add
        // our own cached field for previous frameworks.
        internal static readonly Task CompletedTask = Task.Delay(0);

        public static Task WriteAsync(this TextWriter writer, char value, CancellationToken cancellationToken)
        {
            Debug.Assert(writer != null);
            return cancellationToken.IsCancellationRequested ? FromCanceled(cancellationToken) : writer.WriteAsync(value);
        }

        public static Task WriteAsync(this TextWriter writer, string value, CancellationToken cancellationToken)
        {
            Debug.Assert(writer != null);
            return cancellationToken.IsCancellationRequested ? FromCanceled(cancellationToken) : writer.WriteAsync(value);
        }

        public static Task WriteAsync(this TextWriter writer, char[] value, int start, int count, CancellationToken cancellationToken)
        {
            Debug.Assert(writer != null);
            return cancellationToken.IsCancellationRequested ? FromCanceled(cancellationToken) : writer.WriteAsync(value, start, count);
        }

        public static Task<int> ReadAsync(this TextReader reader, char[] buffer, int index, int count, CancellationToken cancellationToken)
        {
            Debug.Assert(reader != null);
            return cancellationToken.IsCancellationRequested ? FromCanceled<int>(cancellationToken) : reader.ReadAsync(buffer, index, count);
        }
    }
}