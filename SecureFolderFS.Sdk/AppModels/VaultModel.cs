using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.Sdk.AppModels
{
    [Serializable]
    public sealed class VaultModel : IVaultModel
    {
        [JsonIgnore]
        private IFileSystemService FileSystemService { get; } = Ioc.Default.GetRequiredService<IFileSystemService>();

        /// <inheritdoc/>
        [JsonIgnore]
        public IFolder Folder { get; }

        /// <inheritdoc/>
        [JsonIgnore]
        public IDisposable? FolderLock { get; }

        public VaultModel(IFolder folder)
        {
            Folder = folder;
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
