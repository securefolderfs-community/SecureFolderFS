using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.Helpers;
using SecureFolderFS.Core.Migration.DataModels;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.Migration.Helpers
{
    internal static class MigrationVaultParser
    {
        /// <summary>
        /// Computes a unique HMAC thumbprint of <paramref name="configDataModel"/> properties.
        /// </summary>
        /// <param name="configDataModel">The <see cref="V3VaultConfigurationDataModel"/> to compute the thumbprint for.</param>
        /// <param name="macKey">The key part of HMAC.</param>
        /// <param name="mac">The destination to fill the calculated HMAC thumbprint into.</param>
        public static void V3CalculateConfigMac(V3VaultConfigurationDataModel configDataModel, ReadOnlySpan<byte> macKey, Span<byte> mac)
        {
            // Initialize HMAC
            using var hmacSha256 = new HMACSHA256(macKey.ToArray());

            // Update HMAC
            hmacSha256.AppendData(BitConverter.GetBytes(configDataModel.Version));                                              // Version
            hmacSha256.AppendData(BitConverter.GetBytes(CryptHelpers.ContentCipherId(configDataModel.ContentCipherId)));        // ContentCipherScheme
            hmacSha256.AppendData(BitConverter.GetBytes(CryptHelpers.FileNameCipherId(configDataModel.FileNameCipherId)));      // FileNameCipherScheme
            hmacSha256.AppendData(BitConverter.GetBytes(configDataModel.RecycleBinSize));                                       // RecycleBinSize
            hmacSha256.AppendData(Encoding.UTF8.GetBytes(configDataModel.FileNameEncodingId));                                  // FileNameEncodingId
            hmacSha256.AppendData(Encoding.UTF8.GetBytes(configDataModel.Uid));                                                 // Id
            hmacSha256.AppendFinalData(Encoding.UTF8.GetBytes(configDataModel.AuthenticationMethod));                           // AuthMethod

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
        public static (byte[] dekKey, byte[] macKey) V3DeriveKeystore(ReadOnlySpan<byte> passkey, V3VaultKeystoreDataModel keystoreDataModel)
        {
            var dekKey = new byte[Cryptography.Constants.KeyTraits.DEK_KEY_LENGTH];
            var macKey = new byte[Cryptography.Constants.KeyTraits.MAC_KEY_LENGTH];

            // Derive KEK
            Span<byte> kek = stackalloc byte[Cryptography.Constants.KeyTraits.ARGON2_KEK_LENGTH];
            Argon2id.DeriveKey(passkey, keystoreDataModel.Salt, kek);

            // Unwrap keys
            using var rfc3394 = new Rfc3394KeyWrap();
            rfc3394.UnwrapKey(keystoreDataModel.WrappedDekKey, kek, dekKey);
            rfc3394.UnwrapKey(keystoreDataModel.WrappedMacKey, kek, macKey);

            return (dekKey, macKey);
        }

        /// <summary>
        /// Encrypts cryptographic keys and creates a new instance of <see cref="V3VaultKeystoreDataModel"/>.
        /// </summary>
        /// <param name="passkey">The passkey credential that combines password and 'magic'.</param>
        /// <param name="dekKey">The DEK key.</param>
        /// <param name="macKey">The MAC key.</param>
        /// <param name="salt">The salt used during KEK derivation.</param>
        /// <returns>A new instance of <see cref="V3VaultKeystoreDataModel"/> containing the encrypted cryptographic keys.</returns>
        [SkipLocalsInit]
        public static V3VaultKeystoreDataModel V3EncryptKeystore(
            ReadOnlySpan<byte> passkey,
            ReadOnlySpan<byte> dekKey,
            ReadOnlySpan<byte> macKey,
            byte[] salt)
        {
            // Derive KEK
            Span<byte> kek = stackalloc byte[Cryptography.Constants.KeyTraits.ARGON2_KEK_LENGTH];
            Argon2id.DeriveKey(passkey, salt, kek);

            // Wrap keys
            using var rfc3394 = new Rfc3394KeyWrap();
            var wrappedDekKey = rfc3394.WrapKey(dekKey, kek);
            var wrappedMacKey = rfc3394.WrapKey(macKey, kek);

            // Generate keystore
            return new()
            {
                WrappedDekKey = wrappedDekKey,
                WrappedMacKey = wrappedMacKey,
                Salt = salt
            };
        }
    }
}
