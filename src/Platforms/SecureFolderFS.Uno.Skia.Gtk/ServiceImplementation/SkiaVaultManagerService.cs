using System.Runtime.CompilerServices;
using OwlCore.Storage;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ViewModels;

namespace SecureFolderFS.Uno.Skia.Gtk.ServiceImplementation
{
    /// <inheritdoc cref="IVaultManagerService"/>
    internal sealed class SkiaVaultManagerService : BaseVaultManagerService
    {
        /// <inheritdoc/>
        public override async IAsyncEnumerable<AuthenticationViewModel> GetAvailableSecurityAsync(IFolder vaultFolder, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var vaultReader = new VaultReader(vaultFolder, StreamSerializer.Instance);
            var config = await vaultReader.ReadConfigurationAsync(cancellationToken);
            var authenticationMethods = config.AuthenticationMethod.Split(';');

            foreach (var item in authenticationMethods)
            {
                var supported = item switch
                {
                    Core.Constants.Vault.AuthenticationMethods.AUTH_PASSWORD => true,
                    Core.Constants.Vault.AuthenticationMethods.AUTH_WINDOWS_HELLO => false,
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
                    Core.Constants.Vault.AuthenticationMethods.AUTH_KEYFILE => new KeyFileLoginViewModel(Core.Constants.Vault.AuthenticationMethods.AUTH_KEYFILE, vaultFolder),
                    _ => throw new NotSupportedException($"The authentication method '{item}' is not supported by the platform.")
                };
            }
        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<AuthenticationViewModel> GetAllSecurityAsync(IFolder vaultFolder, string vaultId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Password
            yield return new PasswordCreationViewModel(Core.Constants.Vault.AuthenticationMethods.AUTH_PASSWORD, vaultFolder);

            // Key File
            yield return new KeyFileCreationViewModel(vaultId, Core.Constants.Vault.AuthenticationMethods.AUTH_KEYFILE, vaultFolder);

            await Task.CompletedTask;
        }
    }
}
