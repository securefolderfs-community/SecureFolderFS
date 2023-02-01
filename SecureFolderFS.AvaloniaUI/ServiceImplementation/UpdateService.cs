using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.AvaloniaUI.ServiceImplementation
{
    // TODO implement
    /// <inheritdoc cref="IUpdateService"/>
    internal sealed class UpdateService : IUpdateService
    {
        public Task<bool> IsSupportedAsync()
        {
            return Task.FromResult(false);
        }

        public Task<bool> InitializeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUpdateAvailableAsync()
        {
            throw new NotImplementedException();
        }

        public Task<AppUpdateResultType> UpdateAsync(IProgress<double>? progress, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}