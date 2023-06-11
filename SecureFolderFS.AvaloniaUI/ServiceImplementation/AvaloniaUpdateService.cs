using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    // TODO: Implement updates on AvaloniaUI
    /// <inheritdoc cref="IUpdateService"/>
    internal sealed class AvaloniaUpdateService : IUpdateService
    {
        /// <inheritdoc/>
        public Task<bool> IsSupportedAsync()
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsUpdateAvailableAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<IResult> UpdateAsync(IProgress<double>? progress, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}