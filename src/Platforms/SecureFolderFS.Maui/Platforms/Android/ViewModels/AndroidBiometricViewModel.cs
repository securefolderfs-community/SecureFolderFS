using System.ComponentModel;
using System.Security.Cryptography;
using Android.Hardware.Biometrics;
using Android.OS;
using AndroidX.Core.Content;
using Java.Security;
using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Maui.AppModels;
using SecureFolderFS.Maui.Platforms.Android.Helpers;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Shared.SecureStore;
using SecureFolderFS.Storage.Extensions;

namespace SecureFolderFS.Maui.Platforms.Android.ViewModels
{
    [Bindable(true)]
    public abstract class AndroidBiometricViewModel : AuthenticationViewModel
    {
        private const string KEY_ALIAS_PREFIX = "securefolderfs_biometric_";
        private const string KEYSTORE_PROVIDER = "AndroidKeyStore";

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

        public AndroidBiometricViewModel(IFolder vaultFolder, string vaultId)
            : base(Constants.Vault.Authentication.AUTH_ANDROID_BIOMETRIC)
        {
            Title = "AndroidBiometrics".ToLocalized();
            VaultFolder = vaultFolder;
            VaultId = vaultId;
        }

        /// <inheritdoc/>
        public override async Task RevokeAsync(string? id, CancellationToken cancellationToken = default)
        {
            id ??= VaultId;
            var keyStore = KeyStore.GetInstance(KEYSTORE_PROVIDER);
            if (keyStore is not null)
            {
                keyStore.Load(null);
                var alias = $"{KEY_ALIAS_PREFIX}{id}";
                if (keyStore.ContainsAlias(alias))
                    keyStore.DeleteEntry(alias);
            }

            if (VaultFolder is not IModifiableFolder modifiableFolder)
                return;

            var authenticationFile = await modifiableFolder.TryGetFileByNameAsync($"{Id}{Constants.Vault.Names.CONFIGURATION_EXTENSION}", cancellationToken);
            if (authenticationFile is not null)
                await modifiableFolder.DeleteAsync(authenticationFile, cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task<IResult<IKeyBytes>> EnrollAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);
            var keyStore = KeyStore.GetInstance(KEYSTORE_PROVIDER);
            if (keyStore is null)
                throw new CryptographicException("Android KeyStore could not be loaded.");

            // Load the KeyStore with default parameters
            keyStore.Load(null);

            IPrivateKey privateKey;
            var alias = $"{KEY_ALIAS_PREFIX}{id}";
            if (!keyStore.ContainsAlias(alias))
            {
                var keyPairGenerator = AndroidBiometricHelpers.GetKeyPairGenerator(alias, KEYSTORE_PROVIDER);
                var keyPair = keyPairGenerator?.GenerateKeyPair();
                privateKey = keyPair?.Private ?? throw new CryptographicException("KeyPair could not be generated.");
            }
            else
            {
                var privateKeyEntry = keyStore.GetEntry(alias, null) as KeyStore.PrivateKeyEntry;
                privateKey = privateKeyEntry?.PrivateKey ?? throw new CryptographicException("Private key could not be found.");
            }

            var signature = await MakeSignatureAsync(privateKey, data, cancellationToken);
            return Result<IKeyBytes>.Success(signature);
        }

        /// <inheritdoc/>
        public override async Task<IResult<IKeyBytes>> AcquireAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);
            var keyStore = KeyStore.GetInstance(KEYSTORE_PROVIDER);
            if (keyStore is null)
                throw new CryptographicException("Android KeyStore could not be loaded.");

            // Load the KeyStore with default parameters
            keyStore.Load(null);

            var alias = $"{KEY_ALIAS_PREFIX}{id}";
            var privateKeyEntry = keyStore.GetEntry(alias, null) as KeyStore.PrivateKeyEntry;
            var privateKey = privateKeyEntry?.PrivateKey ?? throw new CryptographicException("Private key could not be found.");

            var signature = await MakeSignatureAsync(privateKey, data, cancellationToken);
            return Result<IKeyBytes>.Success(signature);
        }

        private static async Task<IKeyBytes> MakeSignatureAsync(IPrivateKey privateKey, byte[] data, CancellationToken cancellationToken)
        {
            var signature = Signature.GetInstance("SHA256withRSA");
            if (signature is null)
                throw new CryptographicException("Signature could not be loaded.");

            // Init signature
            signature.InitSign(privateKey);

            var tcs = new TaskCompletionSource<IKeyBytes>();
            var executor = ContextCompat.GetMainExecutor(MainActivity.Instance!);
            var promptInfo = new BiometricPrompt.Builder(MainActivity.Instance!)
                .SetTitle("Authenticate".ToLocalized())
                .SetSubtitle("AuthenticateForCredentials".ToLocalized())
                .SetNegativeButton("Cancel", executor, new DialogOnClickListener((_, _) =>
                {
                    tcs.TrySetCanceled();
                }))
                .Build();

            // Dismiss the prompt when the caller cancels
            var cancellationSignal = new CancellationSignal();
            using var cancellationRegistration = cancellationToken.Register(() => cancellationSignal.Cancel());

            promptInfo.Authenticate(new BiometricPrompt.CryptoObject(signature), cancellationSignal, executor,
                new BiometricPromptCallback(
                    onSuccess: result =>
                    {
                        try
                        {
                            if (result?.CryptoObject?.Signature is null)
                                throw new CryptographicException("The authenticated signature was not available.");

                            var signedBytes = AndroidBiometricHelpers.SignData(result.CryptoObject.Signature, data);
                            if (signedBytes is null)
                                throw new CryptographicException("Could not sign the data.");

                            tcs.TrySetResult(ManagedKey.TakeOwnership(signedBytes));
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                        }
                    },
                    onError: (code, message) =>
                    {
                        // Every terminal prompt error must complete the task, otherwise the caller awaits forever
                        if (code is BiometricErrorCode.UserCanceled or BiometricErrorCode.Canceled)
                            tcs.TrySetCanceled();
                        else
                            tcs.TrySetException(new CryptographicException($"Biometric authentication failed ({code}). {message}"));
                    },
                    onFailure: null));

            return await tcs.Task;
        }
    }
}
