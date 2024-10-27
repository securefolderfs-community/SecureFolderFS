using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultHealthService"/>
    public class VaultHealthService : IVaultHealthService
    {
        /// <inheritdoc/>
        public async Task<IResult> ScanFileAsync(IFile file, CancellationToken cancellationToken = default)
        {
            await Task.Delay(5);
            return Result.Success;
        }

        /// <inheritdoc/>
        public async Task<IResult> ScanFolderAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            await Task.Delay(5);
            return Result.Success;
        }
    }
}
