using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
#if __UNO_SKIA_MACOS__
using SecureFolderFS.Uno.PInvoke;
#endif

namespace SecureFolderFS.Uno.Platforms.Desktop.ViewModels
{
    [Bindable(true)]
    public abstract partial class MacOSBiometricsViewModel : AuthenticationViewModel
    {
        private const string KEY_ALIAS_PREFIX = "securefolderfs_biometric_macos_";

        [ObservableProperty] private bool _IsAuthenticated;

        /// <summary>
        /// Gets the unique ID of the vault.
        /// </summary>
        protected string VaultId { get; }

        /// <summary>
        /// Gets the associated folder of the vault.
        /// </summary>
        protected IFolder VaultFolder { get; }

        /// <inheritdoc/>
        public sealed override bool CanComplement { get; } = true;

        /// <inheritdoc/>
        public sealed override AuthenticationStage Availability { get; } = AuthenticationStage.Any;

        protected MacOSBiometricsViewModel(IFolder vaultFolder, string vaultId)
            : base(Constants.Vault.Authentication.AUTH_APPLE_MACOS)
        {
            Title = "MacOsBiometrics".ToLocalized();
            Description = "AuthenticateUsing".ToLocalized(Title);
            VaultFolder = vaultFolder;
            VaultId = vaultId;
        }

        /// <summary>
        /// Checks whether biometric authentication is available on this Mac.
        /// </summary>
        public static bool IsSupported()
        {
#if __UNO_SKIA_MACOS__
            return UnsafeNative.Biometrics.CanEvaluateBiometricPolicy();
#else
            return false;
#endif
        }

        /// <inheritdoc/>
        public override Task RevokeAsync(string? id, CancellationToken cancellationToken = default)
        {
#if __UNO_SKIA_MACOS__
            id ??= VaultId;
            var alias = $"{KEY_ALIAS_PREFIX}{id}";
            UnsafeNative.Biometrics.DeleteKey(alias);
#endif
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override async Task<IResult<IKeyBytes>> EnrollAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
#if __UNO_SKIA_MACOS__
            ArgumentNullException.ThrowIfNull(data);

            var alias = $"{KEY_ALIAS_PREFIX}{id}";

            // Authenticate the user with biometrics first
            var authenticated = await UnsafeNative.Biometrics.EvaluateBiometricPolicyAsync("AuthenticateForCredentials".ToLocalized());
            if (!authenticated)
                throw new CryptographicException("Biometric authentication failed.");

            // Get or create the Secure Enclave key pair
            var privateKey = UnsafeNative.Biometrics.GetPrivateKey(alias);
            if (privateKey == IntPtr.Zero)
                privateKey = UnsafeNative.Biometrics.CreateSecureEnclaveKey(alias);

            // Encrypt the key material with the public key
            var encrypted = UnsafeNative.Biometrics.Encrypt(privateKey, data);
            return Result<IKeyBytes>.Success(ManagedKey.TakeOwnership(encrypted));
#else
            throw new PlatformNotSupportedException("macOS biometrics are only supported on macOS.");
#endif
        }

        /// <inheritdoc/>
        public override Task<IResult<IKeyBytes>> AcquireAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
#if __UNO_SKIA_MACOS__
            ArgumentNullException.ThrowIfNull(data);

            var alias = $"{KEY_ALIAS_PREFIX}{id}";
            var privateKey = UnsafeNative.Biometrics.GetPrivateKey(alias);
            if (privateKey == IntPtr.Zero)
                throw new CryptographicException("Private key could not be found.");

            // Decryption with Secure Enclave key will trigger the biometric prompt automatically
            var decrypted = UnsafeNative.Biometrics.Decrypt(privateKey, data);
            return Task.FromResult<IResult<IKeyBytes>>(Result<IKeyBytes>.Success(ManagedKey.TakeOwnership(decrypted)));
#else
            throw new PlatformNotSupportedException("macOS biometrics are only supported on macOS.");
#endif
        }
    }
}
