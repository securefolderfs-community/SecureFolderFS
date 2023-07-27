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
        /// Constructs a password key from provided <paramref name="password"/> and additional <paramref name="magic"/>.
        /// </summary>
        /// <param name="password">The user password part.</param>
        /// <param name="magic">The optional 'magic' part.</param>
        /// <returns>A concatenated <see cref="SecretKey"/> that represents the password key.</returns>
        public static SecretKey ConstructPasskey(IPassword password, SecretKey? magic)
        {
            var pwd = password.GetPassword();
            if (magic is not null) // Combine password and 'magic'
            {
                var passkey = new SecureKey(new byte[pwd.Length + magic.Key.Length]);
                var passkeySpan = passkey.Key.AsSpan();

                // Copy and combine
                pwd.CopyTo(passkeySpan);
                magic.Key.CopyTo(passkeySpan.Slice(pwd.Length));

                return passkey;
            }
            else // Just use password, if 'magic' is empty
            {
                // We need to copy the password to a SecretKey instance to represent the passkey.
                // By doing this, we no longer have to rely on provider's role of disposing the password object,
                // and thus we allow the consumer of the passkey to dispose of the key at their own discretion
                var passwordSecret = new SecureKey(new byte[pwd.Length]);
                pwd.CopyTo(passwordSecret.Key.AsSpan());

                return passwordSecret;
            }
        }

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
        /// <param name="passkey">The passkey credential that combines password and 'magic'.</param>
        /// <param name="keystoreDataModel">The keystore that holds wrapped keys.</param>
        /// <param name="cipherProvider">The cryptographic cipher provider.</param>
        /// <returns>A tuple containing the DEK and MAC keys respectively.</returns>
        [SkipLocalsInit]
        public static (SecretKey encKey, SecretKey macKey) DeriveKeystore(
            SecretKey passkey,
            VaultKeystoreDataModel keystoreDataModel,
            CipherProvider cipherProvider)
        {
            var encKey = new SecureKey(new byte[Constants.KeyChains.ENCKEY_LENGTH]);
            var macKey = new SecureKey(new byte[Constants.KeyChains.MACKEY_LENGTH]);

            // Derive KEK
            Span<byte> kek = stackalloc byte[Cryptography.Constants.ARGON2_KEK_LENGTH];
            cipherProvider.Argon2idCrypt.DeriveKey(passkey.Key, keystoreDataModel.Salt, kek);

            // Unwrap keys
            cipherProvider.Rfc3394KeyWrap.UnwrapKey(keystoreDataModel.WrappedEncKey, kek, encKey.Key);
            cipherProvider.Rfc3394KeyWrap.UnwrapKey(keystoreDataModel.WrappedMacKey, kek, macKey.Key);

            return (encKey, macKey);
        }

        /// <summary>
        /// Encrypts cryptographic keys and creates a new instance of <see cref="VaultKeystoreDataModel"/>.
        /// </summary>
        /// <param name="passkey">The passkey credential that combines password and 'magic'.</param>
        /// <param name="encKey">The DEK key.</param>
        /// <param name="macKey">The MAC key.</param>
        /// <param name="salt">The salt used during KEK derivation.</param>
        /// <param name="cipherProvider">The cryptographic cipher provider.</param>
        /// <returns>A new instance of <see cref="VaultKeystoreDataModel"/> containing the encrypted cryptographic keys.</returns>
        [SkipLocalsInit]
        public static VaultKeystoreDataModel EncryptKeystore(
            SecretKey passkey,
            SecretKey encKey,
            SecretKey macKey,
            byte[] salt,
            CipherProvider cipherProvider)
        {
            // Derive KEK
            Span<byte> kek = stackalloc byte[Cryptography.Constants.ARGON2_KEK_LENGTH];
            cipherProvider.Argon2idCrypt.DeriveKey(passkey, salt, kek);

            // Wrap keys
            var wrappedEncKey = cipherProvider.Rfc3394KeyWrap.WrapKey(encKey, kek);
            var wrappedMacKey = cipherProvider.Rfc3394KeyWrap.WrapKey(macKey, kek);

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
