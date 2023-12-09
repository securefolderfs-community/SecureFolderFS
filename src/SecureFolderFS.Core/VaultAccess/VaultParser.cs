using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.Helpers;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace SecureFolderFS.Core.VaultAccess
{
    internal static class VaultParser
    {
        /// <summary>
        /// Computes a unique HMAC thumbprint of <paramref name="configDataModel"/> properties.
        /// </summary>
        /// <param name="configDataModel">The <see cref="VaultConfigurationDataModel"/> to compute the thumbprint for.</param>
        /// <param name="macKey">The key part of HMAC.</param>
        /// <param name="mac">The destination to fill the calculated HMAC thumbprint into.</param>
        public static void CalculateConfigMac(VaultConfigurationDataModel configDataModel, SecretKey macKey, Span<byte> mac)
        {
            // Initialize HMAC
            using var hmacSha256 = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, macKey);

            // Update HMAC
            hmacSha256.AppendData(BitConverter.GetBytes(Constants.Vault.Versions.LATEST_VERSION));                                // Version
            hmacSha256.AppendData(BitConverter.GetBytes(CryptHelpers.ContentCipherId(configDataModel.ContentCipherId)));        // ContentCipherScheme
            hmacSha256.AppendData(BitConverter.GetBytes(CryptHelpers.FileNameCipherId(configDataModel.FileNameCipherId)));      // FileNameCipherScheme
            hmacSha256.AppendData(Encoding.UTF8.GetBytes(configDataModel.Id));                                                  // Id
            hmacSha256.AppendData(Encoding.UTF8.GetBytes(configDataModel.AuthenticationMethod));                                          // AuthMethod

            // Fill the hash to payload
            hmacSha256.GetCurrentHash(mac);
        }

        /// <summary>
        /// Derives DEK and MAC keys from provided credentials.
        /// </summary>
        /// <param name="passkey">The passkey credential that combines password and 'magic'.</param>
        /// <param name="keystoreDataModel">The keystore that holds wrapped keys.</param>
        /// <returns>A tuple containing the DEK and MAC keys respectively.</returns>
        [SkipLocalsInit]
        public static (SecretKey encKey, SecretKey macKey) DeriveKeystore(SecretKey passkey, VaultKeystoreDataModel keystoreDataModel)
        {
            var encKey = new SecureKey(Cryptography.Constants.KeyChains.ENCKEY_LENGTH);
            var macKey = new SecureKey(Cryptography.Constants.KeyChains.MACKEY_LENGTH);

            // Derive KEK
            Span<byte> kek = stackalloc byte[Cryptography.Constants.ARGON2_KEK_LENGTH];
            Argon2id.DeriveKey(passkey.Key, keystoreDataModel.Salt, kek);

            // Unwrap keys
            Rfc3394KeyWrap.UnwrapKey(keystoreDataModel.WrappedEncKey, kek, encKey.Key);
            Rfc3394KeyWrap.UnwrapKey(keystoreDataModel.WrappedMacKey, kek, macKey.Key);

            return (encKey, macKey);
        }

        /// <summary>
        /// Encrypts cryptographic keys and creates a new instance of <see cref="VaultKeystoreDataModel"/>.
        /// </summary>
        /// <param name="passkey">The passkey credential that combines password and 'magic'.</param>
        /// <param name="encKey">The DEK key.</param>
        /// <param name="macKey">The MAC key.</param>
        /// <param name="salt">The salt used during KEK derivation.</param>
        /// <returns>A new instance of <see cref="VaultKeystoreDataModel"/> containing the encrypted cryptographic keys.</returns>
        [SkipLocalsInit]
        public static VaultKeystoreDataModel EncryptKeystore(
            SecretKey passkey,
            SecretKey encKey,
            SecretKey macKey,
            byte[] salt)
        {
            // Derive KEK
            Span<byte> kek = stackalloc byte[Cryptography.Constants.ARGON2_KEK_LENGTH];
            Argon2id.DeriveKey(passkey, salt, kek);

            // Wrap keys
            var wrappedEncKey = Rfc3394KeyWrap.WrapKey(encKey, kek);
            var wrappedMacKey = Rfc3394KeyWrap.WrapKey(macKey, kek);

            // Generate keystore
            return new()
            {
                WrappedEncKey = wrappedEncKey,
                WrappedMacKey = wrappedMacKey,
                Salt = salt
            };
        }
    }
}
