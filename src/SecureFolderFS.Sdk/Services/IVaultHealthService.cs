using OwlCore.Storage;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.Services
{
    public interface IVaultHealthService
    {
        /// <summary>
        /// Gets the file health validator for a given <paramref name="vaultFolder"/>.
        /// </summary>
        /// <param name="vaultFolder">The <see cref="IFolder"/> that represents the vault.</param>
        /// <returns>A new instance of <see cref="IAsyncValidator{T}"/> to validate files.</returns>
        IAsyncValidator<IFile> GetFileValidator(IFolder vaultFolder);

        /// <summary>
        /// Gets the file health validator for a given <paramref name="vaultFolder"/>.
        /// </summary>
        /// <param name="vaultFolder">The <see cref="IFolder"/> that represents the vault.</param>
        /// <returns>A new instance of <see cref="IAsyncValidator{T}"/> to validate folders.</returns>
        IAsyncValidator<IFolder> GetFolderValidator(IFolder vaultFolder);

        IHealthIssueModel GetIssueInfo(IStorable storable);
    }
}
