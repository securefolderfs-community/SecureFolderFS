using System.ComponentModel;
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
    [Bindable(true)]
    internal abstract class IOSSecureEnclaveViewModel : AuthenticationViewModel
    {
        private const string KEY_ALIAS_PREFIX = "securefolderfs_biometric_ios_";

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

        public IOSSecureEnclaveViewModel(IFolder vaultFolder, string vaultId)
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
                KeyType = SecKeyType.ECSecPrimeRandom
            };
            
            // Remove the key from the keychain
            _ = SecKeyChain.Remove(query);
            return Task.CompletedTask;
        }
        
        /// <inheritdoc/>
        public override async Task<IKey> CreateAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);
            
            var alias = $"{KEY_ALIAS_PREFIX}{id}";
            var privateKey = GetOrCreateKey(alias);
            return await SignWithBiometricsAsync(privateKey, data);
        }

        /// <inheritdoc/>
        public override async Task<IKey> SignAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);

            var alias = $"{KEY_ALIAS_PREFIX}{id}";
            var privateKey = GetExistingKey(alias) ?? throw new CryptographicException("Private key could not be found.");
            return await SignWithBiometricsAsync(privateKey, data);
        }
        
        private static SecKey? GetExistingKey(string alias)
        {
            var applicationTag = NSData.FromString(alias, NSStringEncoding.UTF8);
            var query = new SecRecord(SecKind.Key)
            {
                ApplicationTag = applicationTag,
                KeyClass = SecKeyClass.Private,
                KeyType = SecKeyType.ECSecPrimeRandom
            };

            // Query the keychain for a SecKey.
            var key = SecKeyChain.QueryAsConcreteType(query, out var result) as SecKey;
            
            // Return the SecKey based on the success status
            return result == SecStatusCode.Success ? key : null;
        }

        private static SecKey GetOrCreateKey(string alias)
        {
            var existing = GetExistingKey(alias);
            if (existing is not null)
                return existing;

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
                    AccessControl = access,
                    ApplicationTag = applicationTag
                }
            };

            var key = SecKey.CreateRandomKey(genParams, out var error);
            if (key is null || error is not null)
                throw new CryptographicException($"KeyPair could not be generated. {error?.LocalizedDescription}");

            return key;
        }

        private static async Task<IKey> SignWithBiometricsAsync(SecKey privateKey, byte[] data)
        {
            var context = new LAContext();
            var (ok, evalErr) = await EvaluateAsync(context, "AuthenticateForCredentials".ToLocalized());
            if (!ok)
                throw new CryptographicException(evalErr?.LocalizedDescription ?? "Authentication failed.");
            
            var algorithm = SecKeyAlgorithm.EcdsaSignatureMessageX962Sha256;
            if (!privateKey.IsAlgorithmSupported(SecKeyOperationType.Sign, algorithm))
                throw new CryptographicException("The Secure Enclave key does not support ECDSA P-256 with SHA-256.");
            
            using var nsData = NSData.FromArray(data);
            using var signature = privateKey.CreateSignature(algorithm, nsData, out var err);
            if (signature is null || err is not null)
                throw new CryptographicException($"Could not sign the data. {err?.LocalizedDescription}");
            
            var signedDer = signature.ToArray();
            var signedRaw = DerEcdsaToP256(signedDer);
            return SecureKey.TakeOwnership(signedRaw);
        }
        
        private static Task<(bool ok, NSError? error)> EvaluateAsync(LAContext context, string reason)
        {
            var tcs = new TaskCompletionSource<(bool, NSError?)>();
            if (!context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out var canErr))
            {
                tcs.SetResult((false, canErr));
                return tcs.Task;
            }

            context.EvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, reason, (success, evalErr) =>
            {
                tcs.TrySetResult((success, evalErr));
            });

            return tcs.Task;
        }

        /// <summary>
        /// Converts a DER-encoded ECDSA signature (ASN.1 SEQUENCE of INTEGER r,s)
        /// into a fixed-length 64-byte raw concatenation r||s (each 32 bytes, big-endian).
        /// Minimal, defensive parser sufficient for typical ECDSA outputs from Secure Enclave.
        /// </summary>
        private static byte[] DerEcdsaToP256(byte[] der)
        {
            if (der == null || der.Length < 8 || der[0] != 0x30)
                throw new CryptographicException("Invalid ECDSA DER signature.");

            // Handle long-form length
            var idx = 2;
            if ((der[1] & 0x80) != 0)
            {
                var lengthBytes = der[1] & 0x7F;
                if (lengthBytes is < 1 or > 2)
                    throw new CryptographicException("Unsupported DER length.");
                
                idx = 2 + lengthBytes;
            }

            if (der[idx] != 0x02)
                throw new CryptographicException("Invalid ECDSA DER: missing INTEGER r.");
            
            idx++;
            int lenR = der[idx++];
            var rBytes = new byte[lenR];
            Array.Copy(der, idx, rBytes, 0, lenR);
            idx += lenR;

            if (der[idx] != 0x02)
                throw new CryptographicException("Invalid ECDSA DER: missing INTEGER s.");
            
            idx++;
            int lenS = der[idx++];
            var sBytes = new byte[lenS];
            Array.Copy(der, idx, sBytes, 0, lenS);

            // r and s may have a leading 0x00 for sign - strip then left-pad to 32 bytes
            var r = StripLeftZeros(rBytes);
            var s = StripLeftZeros(sBytes);

            if (r.Length > 32 || s.Length > 32)
                throw new CryptographicException("ECDSA r or s is longer than 32 bytes.");

            var raw = new byte[64];
            Buffer.BlockCopy(r, 0, raw, 32 - r.Length, r.Length);
            Buffer.BlockCopy(s, 0, raw, 64 - s.Length, s.Length);
            
            return raw;
        }

        private static byte[] StripLeftZeros(byte[] input)
        {
            var i = 0;
            while (i < input.Length - 1 && input[i] == 0x00)
                i++;
            
            if (i == 0)
                return input;
            
            var res = new byte[input.Length - i];
            Buffer.BlockCopy(input, i, res, 0, res.Length);
            
            return res;
        }
    }
}
