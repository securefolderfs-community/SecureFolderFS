using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    /// <inheritdoc cref="IThreadingService"/>
    internal sealed class ThreadingService : IThreadingService
    {
        private readonly Dispatcher _dispatcher;

        public ThreadingService()
        {
            _dispatcher = Dispatcher.UIThread;
        }

        /// <inheritdoc/>
        public IAwaitable ExecuteOnUiThreadAsync()
        {
            return new UiThreadAwaitable(_dispatcher);
        }

        /// <inheritdoc/>
        public Task ExecuteOnUiThreadAsync(Action action)
        {
            return _dispatcher.InvokeAsync(action);
        }

        // TODO ensure this is implemented properly
        private sealed class UiThreadAwaitable : IAwaitable
        {
            private readonly Dispatcher _dispatcher;

            /// <inheritdoc/>
            public bool IsCompleted => _dispatcher.CheckAccess();

            public UiThreadAwaitable(Dispatcher dispatcher)
            {
                _dispatcher = dispatcher;
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

            /// <inheritdoc/>
            public void OnCompleted(Action continuation)
            {
                _ = _dispatcher.InvokeAsync(continuation);
            }
        }
    }
}