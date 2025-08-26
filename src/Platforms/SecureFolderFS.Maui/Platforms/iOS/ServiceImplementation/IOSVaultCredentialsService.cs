using System.Runtime.CompilerServices;
using Foundation;
using LocalAuthentication;
using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Maui.Platforms.iOS.ViewModels;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.UI.ServiceImplementation;
using SecureFolderFS.UI.ViewModels.Authentication;
using UIKit;

namespace SecureFolderFS.Maui.Platforms.iOS.ServiceImplementation
{
    /// <inheritdoc cref="IVaultCredentialsService"/>
    internal sealed class IOSVaultCredentialsService : BaseVaultCredentialsService
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
                    Constants.Vault.Authentication.AUTH_PASSWORD => new PasswordLoginViewModel(),
                    Constants.Vault.Authentication.AUTH_KEYFILE => new KeyFileLoginViewModel(vaultFolder),
                    Constants.Vault.Authentication.AUTH_APPLE_BIOMETRIC when AreBiometricsAvailable(out var biometryType) => 
                        new IOSBiometricLoginViewModel(vaultFolder, config.Uid, biometryType switch
                        {
                            LABiometryType.FaceId => "AuthenticateWithFaceID".ToLocalized(),
                            LABiometryType.TouchId => "AuthenticateWithTouchID".ToLocalized(),
                            _ => "AuthenticateWithBiometrics".ToLocalized()
                        }),
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
            
            // Face ID, Touch ID
            if (AreBiometricsAvailable(out var biometryType))
                yield return new IOSBiometricCreationViewModel(vaultFolder, vaultId, biometryType switch
                {
                    LABiometryType.FaceId => "AuthenticateWithFaceID".ToLocalized(),
                    LABiometryType.TouchId => "AuthenticateWithTouchID".ToLocalized(),
                    _ => "AuthenticateWithBiometrics".ToLocalized()
                });

            await Task.CompletedTask;
        }
        
        private static bool AreBiometricsAvailable(out LABiometryType biometryType)
        {
            using var context = new LAContext();
            var result = context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out _);
            biometryType = context.BiometryType; 
            
            return result;
        }
    }
}
