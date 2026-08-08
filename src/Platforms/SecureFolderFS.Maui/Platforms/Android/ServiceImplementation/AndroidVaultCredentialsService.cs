using System.Runtime.CompilerServices;
using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Maui.Platforms.Android.ViewModels;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ViewModels.Authentication;

namespace SecureFolderFS.Maui.Platforms.Android.ServiceImplementation
{
    /// <inheritdoc cref="IVaultCredentialsService"/>
    internal sealed class AndroidVaultCredentialsService : BaseVaultCredentialsService
    {
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

        /// <inheritdoc/>
        protected override async IAsyncEnumerable<AuthenticationViewModel> GetLoginAsync(
            IFolder vaultFolder,
            AuthenticationMethod unlockProcedure,
            string vaultId,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var item in EnumerateLoginMethods(unlockProcedure))
            {
                yield return item switch
                {
                    // Password
                    Constants.Vault.Authentication.AUTH_PASSWORD => new PasswordLoginViewModel(),
                    
                    // Key File
                    Constants.Vault.Authentication.AUTH_KEYFILE => new KeyFileLoginViewModel(vaultFolder),
                    
                    // Android Biometrics
                    Constants.Vault.Authentication.AUTH_ANDROID_BIOMETRIC => new AndroidBiometricLoginViewModel(vaultFolder, vaultId),
                    
                    // App Platform
                    Constants.Vault.Authentication.AUTH_APP_PLATFORM => new AppPlatformLoginViewModel(vaultFolder),
                    
                    _ => throw new NotSupportedException($"The authentication method '{item}' is not supported by the platform.")
                };
            }
        }
    }
}
