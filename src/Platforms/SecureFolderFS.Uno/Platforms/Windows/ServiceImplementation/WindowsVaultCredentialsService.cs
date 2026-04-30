using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.Security.Credentials;
using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.AppModels;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ViewModels.Authentication;
using SecureFolderFS.Uno.ViewModels.DeviceLink;
using SecureFolderFS.Uno.ViewModels.WindowsHello;
using SecureFolderFS.Uno.ViewModels.YubiKey;

namespace SecureFolderFS.Uno.Platforms.Windows.ServiceImplementation
{
    /// <inheritdoc cref="IVaultCredentialsService"/>
    internal sealed class WindowsVaultCredentialsService : BaseVaultCredentialsService
    {
        /// <inheritdoc/>
        public override async IAsyncEnumerable<AuthenticationViewModel> GetCreationAsync(IFolder vaultFolder, string vaultId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Password
            yield return new PasswordCreationViewModel() { Icon = new ImageGlyph("\uE8AC") };

            // Windows Hello
            if (await KeyCredentialManager.IsSupportedAsync().AsTask(cancellationToken))
                yield return new WindowsHelloCreationViewModel(vaultFolder, vaultId);

            var iapService = DI.Service<IIapService>();
            if (await iapService.IsOwnedAsync(IapProductType.Any, cancellationToken))
            {
                // YubiKey
                if (YubiKeyViewModel.IsSupported())
                    yield return new YubiKeyCreationViewModel(vaultFolder, vaultId) { Icon = new ImageGlyph("\uEE7E") };
            }

            // Key File
            yield return new KeyFileCreationViewModel(vaultId) { Icon = new ImageGlyph("\uE8D7") };

            // Device Link
            yield return new DeviceLinkCreationViewModel(vaultFolder, vaultId) { Icon = new ImageGlyph("\uE8EA") };
        }

        /// <inheritdoc/>
        protected override async IAsyncEnumerable<AuthenticationViewModel> GetLoginAsync(
            IFolder vaultFolder,
            AuthenticationMethod unlockProcedure, string vaultId,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var item in unlockProcedure.Methods)
            {
                yield return item switch
                {
                    // Password
                    Constants.Vault.Authentication.AUTH_PASSWORD => new PasswordLoginViewModel() { Icon = new ImageGlyph("\uE8AC") },

                    // Windows Hello
                    Constants.Vault.Authentication.AUTH_WINDOWS_HELLO => await KeyCredentialManager.IsSupportedAsync().AsTask(cancellationToken)
                        ? new WindowsHelloLoginViewModel(vaultFolder, vaultId)
                        : throw new NotSupportedException($"The authentication method '{item}' is not supported by the platform."),

                    // YubiKey
                    Constants.Vault.Authentication.AUTH_YUBIKEY => new YubiKeyLoginViewModel(vaultFolder, vaultId) { Icon = new ImageGlyph("\uEE7E") },

                    // Key File
                    Constants.Vault.Authentication.AUTH_KEYFILE => new KeyFileLoginViewModel(vaultFolder) { Icon = new ImageGlyph("\uE8D7") },

                    // Device Link
                    Constants.Vault.Authentication.AUTH_DEVICE_LINK => new DeviceLinkLoginViewModel(vaultFolder, vaultId) { Icon = new ImageGlyph("\uE8EA") },

                    _ => throw new NotSupportedException($"The authentication method '{item}' is not supported by the platform.")
                };
            }
        }
    }
}
