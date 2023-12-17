using System;
using Microsoft.UI.Dispatching;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Utilities;

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <inheritdoc cref="IThreadingService"/>
    internal sealed class ThreadingService : IThreadingService
    {
        private readonly DispatcherQueue _dispatcherQueue;

        public ThreadingService()
        {
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        }

        /// <inheritdoc/>
        public IAwaitable ChangeThreadAsync()
        {
            return new ContextSwitchAwaitable(_dispatcherQueue);
        }

        private sealed class ContextSwitchAwaitable : IAwaitable
        {
            private readonly DispatcherQueue _dispatcherQueue;

            /// <inheritdoc/>
            public bool IsCompleted => _dispatcherQueue.HasThreadAccess;

            public ContextSwitchAwaitable(DispatcherQueue dispatcherQueue)
            {
                _dispatcherQueue = dispatcherQueue;
            }

            /// <inheritdoc/>
            public void OnCompleted(Action continuation)
            {
                _ = _dispatcherQueue.TryEnqueue(() =>
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
