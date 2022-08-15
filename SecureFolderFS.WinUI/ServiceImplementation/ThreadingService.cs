using System;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Utils;
using SecureFolderFS.WinUI.Dispatching;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IThreadingService"/>
    internal sealed class ThreadingService : IThreadingService
    {
        private readonly IThreadDispatcher _threadDispatcher;

        public ThreadingService()
        {
            _threadDispatcher = new DispatcherQueueDispatcher();
        }

        /// <inheritdoc/>
        public IAwaitable ExecuteOnUiThreadAsync()
        {
            return new UiThreadAwaitable(_threadDispatcher);
        }

        /// <inheritdoc/>
        public Task ExecuteOnUiThreadAsync(Action action)
        {
            return _threadDispatcher.DispatchAsync(action);
        }

        private sealed class UiThreadAwaitable : IAwaitable
        {
            private readonly IThreadDispatcher _threadDispatcher;

            public bool IsCompleted => _threadDispatcher.HasThreadAccess;

            public UiThreadAwaitable(IThreadDispatcher threadDispatcher)
            {
                _threadDispatcher = threadDispatcher;
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
                _ = _threadDispatcher.DispatchAsync(continuation);
            }
        }
    }
}
