using System;

namespace SecureFolderFS.Sdk.Helpers
{
    public sealed class CollectionReorderHelper<T> : IDisposable
        where T : class
    {
        private T? _previous;

        public event EventHandler<T>? Reordered;

        public void RegisterRemove(T removed)
        {
            _previous = removed;
        }

        public void RegisterAdd(T added)
        {
            if (_previous == added)
                Reordered?.Invoke(this, added);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _previous = null;
            Reordered = null;
        }
    }
}