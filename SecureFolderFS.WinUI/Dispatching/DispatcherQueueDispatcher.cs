using System;
using System.Threading.Tasks;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;

namespace SecureFolderFS.WinUI.Dispatching
{
    internal sealed class DispatcherQueueDispatcher : IThreadDispatcher
    {
        private readonly DispatcherQueue _inner;

        public bool HasThreadAccess => _inner.HasThreadAccess;

        public DispatcherQueueDispatcher()
        {
            _inner = DispatcherQueue.GetForCurrentThread();
        }

        public Task DispatchAsync(Action action)
        {
            return _inner.EnqueueAsync(action);
        }
    }
}
