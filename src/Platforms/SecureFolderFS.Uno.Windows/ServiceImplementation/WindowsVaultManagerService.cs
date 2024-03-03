using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ViewModels;
using SecureFolderFS.Uno.ViewModels;
using Windows.Security.Credentials;

namespace SecureFolderFS.Uno.Windows.ServiceImplementation
{
    /// <inheritdoc cref="IVaultManagerService"/>
    public sealed class WindowsVaultManagerService : BaseVaultManagerService
    {
        /// <inheritdoc/>
        public override async IAsyncEnumerable<AuthenticationViewModel> GetLoginAuthenticationAsync(IFolder vaultFolder, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var vaultReader = new VaultReader(vaultFolder, StreamSerializer.Instance);
            var config = await vaultReader.ReadConfigurationAsync(cancellationToken);
            var authenticationMethods = config.AuthenticationMethod.Split(';'); // TODO: Move to Constants

            foreach (var item in authenticationMethods)
            {
                var supported = item switch
                {
                    Core.Constants.Vault.AuthenticationMethods.AUTH_PASSWORD => true,
                    Core.Constants.Vault.AuthenticationMethods.AUTH_WINDOWS_HELLO => await KeyCredentialManager.IsSupportedAsync().AsTask(cancellationToken),
                    Core.Constants.Vault.AuthenticationMethods.AUTH_KEYFILE => true,
                    _ => false
                };

                if (!supported)
                    throw new NotSupportedException($"The authentication method '{item}' is not supported by the platform.");
            }

            foreach (var item in authenticationMethods)
            {
                yield return item switch
                {
                    Core.Constants.Vault.AuthenticationMethods.AUTH_PASSWORD => new PasswordLoginViewModel(Core.Constants.Vault.AuthenticationMethods.AUTH_PASSWORD, vaultFolder),
                    Core.Constants.Vault.AuthenticationMethods.AUTH_WINDOWS_HELLO => new WindowsHelloLoginViewModel(Core.Constants.Vault.AuthenticationMethods.AUTH_WINDOWS_HELLO, vaultFolder),
                    Core.Constants.Vault.AuthenticationMethods.AUTH_KEYFILE => new KeyFileLoginViewModel(Core.Constants.Vault.AuthenticationMethods.AUTH_KEYFILE, vaultFolder),
                    _ => throw new NotSupportedException($"The authentication method '{item}' is not supported by the platform.")
                };
            }
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
