using Avalonia.Threading;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Utils;
using System;

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
        public IAwaitable ChangeThreadAsync()
        {
            return new ContextSwitchAwaitable(_dispatcher);
        }

        private sealed class ContextSwitchAwaitable : IAwaitable
        {
            private readonly Dispatcher _dispatcher;

            /// <inheritdoc/>
            public bool IsCompleted => _dispatcher.CheckAccess();

            public ContextSwitchAwaitable(Dispatcher dispatcher)
            {
                _dispatcher = dispatcher;
            }

            /// <inheritdoc/>
            public IAwaitable GetAwaiter()
            {
                return this;
            }

            /// <inheritdoc/>
            public void OnCompleted(Action continuation)
            {
                _ = _dispatcher.InvokeAsync(continuation);
            }

            /// <inheritdoc/>
            public void GetResult()
            {
            }
        }
    }
}