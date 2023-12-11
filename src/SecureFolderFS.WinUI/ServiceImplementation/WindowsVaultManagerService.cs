using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ViewModels;
using SecureFolderFS.WinUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultManagerService"/>
    public sealed class WindowsVaultManagerService : BaseVaultManagerService
    {
        /// <inheritdoc/>
        public override async IAsyncEnumerable<AuthenticationViewModel> GetLoginAuthenticationAsync(IFolder vaultFolder, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return new KeyFileLoginViewModel(Core.Constants.Vault.AuthenticationMethods.AUTH_KEYFILE, vaultFolder);
            //yield return new WindowsHelloLoginViewModel(Core.Constants.Vault.AuthenticationMethods.AUTH_WINDOWS_HELLO, vaultFolder);
            //yield return new PasswordLoginViewModel(Core.Constants.Vault.AuthenticationMethods.AUTH_PASSWORD, vaultFolder);
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<AuthenticationViewModel> GetCreationAuthenticationAsync(IFolder vaultFolder, string vaultId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Password
            yield return new PasswordCreationViewModel(Core.Constants.Vault.AuthenticationMethods.AUTH_PASSWORD, vaultFolder);

            // Windows Hello
            if (await KeyCredentialManager.IsSupportedAsync().AsTask(cancellationToken))
                yield return new WindowsHelloCreationViewModel(vaultId, Core.Constants.Vault.AuthenticationMethods.AUTH_WINDOWS_HELLO, vaultFolder);
            
            // Key File
            yield return new KeyFileCreationViewModel(vaultId, Core.Constants.Vault.AuthenticationMethods.AUTH_KEYFILE, vaultFolder);
        }
    }
}
