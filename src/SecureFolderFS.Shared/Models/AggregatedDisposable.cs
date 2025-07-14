using System;
using System.Collections.ObjectModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Shared.Models
{
    /// <inheritdoc cref="IDisposable"/>
    public sealed class AggregatedDisposable : Collection<IDisposable>, IDisposable
    {
        public AggregatedDisposable(IDisposable[] disposables)
        {
            this.AddMultiple(disposables);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.DisposeElements();
            this.ClearItems();
        }
    }
}
