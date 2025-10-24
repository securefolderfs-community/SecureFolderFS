using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using OwlCore.Storage;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.AppModels;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ViewModels.Authentication;
using SecureFolderFS.Uno.ViewModels;
using Windows.Security.Credentials;

namespace SecureFolderFS.Uno.Platforms.Windows.ServiceImplementation
{
    /// <inheritdoc cref="IVaultService"/>
    internal sealed class WindowsVaultCredentialsService : BaseVaultCredentialsService
    {
        /// <inheritdoc/>
        public override async IAsyncEnumerable<AuthenticationViewModel> GetLoginAsync(IFolder vaultFolder, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var vaultReader = new VaultReader(vaultFolder, StreamSerializer.Instance);
            var config = await vaultReader.ReadConfigurationAsync(cancellationToken);
            var authenticationMethod = AuthenticationMethod.FromString(config.AuthenticationMethod);

            foreach (var item in authenticationMethod.Methods)
            {
                yield return item switch
                {
                    // Password
                    Core.Constants.Vault.Authentication.AUTH_PASSWORD => new PasswordLoginViewModel() { Icon = new ImageGlyph("\uE8AC") },

                    // Windows Hello
                    Core.Constants.Vault.Authentication.AUTH_WINDOWS_HELLO => await KeyCredentialManager.IsSupportedAsync().AsTask(cancellationToken)
                            ? new WindowsHelloLoginViewModel(vaultFolder, config.Uid)
                            : throw new NotSupportedException($"The authentication method '{item}' is not supported by the platform."),

                    // Key File
                    Core.Constants.Vault.Authentication.AUTH_KEYFILE => new KeyFileLoginViewModel(vaultFolder) { Icon = new ImageGlyph("\uE8D7") },

                    _ => throw new NotSupportedException($"The authentication method '{item}' is not supported by the platform.")
                };
            }
        }

        /// <inheritdoc/>
        public override async IAsyncEnumerable<AuthenticationViewModel> GetCreationAsync(IFolder vaultFolder, string vaultId,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Password
            yield return new PasswordCreationViewModel() { Icon = new ImageGlyph("\uE8AC") };

            // Windows Hello
            if (await KeyCredentialManager.IsSupportedAsync().AsTask(cancellationToken))
                yield return new WindowsHelloCreationViewModel(vaultFolder, vaultId);

            // Key File
            yield return new KeyFileCreationViewModel(vaultId) { Icon = new ImageGlyph("\uE8D7") };
        }
    }
}
