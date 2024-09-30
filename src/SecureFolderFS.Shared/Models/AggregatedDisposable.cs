using SecureFolderFS.Shared.Extensions;
using System;

namespace SecureFolderFS.Shared.Models
{
    /// <inheritdoc cref="IDisposable"/>
    public sealed class AggregatedDisposable : IDisposable
    {
        private readonly IDisposable[] _disposables;

        public AggregatedDisposable(IDisposable[] disposables)
        {
            _disposables = disposables;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _disposables.DisposeElements();
        }
    }
}
