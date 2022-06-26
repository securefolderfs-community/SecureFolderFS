using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultModel"/>
    public sealed class VaultModel : IVaultModel
    {
        private IFileSystemService FileSystemService { get; } = Ioc.Default.GetRequiredService<IFileSystemService>();

        /// <inheritdoc/>
        public IFolder Folder { get; }

        /// <inheritdoc/>
        public string VaultName { get; }

        /// <inheritdoc/>
        public IDisposable? FolderLock { get; }

        public VaultModel(IFolder folder)
        {
            Folder = folder;
            VaultName = folder.Name;
        }

        /// <inheritdoc/>
        public bool Equals(IVaultModel? other)
        {
            if (other is null)
                return false;

            return other.Folder.Path.Equals(Folder.Path);
        }

        /// <inheritdoc/>
        public Task<bool> IsAccessibleAsync()
        {
            // TODO: There can be additional measures added to determine if the Folder hasn't expired.
            return FileSystemService.DirectoryExistsAsync(Folder.Path);
        }
    }
}
