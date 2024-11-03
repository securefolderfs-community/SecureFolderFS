using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ViewModels;

namespace SecureFolderFS.Uno.Platforms.MacCatalyst.ServiceImplementation
{
    /// <inheritdoc cref="IVaultService"/>
    internal sealed class MacOsVaultCredentialsService : BaseVaultCredentialsService
    {
        /// <inheritdoc/>
        public override IEnumerable<string> GetContentCiphers()
        {
            // XChaCha20-Poly1305 is not supported in NSec implementation for iOS (Catalyst)
            // Trackers:
            // - https://nsec.rocks/docs/install#supported-platforms
            
            yield return Core.Cryptography.Constants.CipherId.AES_GCM;
        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<AuthenticationViewModel> GetLoginAsync(IFolder vaultFolder, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var vaultReader = new VaultReader(vaultFolder, StreamSerializer.Instance);
            var config = await vaultReader.ReadConfigurationAsync(cancellationToken);
            var authenticationMethods = config.AuthenticationMethod.Split(Constants.Vault.Authentication.SEPARATOR);

            foreach (var item in authenticationMethods)
            {
                yield return item switch
                {
                    Constants.Vault.Authentication.AUTH_PASSWORD => new PasswordLoginViewModel(),
                    Constants.Vault.Authentication.AUTH_KEYFILE => new KeyFileLoginViewModel(vaultFolder),
                    _ => throw new NotSupportedException($"The authentication method '{item}' is not supported by the platform.")
                };
            }
        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<AuthenticationViewModel> GetCreationAsync(IFolder vaultFolder, string vaultId,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Password
            yield return new PasswordCreationViewModel();

            // Key File
            yield return new KeyFileCreationViewModel(vaultId);

            await Task.CompletedTask;
        }
    }
}
