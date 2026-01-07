using System;
using System.Collections.Generic;

namespace SecureFolderFS.Shared.ComponentModel
{
    /// <summary>
    /// An implementation of <see cref="IProgress{T}"/> that broadcasts progress reports to multiple subscribers.
    /// </summary>
    /// <typeparam name="T">The type of progress update value.</typeparam>
    public sealed class MulticastProgress<T> : IProgress<T>, IDisposable
    {
        private readonly List<IProgress<T>> _subscribers;
        private readonly object _lock;

        public MulticastProgress()
        {
            _subscribers = new List<IProgress<T>>();
            _lock = new object();
        }

        /// <summary>
        /// Subscribes an <see cref="IProgress{T}"/> to receive progress reports.
        /// </summary>
        /// <param name="progress">The progress instance to subscribe.</param>
        /// <returns>An <see cref="IDisposable"/> that can be used to unsubscribe.</returns>
        public IDisposable Subscribe(IProgress<T> progress)
        {
            if (progress is null)
                throw new ArgumentNullException(nameof(progress));

            lock (_lock)
            {
                _subscribers.Add(progress);
            }

            return new Subscription(this, progress);
        }

        /// <inheritdoc/>
        public void Report(T value)
        {
            List<IProgress<T>> subscribersCopy;
            lock (_lock)
            {
                subscribersCopy = new List<IProgress<T>>(_subscribers);
            }

            foreach (var subscriber in subscribersCopy)
            {
                subscriber.Report(value);
            }
        }

        private void Unsubscribe(IProgress<T> progress)
        {
            lock (_lock)
            {
                _subscribers.Remove(progress);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (_lock)
            {
                _subscribers.Clear();
            }
        }

        private sealed class Subscription : IDisposable
        {
            private readonly MulticastProgress<T> _parent;
            private readonly IProgress<T> _progress;
            private bool _disposed;

            public Subscription(MulticastProgress<T> parent, IProgress<T> progress)
            {
                _parent = parent;
                _progress = progress;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                _parent.Unsubscribe(_progress);
            }
        }
    }
}

