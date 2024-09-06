using OwlCore.Storage;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.UI.ServiceImplementation;
using System.Runtime.CompilerServices;

namespace SecureFolderFS.Tests.ServiceImplementation
{
    /// <inheritdoc cref="IVaultService"/>
    internal sealed class MockVaultService : BaseVaultService
    {
        /// <inheritdoc/>
        public override IEnumerable<IFileSystemInfoModel> GetFileSystems()
        {
            yield break;
        }

        public override async IAsyncEnumerable<AuthenticationViewModel> GetLoginAsync(IFolder vaultFolder, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }

        public override async IAsyncEnumerable<AuthenticationViewModel> GetCreationAsync(IFolder vaultFolder, string vaultId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }
    }
}
