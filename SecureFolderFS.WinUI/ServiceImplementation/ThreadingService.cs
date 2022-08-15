using System;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.WinUI.ServiceImplementation
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
        public IAwaitable ExecuteOnUiThreadAsync()
        {
            return new UiThreadAwaitable(_dispatcherQueue);
        }

        /// <inheritdoc/>
        public Task ExecuteOnUiThreadAsync(Action action)
        {
            return _dispatcherQueue.EnqueueAsync(action);
        }

        private sealed class UiThreadAwaitable : IAwaitable
        {
            private readonly DispatcherQueue _dispatcherQueue;

            public bool IsCompleted => _dispatcherQueue.HasThreadAccess;

            public UiThreadAwaitable(DispatcherQueue dispatcherQueue)
            {
                _dispatcherQueue = dispatcherQueue;
            }

            public IAwaitable GetAwaiter()
            {
                return this;
            }

            public void GetResult()
            {
            }

            public void OnCompleted(Action continuation)
            {
                _ = _dispatcherQueue.EnqueueAsync(continuation);
            }
        }
    }
}
