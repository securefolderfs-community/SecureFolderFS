using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;
using SecureFolderFS.WinUI.AppModels;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultService"/>
    internal sealed class VaultService : IVaultService
    {
        /// <inheritdoc/>
        public IAsyncValidator<IFolder> GetVaultValidator()
        {
            return new VaultValidator();
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IFileSystemInfoModel> GetFileSystemsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return new DokanyFileSystemDescriptor();

            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<ICipherInfoModel> GetContentCiphersAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // TODO: Implement
            await Task.CompletedTask;
            yield break;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<ICipherInfoModel> GetFilenameCiphersAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // TODO: Implement
            await Task.CompletedTask;
            yield break;
        }
    }
}
