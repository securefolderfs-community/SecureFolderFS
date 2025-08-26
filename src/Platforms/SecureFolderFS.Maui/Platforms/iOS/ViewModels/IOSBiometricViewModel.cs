using System.Security.Cryptography;
using Foundation;
using LocalAuthentication;
using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.ComponentModel;
using Security;

namespace SecureFolderFS.Maui.Platforms.iOS.ViewModels
{
    internal abstract class IOSBiometricViewModel : AuthenticationViewModel
    {
        private const string KEY_ALIAS_PREFIX = "securefolderfs_biometric_ios_";
        private const SecKeyAlgorithm ALGORITHM = SecKeyAlgorithm.EciesEncryptionStandardX963Sha256AesGcm;

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

        public IOSBiometricViewModel(IFolder vaultFolder, string vaultId)
            : base(Constants.Vault.Authentication.AUTH_APPLE_BIOMETRIC)
        {
            Title = "AppleBiometrics".ToLocalized();
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
        public override async Task<IKey> EnrollAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);
            
            var alias = $"{KEY_ALIAS_PREFIX}{id}";
            var privateKey = GetPrivateKey(alias);
            privateKey ??= CreatePrivateKey(alias);

            var publicKey = privateKey.GetPublicKey() ?? throw new CryptographicException("Public key could not be retrieved.");;
            return Encrypt(publicKey, data);
        }
        
        /// <inheritdoc/>
        public override async Task<IKey> AcquireAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);
            
            var alias = $"{KEY_ALIAS_PREFIX}{id}";
            var privateKey = GetPrivateKey(alias);
            if (privateKey is null)
                throw new CryptographicException("Private key could not be found.");
            
            return await DecryptAsync(privateKey, data);
        }

        private static SecretKey Encrypt(SecKey publicKey, byte[] data)
        {
            using var plaintext = NSData.FromArray(data);
            using var ciphertext = publicKey.CreateEncryptedData(ALGORITHM, plaintext, out var error);
            
            if (ciphertext is null || error is not null)
                throw new CryptographicException($"Could not encrypt the data. {error?.LocalizedDescription}");
            
            var ciphertextBuffer = ciphertext.ToArray();
            return SecureKey.TakeOwnership(ciphertextBuffer);
        }

        private static async Task<SecretKey> DecryptAsync(SecKey privateKey, byte[] data)
        {
            var context = new LAContext();
            var policy = LAPolicy.DeviceOwnerAuthenticationWithBiometrics;
            
            var (ok, evalErr) = await EvaluateAsync(context, policy, "AuthenticateForCredentials".ToLocalized());
            if (!ok)
                throw new CryptographicException(evalErr?.LocalizedDescription ?? "Authentication failed.");
            
            var ciphertext = NSData.FromArray(data);
            var plaintext = privateKey.CreateDecryptedData(ALGORITHM, ciphertext, out var error);
            if (plaintext is null || error is not null)
                throw new CryptographicException($"Could not decrypt the data. {error?.LocalizedDescription}");
            
            var plaintextBuffer = plaintext.ToArray();
            return SecureKey.TakeOwnership(plaintextBuffer);
        }
        
        private static Task<(bool ok, NSError? error)> EvaluateAsync(LAContext context, LAPolicy policy, string reason)
        {
            var tcs = new TaskCompletionSource<(bool, NSError?)>();
            if (!context.CanEvaluatePolicy(policy, out var canErr))
            {
                tcs.SetResult((false, canErr));
                return tcs.Task;
            }
            
            context.EvaluatePolicy(policy, reason, (success, evalErr) =>
            {
                tcs.TrySetResult((success, evalErr));
            });

            return tcs.Task;
        }

        private static SecKey CreatePrivateKey(string alias)
        {
            var access = new SecAccessControl(SecAccessible.WhenUnlockedThisDeviceOnly, SecAccessControlCreateFlags.UserPresence);
            var applicationTag = NSData.FromString(alias, NSStringEncoding.UTF8);
            var genParams = new SecKeyGenerationParameters()
            {
                KeyType = SecKeyType.ECSecPrimeRandom,
                KeySizeInBits = 256,
                TokenID = SecTokenID.SecureEnclave,
                IsPermanent = true,
                AccessControl = access,
                ApplicationTag = applicationTag
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
