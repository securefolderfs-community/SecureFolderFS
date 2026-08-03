using System.Runtime.CompilerServices;
using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ViewModels.Authentication;

namespace SecureFolderFS.Tests.ServiceImplementation
{
    /// <inheritdoc cref="IVaultCredentialsService"/>
    internal sealed class MockVaultCredentialsService : BaseVaultCredentialsService
    {
        /// <inheritdoc/>
        public override async IAsyncEnumerable<AuthenticationViewModel> GetCreationAsync(IFolder vaultFolder, string vaultId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _ = vaultFolder;
            _ = vaultId;
            await Task.CompletedTask;
            yield break;
        }

        /// <inheritdoc/>
        protected override async IAsyncEnumerable<AuthenticationViewModel> GetLoginAsync(
            IFolder vaultFolder,
            AuthenticationMethod unlockProcedure,
            string vaultId,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _ = vaultId;
            await Task.CompletedTask;
            foreach (var item in EnumerateLoginMethods(unlockProcedure))
            {
                yield return item switch
                {
                    Constants.Vault.Authentication.AUTH_PASSWORD => new PasswordLoginViewModel(),
                    Constants.Vault.Authentication.AUTH_KEYFILE => new KeyFileLoginViewModel(vaultFolder),
                    _ => throw new NotSupportedException($"The authentication method '{item}' is not supported by the test platform.")
                };
            }
        }
    }
}
