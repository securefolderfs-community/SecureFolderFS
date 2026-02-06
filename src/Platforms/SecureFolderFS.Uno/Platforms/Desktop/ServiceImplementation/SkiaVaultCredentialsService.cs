using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ViewModels.Authentication;
using SecureFolderFS.Uno.ViewModels;
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
                    Core.Constants.Vault.Authentication.AUTH_PASSWORD => new PasswordLoginViewModel(),
                    Core.Constants.Vault.Authentication.AUTH_KEYFILE => new KeyFileLoginViewModel(vaultFolder),
                    Core.Constants.Vault.Authentication.AUTH_YUBIKEY => new YubiKeyLoginViewModel(vaultFolder, config.Uid),
                    Core.Constants.Vault.Authentication.AUTH_DEVICE_LINK => new DeviceLinkLoginViewModel(vaultFolder, config.Uid),
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

            // YubiKey
            if (YubiKeyViewModel.IsSupported())
                yield return new YubiKeyCreationViewModel(vaultFolder, vaultId);
            
            // Device Link
            yield return new DeviceLinkCreationViewModel(vaultFolder, vaultId);

            await Task.CompletedTask;
        }
    }
}
