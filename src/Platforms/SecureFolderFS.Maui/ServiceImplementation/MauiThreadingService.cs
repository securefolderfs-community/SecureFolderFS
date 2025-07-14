using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Maui.ServiceImplementation
{
    /// <inheritdoc cref="IThreadingService"/>
    internal sealed class MauiThreadingService : IThreadingService
    {
        /// <inheritdoc/>
        public SynchronizationContext? GetContext()
        {
            return SynchronizationContext.Current;
        }

        /// <inheritdoc/>
        public IAwaitable ChangeThreadAsync()
        {
            return new ContextSwitchAwaitable();
        }
        
        private sealed class ContextSwitchAwaitable : IAwaitable
        {
            /// <inheritdoc/>
            public bool IsCompleted => MainThread.IsMainThread;
            
            /// <inheritdoc/>
            public void OnCompleted(Action continuation)
            {
                MainThread.BeginInvokeOnMainThread(continuation);
            }
            
            /// <inheritdoc/>
            public IAwaitable GetAwaiter()
            {
                return this;
            }

            /// <inheritdoc/>
            public void GetResult()
            {
            }
        }
    }
}
