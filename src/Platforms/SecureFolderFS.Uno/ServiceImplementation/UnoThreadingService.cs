using System;
using Microsoft.UI.Dispatching;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <inheritdoc cref="IThreadingService"/>
    internal sealed class UnoThreadingService : IThreadingService
    {
        private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        /// <inheritdoc/>
        public IAwaitable ChangeThreadAsync()
        {
            return new ContextSwitchAwaitable(_dispatcherQueue);
        }

        private sealed class ContextSwitchAwaitable(DispatcherQueue dispatcherQueue) : IAwaitable
        {
            /// <inheritdoc/>
            public bool IsCompleted => dispatcherQueue.HasThreadAccess;

            /// <inheritdoc/>
            public void OnCompleted(Action continuation)
            {
                _ = dispatcherQueue.TryEnqueue(() =>
                {
                    continuation();
                });
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
