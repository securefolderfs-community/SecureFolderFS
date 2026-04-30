using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.AppModels;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ViewModels.Authentication;
using SecureFolderFS.Uno.Platforms.Desktop.ViewModels;
using SecureFolderFS.Uno.ViewModels.DeviceLink;
using SecureFolderFS.Uno.ViewModels.YubiKey;

namespace SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation
{
    /// <inheritdoc cref="IVaultCredentialsService"/>
    internal sealed class SkiaVaultCredentialsService : BaseVaultCredentialsService
    {
        /// <inheritdoc/>
        public override async IAsyncEnumerable<AuthenticationViewModel> GetCreationAsync(IFolder vaultFolder, string vaultId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Password
            yield return new PasswordCreationViewModel() { Icon = new ImageGlyph("\uE8AC") };

            var iapService = DI.Service<IIapService>();
            if (await iapService.IsOwnedAsync(IapProductType.Any, cancellationToken))
            {
                // YubiKey
                if (YubiKeyViewModel.IsSupported())
                    yield return new YubiKeyCreationViewModel(vaultFolder, vaultId) { Icon = new ImageGlyph("\uEE7E") };
            }

            // macOS Biometrics (Touch ID)
            if (MacOSBiometricsViewModel.IsSupported())
                yield return new MacOSBiometricsCreationViewModel(vaultFolder, vaultId);

            // Key File
            yield return new KeyFileCreationViewModel(vaultId) { Icon = new ImageGlyph("\uE8D7") };

            // Device Link
            yield return new DeviceLinkCreationViewModel(vaultFolder, vaultId) { Icon = new ImageGlyph("\uE8EA") };

            await Task.CompletedTask;
        }
        
        /// <inheritdoc/>
        protected override async IAsyncEnumerable<AuthenticationViewModel> GetLoginAsync(
            IFolder vaultFolder,
            AuthenticationMethod unlockProcedure,
            string vaultId,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            foreach (var item in unlockProcedure.Methods)
            {
                yield return item switch
                {
                    Constants.Vault.Authentication.AUTH_PASSWORD => new PasswordLoginViewModel() { Icon = new ImageGlyph("\uE8AC") },
                    Constants.Vault.Authentication.AUTH_YUBIKEY => new YubiKeyLoginViewModel(vaultFolder, vaultId) { Icon = new ImageGlyph("\uEE7E") },
                    Constants.Vault.Authentication.AUTH_KEYFILE => new KeyFileLoginViewModel(vaultFolder) { Icon = new ImageGlyph("\uE8D7") },
                    Constants.Vault.Authentication.AUTH_APPLE_MACOS => new MacOSBiometricsLoginViewModel(vaultFolder, vaultId),
                    Constants.Vault.Authentication.AUTH_DEVICE_LINK => new DeviceLinkLoginViewModel(vaultFolder, vaultId).WithInitAsync(cancellationToken),
                    _ => throw new NotSupportedException($"The authentication method '{item}' is not supported by the platform.")
                };
            }
        }
    }
}
