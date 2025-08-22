using System.ComponentModel;
using System.Security.Cryptography;
using Android.Hardware.Biometrics;
using AndroidX.Core.Content;
using Java.Security;
using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Maui.AppModels;
using SecureFolderFS.Maui.Platforms.Android.Helpers;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using IKey = SecureFolderFS.Shared.ComponentModel.IKey;

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
        public override Task RevokeAsync(string? id, CancellationToken cancellationToken = default)
        {
            var keyStore = KeyStore.GetInstance(KEYSTORE_PROVIDER);
            if (keyStore is null)
                return Task.CompletedTask;

            keyStore.Load(null);
            var alias = $"{KEY_ALIAS_PREFIX}{id}";
            if (keyStore.ContainsAlias(alias))
                keyStore.DeleteEntry(alias);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override async Task<IKey> CreateAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
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

            return await MakeSignatureAsync(privateKey, data);
        }

        /// <inheritdoc/>
        public override async Task<IKey> SignAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
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

            return await MakeSignatureAsync(privateKey, data);
        }

        private static async Task<IKey> MakeSignatureAsync(IPrivateKey privateKey, byte[] data)
        {
            var signature = Signature.GetInstance("SHA256withRSA");
            if (signature is null)
                throw new CryptographicException("Signature could not be loaded.");

            // Init signature
            signature.InitSign(privateKey);

            var tcs = new TaskCompletionSource<IKey>();
            var executor = ContextCompat.GetMainExecutor(MainActivity.Instance!);
            var promptInfo = new BiometricPrompt.Builder(MainActivity.Instance!)
                .SetTitle("Authenticate".ToLocalized())
                .SetSubtitle("AuthenticateForCredentials".ToLocalized())
                .SetNegativeButton("Cancel", executor, new DialogOnClickListener((_, _) =>
                {
                    tcs.TrySetCanceled();
                }))
                .Build();

            promptInfo.Authenticate(new BiometricPrompt.CryptoObject(signature), new(), executor,
                new BiometricPromptCallback(
                    onSuccess: result =>
                    {
                        try
                        {
                            if (result?.CryptoObject?.Signature is null)
                                return;

                            var signedBytes = AndroidBiometricHelpers.SignData(result.CryptoObject.Signature, data);
                            if (signedBytes is null)
                                throw new CryptographicException("Could not sign the data.");

                            tcs.TrySetResult(SecureKey.TakeOwnership(signedBytes));
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                        }
                    },
                    onError: (code, message) =>
                    {
                        _ = message;
                        //tcs.TrySetException(new Exception($"Biometric error {code}: {message}"));
                    },
                    onFailure: null));

            return await tcs.Task;
        }
    }
}
