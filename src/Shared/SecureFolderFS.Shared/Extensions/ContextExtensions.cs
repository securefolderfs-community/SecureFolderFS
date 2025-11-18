using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Extensions
{
    public static class ContextExtensions
    {
        public static void PostOrExecute(this SynchronizationContext? synchronizationContext, SendOrPostCallback action, object? state = null)
        {
            if (synchronizationContext is null)
                action(state);
            else
                synchronizationContext.Post(action, state);
        }

        public static async Task PostOrExecuteAsync(this SynchronizationContext? synchronizationContext, Func<object?, Task> func, object? state = null)
        {
            if (synchronizationContext is null)
                await func(state);
            else
                synchronizationContext.Post(async obj => await func(obj), state);
        }

        public static void TryCancel(this CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                cancellationTokenSource.Cancel();
            }
            catch (Exception)
            {
            }
        }
    }
}
