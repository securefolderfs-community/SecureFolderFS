using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Maui.Platforms.Android.Storage;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.UI.ServiceImplementation;

namespace SecureFolderFS.Maui.Platforms.Android.ServiceImplementation
{
    /// <inheritdoc cref="IVaultManagerService"/>
    internal sealed class AndroidVaultManagerService : BaseVaultManagerService
    {
        /// <inheritdoc/>
        public override Task<IFolder> CreateFileSystemAsync(IVaultModel vaultModel, IDisposable unlockContract, CancellationToken cancellationToken)
        {
            // TODO: Add implementation for android DocumentsProvider FS

            var result = new SystemFolder(Microsoft.Maui.Storage.FileSystem.Current.AppDataDirectory);
            return Task.FromResult<IFolder>(result);
            //throw new NotImplementedException();
        }
    }
}
