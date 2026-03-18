using System;
using System.Threading;

namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// An implementation of <see cref="IProgress{T}"/> that broadcasts progress reports to multiple subscribers.
    /// Optimized for the common case of 2-3 subscribers with zero allocations in the hot path.
    /// </summary>
    /// <typeparam name="T">The type of progress update value.</typeparam>
    public sealed class MulticastProgress<T> : IProgress<T>, IDisposable
    {
        private volatile IProgress<T>? _subscriber1;
        private volatile IProgress<T>? _subscriber2;
        private volatile IProgress<T>? _subscriber3;

        /// <summary>
        /// Subscribes an <see cref="IProgress{T}"/> to receive progress reports.
        /// </summary>
        /// <param name="progress">The progress instance to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe.</returns>
        public IDisposable Subscribe(IProgress<T> progress)
        {
            if (progress is null)
                throw new ArgumentNullException(nameof(progress));

            // Try to assign to the first available slot
            if (_subscriber1 is null && Interlocked.CompareExchange(ref _subscriber1, progress, null) is null)
                return new Subscription(this, progress, 1);

            if (_subscriber2 is null && Interlocked.CompareExchange(ref _subscriber2, progress, null) is null)
                return new Subscription(this, progress, 2);

            if (_subscriber3 is null && Interlocked.CompareExchange(ref _subscriber3, progress, null) is null)
                return new Subscription(this, progress, 3);

            throw new InvalidOperationException("Maximum number of subscribers (3) reached.");
        }

        /// <inheritdoc/>
        public void Report(T value)
        {
            // Read volatile fields - no lock, no allocation, no array iteration
            _subscriber1?.Report(value);
            _subscriber2?.Report(value);
            _subscriber3?.Report(value);
        }

        private void Unsubscribe(IProgress<T> progress, int slot)
        {
            // Clear the specific slot
            switch (slot)
            {
                case 1:
                    Interlocked.CompareExchange(ref _subscriber1, null, progress);
                    break;
                case 2:
                    Interlocked.CompareExchange(ref _subscriber2, null, progress);
                    break;
                case 3:
                    Interlocked.CompareExchange(ref _subscriber3, null, progress);
                    break;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _subscriber1 = null;
            _subscriber2 = null;
            _subscriber3 = null;
        }

        private sealed class Subscription : IDisposable
        {
            private readonly MulticastProgress<T> _parent;
            private readonly IProgress<T> _progress;
            private readonly int _slot;
            private bool _disposed;

            public Subscription(MulticastProgress<T> parent, IProgress<T> progress, int slot)
            {
                _parent = parent;
                _progress = progress;
                _slot = slot;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                _parent.Unsubscribe(_progress, _slot);
            }
        }
    }
}

