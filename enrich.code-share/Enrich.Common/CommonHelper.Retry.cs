using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Common
{
    public static partial class CommonHelper
    {
        private const int NumberOfRetry = 2;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(100);

        public static Task RetryAsync(Func<Task> action, Func<Exception, int, Task> actionOnRetry = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            async Task OnRetryAsync(Exception ex, int num)
            {
                if (actionOnRetry != null) await actionOnRetry(ex, num);
                await Task.Delay(NumberOfRetry * RetryDelay, cancellationToken);
            }

            return Policy.Handle<Exception>().RetryAsync(NumberOfRetry, OnRetryAsync).ExecuteAsync(action);
        }

        public static Task<T> RetryAsync<T>(Func<CancellationToken, Task<T>> action, Func<Exception, int, Task> actionOnRetry = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            async Task OnRetryAsync(Exception ex, int num)
            {
                if (actionOnRetry != null) await actionOnRetry(ex, num);
                await Task.Delay(NumberOfRetry * RetryDelay, cancellationToken);
            }

            return Policy.Handle<Exception>().RetryAsync(NumberOfRetry, OnRetryAsync).ExecuteAsync(action, cancellationToken);
        }

        public static Task<T> RetryAsync<T>(Func<Task<T>> action, Func<Exception, int, Task> actionOnRetry = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            async Task OnRetryAsync(Exception ex, int num)
            {
                if (actionOnRetry != null) await actionOnRetry(ex, num);
                await Task.Delay(NumberOfRetry * RetryDelay, cancellationToken);
            }

            return Policy.Handle<Exception>().RetryAsync(NumberOfRetry, OnRetryAsync).ExecuteAsync(action);
        }
    }
}

