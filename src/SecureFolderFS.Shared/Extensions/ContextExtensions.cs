using System.Threading;

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
    }
}
