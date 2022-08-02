using System.Collections.Generic;
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
        public async IAsyncEnumerable<IFileSystemInfoModel> GetFileSystemsAsync()
        {
            yield return new DokanyFileSystemDescriptor();

            await Task.CompletedTask;
        }
    }
}
