using OwlCore.Storage;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultHealthService"/>
    public class VaultHealthService : IVaultHealthService
    {
        /// <inheritdoc/>
        public virtual IAsyncValidator<IFile> GetFileValidator(IFolder vaultFolder)
        {
            return new FileValidator(vaultFolder);
        }

        /// <inheritdoc/>
        public virtual IAsyncValidator<IFolder> GetFolderValidator(IFolder vaultFolder)
        {
            return new FolderValidator(vaultFolder);
        }

        /// <inheritdoc/>
        public IHealthIssueModel GetIssueInfo(IStorable storable)
        {
            return null;
        }
    }
}
