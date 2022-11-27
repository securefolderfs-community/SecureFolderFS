using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultModel"/>
    public sealed class LocalVaultModel : IVaultModel
    {
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
        public bool Equals(IVaultModel? other)
        {
            if (other is null)
                return false;

            return Folder.Id.Equals(other.Folder.Id);
        }
    }
}
