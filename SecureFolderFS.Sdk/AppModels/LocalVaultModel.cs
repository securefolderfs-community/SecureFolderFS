using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultModel"/>
    public sealed class LocalVaultModel : IVaultModel
    {
        private IFileSystemService FileSystemService { get; } = Ioc.Default.GetRequiredService<IFileSystemService>();

        /// <inheritdoc/>
        public IFolder Folder { get; }

        /// <inheritdoc/>
        public string VaultName { get; }

        public LocalVaultModel(IFolder folder)
        {
            Folder = folder;
            VaultName = folder.Name;
        }

        /// <inheritdoc/>
        public Task<bool> IsAccessibleAsync(CancellationToken cancellationToken = default)
        {
            if (Folder is not ILocatableFolder vaultFolder)
                return Task.FromResult(false);

            // TODO: There can be additional measures added to determine if the Folder is still accessible.
            return FileSystemService.DirectoryExistsAsync(vaultFolder.Path, cancellationToken);
        }

        /// <inheritdoc/>
        public bool Equals(IVaultModel? other)
        {
            if (other is null)
                return false;

            return Folder.Id.Equals(other.Folder.Id);
        }
    }
}
