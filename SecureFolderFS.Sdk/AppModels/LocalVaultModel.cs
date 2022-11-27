using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultModel"/>
    public sealed class LocalVaultModel : IVaultModel
    {
        /// <inheritdoc/>
        public IFolder Folder { get; }

        /// <inheritdoc/>
        public string VaultName { get; }

        /// <inheritdoc/>
        public DateTime LastAccessedDate { get; private set; }

        public LocalVaultModel(IFolder folder)
        {
            Folder = folder;
            VaultName = folder.Name;
        }

        /// <inheritdoc/>
        public Task AccessVaultAsync(CancellationToken cancellationToken = default)
        {
            LastAccessedDate = DateTime.Now;

            // TODO: Persist LastAccessDate
            return Task.CompletedTask;
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
