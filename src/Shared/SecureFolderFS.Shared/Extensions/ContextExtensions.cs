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

        public static Task PostOrExecuteAsync(this SynchronizationContext? synchronizationContext, Func<Task> func)
        {
            if (synchronizationContext is null)
                return func();

            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            synchronizationContext.Post(async _ =>
            {
                try
                {
                    await func();
                    tcs.SetResult();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);

            return tcs.Task;
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
