using System.Runtime.CompilerServices;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Maui.Platforms.Android.ViewModels;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ViewModels.Authentication;

namespace SecureFolderFS.Maui.Platforms.Android.ServiceImplementation
{
    /// <inheritdoc cref="IVaultService"/>
    internal sealed class AndroidVaultCredentialsService : BaseVaultCredentialsService
    {
        /// <inheritdoc/>
        public override IEnumerable<string> GetContentCiphers()
        {
            // XChaCha20-Poly1305 is not supported in NSec implementation for Android
            // Trackers:
            // - https://github.com/ektrah/nsec/issues/81
            // - https://nsec.rocks/docs/install#supported-platforms

            yield return Constants.CipherId.AES_GCM;

#if DEBUG
            yield return Constants.CipherId.NONE;
#endif
        }

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
                    Core.Constants.Vault.Authentication.AUTH_ANDROID_BIOMETRIC => new AndroidBiometricLoginViewModel(vaultFolder, config.Uid),
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

            // Android Biometric
            yield return new AndroidBiometricCreationViewModel(vaultFolder, vaultId);

            // Key File
            yield return new KeyFileCreationViewModel(vaultId);

            await Task.CompletedTask;
        }
    }
}
