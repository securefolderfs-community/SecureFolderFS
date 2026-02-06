using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.AppModels;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ViewModels.Authentication;
using SecureFolderFS.Uno.ViewModels.DeviceLink;
using SecureFolderFS.Uno.ViewModels.YubiKey;

namespace SecureFolderFS.Uno.Platforms.Desktop.ServiceImplementation
{
    internal sealed class SkiaVaultCredentialsService : BaseVaultCredentialsService
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
                    Constants.Vault.Authentication.AUTH_PASSWORD => new PasswordLoginViewModel() { Icon = new ImageGlyph("\uE8AC") },
                    Constants.Vault.Authentication.AUTH_YUBIKEY => new YubiKeyLoginViewModel(vaultFolder, config.Uid) { Icon = new ImageGlyph("\uEE7E") },
                    Constants.Vault.Authentication.AUTH_KEYFILE => new KeyFileLoginViewModel(vaultFolder) { Icon = new ImageGlyph("\uE8D7") },
                    Constants.Vault.Authentication.AUTH_DEVICE_LINK => new DeviceLinkLoginViewModel(vaultFolder, config.Uid) { Icon = new ImageGlyph("\uE8EA") },
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
            
            // YubiKey
            if (YubiKeyViewModel.IsSupported())
                yield return new YubiKeyCreationViewModel(vaultFolder, vaultId) { Icon = new ImageGlyph("\uEE7E") };
            
            // Key File
            yield return new KeyFileCreationViewModel(vaultId) { Icon = new ImageGlyph("\uE8D7") };
            
            // Device Link
            yield return new DeviceLinkCreationViewModel(vaultFolder, vaultId) { Icon = new ImageGlyph("\uE8EA") };

            await Task.CompletedTask;
        }
    }
}
