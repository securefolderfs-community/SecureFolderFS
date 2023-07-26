using System;
using System.Runtime.CompilerServices;
using System.Text;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Core.VaultAccess
{
    internal static class VaultParser
    {
        /// <summary>
        /// Computes unique HMAC thumbprint of <paramref name="configDataModel"/> properties.
        /// </summary>
        /// <param name="configDataModel">The <see cref="VaultConfigurationDataModel"/> to compute the thumbprint for.</param>
        /// <param name="macKey">The key part of HMAC.</param>
        /// <param name="hmacSha256">The HMAC-SHA256 cryptographic provider.</param>
        /// <param name="mac">The destination to fill the calculated HMAC thumbprint into.</param>
        public static void CalculatePayloadMac(VaultConfigurationDataModel configDataModel, SecretKey macKey, IHmacSha256Crypt hmacSha256, Span<byte> mac)
        {
            // Initialize HMAC
            using var hmacSha256Crypt = hmacSha256.GetInstance();
            hmacSha256Crypt.InitializeHmac(macKey);

            // Update HMAC
            hmacSha256Crypt.Update(BitConverter.GetBytes(Constants.VaultVersion.LATEST_VERSION));       // Version
            hmacSha256Crypt.Update(BitConverter.GetBytes((uint)configDataModel.ContentCipherScheme));   // ContentCipherScheme
            hmacSha256Crypt.Update(BitConverter.GetBytes((uint)configDataModel.FileNameCipherScheme));  // FileNameCipherScheme
            hmacSha256Crypt.Update(Encoding.UTF8.GetBytes(configDataModel.Id));                         // Id
            hmacSha256Crypt.Update(Encoding.UTF8.GetBytes(configDataModel.AuthMethod));                 // AuthMethod

            // Fill the hash to payload
            hmacSha256Crypt.GetHash(mac);
        }

        /// <summary>
        /// Derives DEK and MAC keys from provided credentials.
        /// </summary>
        /// <param name="password">The user-typed password credential.</param>
        /// <param name="magic">The 'magic' key credential for additional authentication.</param>
        /// <param name="keystoreDataModel">The keystore that holds wrapped keys.</param>
        /// <param name="cipherProvider">The cryptographic cipher provider.</param>
        /// <returns>A tuple containing the DEK and MAC keys respectively.</returns>
        [SkipLocalsInit]
        public static (SecretKey encKey, SecretKey macKey) DeriveKeystore(
            IPassword password,
            SecretKey? magic,
            VaultKeystoreDataModel keystoreDataModel,
            CipherProvider cipherProvider)
        {
            var encKey = new SecureKey(new byte[Constants.KeyChains.ENCKEY_LENGTH]);
            var macKey = new SecureKey(new byte[Constants.KeyChains.MACKEY_LENGTH]);

            // Create passkey
            Span<byte> passkey;
            if (magic is not null)
            {
                var pwd = password.GetPassword();
                passkey = new byte[pwd.Length + magic.Key.Length];

                pwd.CopyTo(passkey);
                magic.Key.CopyTo(passkey.Slice(0, pwd.Length));
            }
            else
                passkey = password.GetPassword();

            // Derive KEK
            Span<byte> kek = stackalloc byte[Cryptography.Constants.ARGON2_KEK_LENGTH];
            cipherProvider.Argon2idCrypt.DeriveKey(passkey, keystoreDataModel.Salt, kek);

            // Unwrap keys
            cipherProvider.Rfc3394KeyWrap.UnwrapKey(keystoreDataModel.WrappedEncKey, kek, encKey.Key);
            cipherProvider.Rfc3394KeyWrap.UnwrapKey(keystoreDataModel.WrappedMacKey, kek, macKey.Key);

            return (encKey, macKey);
        }
    }
}
