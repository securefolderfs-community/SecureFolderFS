using System.ComponentModel;
using System.Security.Cryptography;
using CommunityToolkit.Mvvm.ComponentModel;
using Foundation;
using LocalAuthentication;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using Security;

namespace SecureFolderFS.Maui.Platforms.iOS.ViewModels
{
    [Bindable(true)]
    internal abstract partial class IOSBiometricViewModel : AuthenticationViewModel
    {
        private const string KEY_ALIAS_PREFIX = "securefolderfs_biometric_ios_";
        private const SecKeyAlgorithm ALGORITHM = SecKeyAlgorithm.EciesEncryptionStandardX963Sha256AesGcm;

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

        public IOSBiometricViewModel(IFolder vaultFolder, string vaultId, string title)
            : base(Core.Constants.Vault.Authentication.AUTH_APPLE_BIOMETRIC)
        {
            Title = title;
            Description = "AuthenticateUsing".ToLocalized(Title);
            VaultFolder = vaultFolder;
            VaultId = vaultId;
        }

        /// <inheritdoc/>
        public override Task RevokeAsync(string? id, CancellationToken cancellationToken = default)
        {
            id ??= VaultId;

            var alias = $"{KEY_ALIAS_PREFIX}{id}";
            var applicationTag = NSData.FromString(alias, NSStringEncoding.UTF8);
            var query = new SecRecord(SecKind.Key)
            {
                ApplicationTag = applicationTag,
                KeyClass = SecKeyClass.Private,
                TokenID = SecTokenID.SecureEnclave 
            };

            // Remove the key from the keychain
            var status = SecKeyChain.Remove(query);
            _ = status;

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override async Task<IResult<IKeyBytes>> EnrollAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);

            var alias = $"{KEY_ALIAS_PREFIX}{id}";
            var privateKey = GetPrivateKey(alias);
            var context = new LAContext();
            var (ok, evalErr) = await EvaluateAsync(context, LAPolicy.DeviceOwnerAuthenticationWithBiometrics, "AuthenticateForCredentials".ToLocalized());
            if (!ok)
                throw new CryptographicException(evalErr?.LocalizedDescription ?? "Authentication failed.");

            privateKey ??= CreatePrivateKey(alias);
            var publicKey = privateKey.GetPublicKey() ?? throw new CryptographicException("Public key could not be retrieved.");;
            var encrypted = Encrypt(publicKey, data);

            return Result<IKeyBytes>.Success(encrypted);
        }
        
        /// <inheritdoc/>
        public override async Task<IResult<IKeyBytes>> AcquireAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);

            var alias = $"{KEY_ALIAS_PREFIX}{id}";
            var privateKey = GetPrivateKey(alias);
            if (privateKey is null)
                throw new CryptographicException("Private key could not be found.");

            var decrypted = await DecryptAsync(privateKey, data);
            return Result<IKeyBytes>.Success(decrypted);
        }

        private static ManagedKey Encrypt(SecKey publicKey, byte[] data)
        {
            using var plaintext = NSData.FromArray(data);
            using var ciphertext = publicKey.CreateEncryptedData(ALGORITHM, plaintext, out var error);

            if (ciphertext is null || error is not null)
                throw new CryptographicException($"Could not encrypt the data. {error?.LocalizedDescription}");

            var ciphertextBuffer = ciphertext.ToArray();
            return ManagedKey.TakeOwnership(ciphertextBuffer);
        }

        private static async Task<ManagedKey> DecryptAsync(SecKey privateKey, byte[] data)
        {
            var ciphertext = NSData.FromArray(data);
            var plaintext = privateKey.CreateDecryptedData(ALGORITHM, ciphertext, out var error);
            if (plaintext is null || error is not null)
                throw new CryptographicException($"Could not decrypt the data. {error?.LocalizedDescription}");

            var plaintextBuffer = plaintext.ToArray();
            return ManagedKey.TakeOwnership(plaintextBuffer);
        }
        
        private static async Task<(bool ok, NSError? error)> EvaluateAsync(LAContext context, LAPolicy policy, string reason)
        {
            if (!context.CanEvaluatePolicy(policy, out var canErr))
                return (false, canErr);

            var tcs = new TaskCompletionSource<(bool, NSError?)>();
            context.EvaluatePolicy(policy, reason, (success, evalErr) =>
            {
                tcs.TrySetResult((success, evalErr));
            });

            return await tcs.Task;
        }

        private static SecKey CreatePrivateKey(string alias)
        {
            var access = new SecAccessControl(SecAccessible.WhenUnlockedThisDeviceOnly, SecAccessControlCreateFlags.PrivateKeyUsage | SecAccessControlCreateFlags.UserPresence);
            var applicationTag = NSData.FromString(alias, NSStringEncoding.UTF8);
            var genParams = new SecKeyGenerationParameters()
            {
                KeyType = SecKeyType.ECSecPrimeRandom,
                KeySizeInBits = 256,
                TokenID = SecTokenID.SecureEnclave,
                PrivateKeyAttrs = new SecKeyParameters()
                {
                    IsPermanent = true,
                    ApplicationTag = applicationTag,
                    AccessControl = access
                }
            };

            var key = SecKey.CreateRandomKey(genParams, out var error);
            if (key is null || error is not null)
                throw new CryptographicException($"KeyPair could not be generated. {error?.LocalizedDescription}");

            return key;
        }
        
        private static SecKey? GetPrivateKey(string alias)
        {
            var applicationTag = NSData.FromString(alias, NSStringEncoding.UTF8);
            var query = new SecRecord(SecKind.Key)
            {
                ApplicationTag = applicationTag,
                KeyClass = SecKeyClass.Private,
                TokenID = SecTokenID.SecureEnclave
            };

            // Query the keychain for a SecKey.
            var key = SecKeyChain.QueryAsConcreteType(query, out var result) as SecKey;

            // Return the SecKey based on the success status
            return result == SecStatusCode.Success ? key : null;
        }
    }
}
