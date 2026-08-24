using System;
using System.Collections.Generic;
using System.Threading;

namespace SecureFolderFS.Sdk.Api.Helpers
{
    /// <summary>
    /// Collapses duplicate in-flight requests so identical work is never started twice. While a request
    /// for a given key is outstanding, further requests for the same key are refused entry.
    /// </summary>
    public sealed class RequestCoalescerHelper
    {
        private readonly Lock _lock = new();
        private readonly HashSet<string> _inFlight = [];

        /// <summary>
        /// Attempts to claim exclusive ownership of a unit of work.
        /// </summary>
        /// <returns>A scope that releases the claim when disposed, or <see langword="null"/> when the
        /// same work is already in flight.</returns>
        public IDisposable? TryBeginScope(string key)
        {
            lock (_lock)
            {
                if (!_inFlight.Add(key))
                    return null;
            }

            return new Scope(this, key);
        }

        private void End(string key)
        {
            lock (_lock)
                _inFlight.Remove(key);
        }

        private sealed class Scope(RequestCoalescerHelper owner, string key) : IDisposable
        {
            /// <inheritdoc/>
            public void Dispose() => owner.End(key);
        }
    }
}