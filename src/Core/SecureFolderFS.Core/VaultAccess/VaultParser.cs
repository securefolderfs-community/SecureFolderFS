using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.Helpers;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Core.VaultAccess
{
    public static class VaultParser
    {
        /// <summary>
        /// Computes a unique HMAC thumbprint of <paramref name="configDataModel"/> properties.
        /// </summary>
        /// <param name="configDataModel">The <see cref="VaultConfigurationDataModel"/> to compute the thumbprint for.</param>
        /// <param name="macKey">The key part of HMAC.</param>
        /// <param name="mac">The destination to fill the calculated HMAC thumbprint into.</param>
        public static void CalculateConfigMac(VaultConfigurationDataModel configDataModel, ReadOnlySpan<byte> macKey, Span<byte> mac)
        {
            // Initialize HMAC
            using var hmacSha256 = new HMACSHA256(macKey.ToArray());

            // Update HMAC
            hmacSha256.AppendData(BitConverter.GetBytes(configDataModel.Version));                                          // Version
            hmacSha256.AppendData(BitConverter.GetBytes(CryptHelpers.ContentCipherId(configDataModel.ContentCipherId)));    // ContentCipherId
            hmacSha256.AppendData(BitConverter.GetBytes(CryptHelpers.FileNameCipherId(configDataModel.FileNameCipherId)));  // FileNameCipherId
            hmacSha256.AppendData(BitConverter.GetBytes(configDataModel.RecycleBinSize));                                   // RecycleBinSize
            hmacSha256.AppendData(BitConverter.GetBytes(configDataModel.ShorteningThreshold));                              // ShorteningThreshold
            hmacSha256.AppendData(Encoding.UTF8.GetBytes(configDataModel.FileNameEncodingId));                              // FileNameEncodingId
            hmacSha256.AppendData(Encoding.UTF8.GetBytes(configDataModel.Uid));                                             // Uid
            if (configDataModel.AppPlatform?.ServerUrl is { } serverUrl)
                hmacSha256.AppendData(Encoding.UTF8.GetBytes(serverUrl));                                                   // AppPlatform.ServerUrl
            if (configDataModel.ComplementGeneration > 0)
                hmacSha256.AppendData(BitConverter.GetBytes(configDataModel.ComplementGeneration));                         // ComplementGeneration (omitted at gen 0 for back-compat)
            hmacSha256.AppendFinalData(Encoding.UTF8.GetBytes(configDataModel.AuthenticationMethod));                       // AuthenticationMethod

            // Fill the hash to payload
            hmacSha256.GetCurrentHash(mac);
        }

        /// <summary>
        /// Derives DEK and MAC keys from provided credentials for a vault.
        /// Decrypts <see cref="VaultKeystoreDataModel.EncryptedSoftwareEntropy"/> using the
        /// raw passkey, then mixes it into the Argon2id input via HKDF-Extract before
        /// deriving the KEK. This raises the quantum security floor to 256 bits regardless
        /// of the entropy of the auth factor feeding the passkey.
        /// </summary>
        /// <param name="passkey">The passkey credential that combines all active auth factor outputs.</param>
        /// <param name="keystoreDataModel">The keystore that holds wrapped keys.</param>
        /// <returns>A tuple containing the DEK and MAC keys respectively.</returns>
        [SkipLocalsInit]
        public static (byte[] dekKey, byte[] macKey) DeriveKeystore(ReadOnlySpan<byte> passkey, VaultKeystoreDataModel keystoreDataModel)
        {
            ArgumentNullException.ThrowIfNull(keystoreDataModel.Salt);
            ArgumentNullException.ThrowIfNull(keystoreDataModel.EncryptedSoftwareEntropy);
            ArgumentNullException.ThrowIfNull(keystoreDataModel.SoftwareEntropyNonce);
            ArgumentNullException.ThrowIfNull(keystoreDataModel.SoftwareEntropyTag);

            var dekKey = new byte[Cryptography.Constants.KeyTraits.DEK_KEY_LENGTH];
            var macKey = new byte[Cryptography.Constants.KeyTraits.MAC_KEY_LENGTH];

            // Step 1: Decrypt SoftwareEntropy using a key derived from the raw passkey.
            //   The bootstrap key is derived from the passkey alone (not the augmented key)
            //   so that recovering SoftwareEntropy always requires all active auth factors.
            Span<byte> bootstrapKey = stackalloc byte[32];
            HKDF.DeriveKey(
                HashAlgorithmName.SHA256,
                passkey,
                bootstrapKey,
                keystoreDataModel.Salt, // Salt ties the bootstrap key to this specific keystore
                "SFFSv4-EntropyBootstrap-v1"u8);

            Span<byte> softwareEntropy = stackalloc byte[keystoreDataModel.EncryptedSoftwareEntropy.Length];
            using (var aes = new AesGcm(bootstrapKey, 16))
            {
                aes.Decrypt(
                    keystoreDataModel.SoftwareEntropyNonce,
                    keystoreDataModel.EncryptedSoftwareEntropy,
                    keystoreDataModel.SoftwareEntropyTag,
                    softwareEntropy);
            }

            try
            {
                // Step 2: Mix passkey and SoftwareEntropy via HKDF-Extract.
                //   passkey is IKM; SoftwareEntropy is salt.
                //   Breaking either alone is insufficient to reproduce the augmented key.
                Span<byte> augmentedPasskey = stackalloc byte[32];
                HKDF.DeriveKey(
                    HashAlgorithmName.SHA256,
                    passkey,
                    augmentedPasskey,
                    softwareEntropy,
                    "SFFSv4-AugmentedPasskey-v1"u8);

                // Step 3: Derive KEK from the augmented passkey
                Span<byte> kek = stackalloc byte[Cryptography.Constants.KeyTraits.ARGON2_KEK_LENGTH];
                Argon2id.DeriveKey(augmentedPasskey, keystoreDataModel.Salt, kek);

                // Step 4: Unwrap keys
                using var rfc3394 = new Rfc3394KeyWrap();
                rfc3394.UnwrapKey(keystoreDataModel.WrappedDekKey, kek, dekKey);
                rfc3394.UnwrapKey(keystoreDataModel.WrappedMacKey, kek, macKey);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(softwareEntropy);
            }

            return (dekKey, macKey);
        }

        /// <summary>
        /// Encrypts cryptographic keys and creates a new instance of <see cref="VaultKeystoreDataModel"/>.
        /// Generates and encrypts a fresh <see cref="VaultKeystoreDataModel.EncryptedSoftwareEntropy"/>
        /// which is mixed into Argon2id input at unlock time to raise the quantum security floor.
        /// </summary>
        /// <param name="passkey">The passkey credential that combines all active auth factor outputs.</param>
        /// <param name="dekKey">The DEK key.</param>
        /// <param name="macKey">The MAC key.</param>
        /// <param name="salt">The salt used during KEK derivation.</param>
        /// <returns>A new instance of <see cref="VaultKeystoreDataModel"/> containing the encrypted cryptographic keys and entropy.</returns>
        [SkipLocalsInit]
        public static VaultKeystoreDataModel EncryptKeystore(
            ReadOnlySpan<byte> passkey,
            ReadOnlySpan<byte> dekKey,
            ReadOnlySpan<byte> macKey,
            byte[] salt)
        {
            // Step 1: Generate fresh SoftwareEntropy (256-bit CSPRNG)
            Span<byte> softwareEntropy = stackalloc byte[32];
            RandomNumberGenerator.Fill(softwareEntropy);

            return EncryptKeystoreWithEntropy(passkey, dekKey, macKey, salt, softwareEntropy);
        }

        /// <summary>
        /// Re-encrypts cryptographic keys into a new <see cref="VaultKeystoreDataModel"/> while
        /// preserving the provided <paramref name="existingSoftwareEntropy"/>.
        /// This is an optional credential-rotation path when the previous passkey is available.
        /// </summary>
        /// <param name="passkey">The new passkey credential.</param>
        /// <param name="dekKey">The DEK key (unchanged from the existing keystore).</param>
        /// <param name="macKey">The MAC key (unchanged from the existing keystore).</param>
        /// <param name="salt">A freshly generated salt for the new keystore.</param>
        /// <param name="existingSoftwareEntropy">The plaintext SoftwareEntropy recovered from the old keystore.</param>
        /// <returns>A new <see cref="VaultKeystoreDataModel"/> with re-encrypted keys and entropy.</returns>
        [SkipLocalsInit]
        public static VaultKeystoreDataModel ReEncryptKeystore(
            ReadOnlySpan<byte> passkey,
            ReadOnlySpan<byte> dekKey,
            ReadOnlySpan<byte> macKey,
            byte[] salt,
            ReadOnlySpan<byte> existingSoftwareEntropy)
        {
            return EncryptKeystoreWithEntropy(passkey, dekKey, macKey, salt, existingSoftwareEntropy);
        }

        /// <summary>
        /// Decrypts the <see cref="VaultKeystoreDataModel.EncryptedSoftwareEntropy"/> from an existing
        /// keystore using the previous passkey.
        /// This is only required for preserve-entropy rotation; fresh-entropy rotation uses
        /// <see cref="EncryptKeystore"/>.
        /// </summary>
        /// <param name="passkey">The current (old) passkey.</param>
        /// <param name="keystoreDataModel">The existing V4 keystore.</param>
        /// <param name="softwareEntropy">The destination span to fill with the decrypted entropy (must be 32 bytes).</param>
        public static void DecryptSoftwareEntropy(
            ReadOnlySpan<byte> passkey,
            VaultKeystoreDataModel keystoreDataModel,
            Span<byte> softwareEntropy)
        {
            ArgumentNullException.ThrowIfNull(keystoreDataModel.Salt);
            ArgumentNullException.ThrowIfNull(keystoreDataModel.EncryptedSoftwareEntropy);
            ArgumentNullException.ThrowIfNull(keystoreDataModel.SoftwareEntropyNonce);
            ArgumentNullException.ThrowIfNull(keystoreDataModel.SoftwareEntropyTag);

            Span<byte> bootstrapKey = stackalloc byte[32];
            HKDF.DeriveKey(
                HashAlgorithmName.SHA256,
                passkey,
                bootstrapKey,
                keystoreDataModel.Salt,
                "SFFSv4-EntropyBootstrap-v1"u8);

            using var aes = new AesGcm(bootstrapKey, 16);
            aes.Decrypt(
                keystoreDataModel.SoftwareEntropyNonce,
                keystoreDataModel.EncryptedSoftwareEntropy,
                keystoreDataModel.SoftwareEntropyTag,
                softwareEntropy);
        }

        public static void DeriveComplementKey(
            ReadOnlySpan<byte> passkey,
            string vaultId,
            string authenticationMethodId,
            int generation,
            Span<byte> complementKey)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(vaultId);
            ArgumentException.ThrowIfNullOrWhiteSpace(authenticationMethodId);
            ArgumentOutOfRangeException.ThrowIfNegative(generation);

            var salt = Encoding.UTF8.GetBytes(vaultId);

            // Generation 0 reproduces the legacy derivation (no suffix); any later generation mixes in
            // the counter so rotating it produces an entirely different complement domain, invalidating
            // shares and keystore material issued under previous generations.
            var info = generation > 0
                ? Encoding.UTF8.GetBytes($"{authenticationMethodId}|gen={generation}")
                : Encoding.UTF8.GetBytes(authenticationMethodId);

            HKDF.DeriveKey(
                HashAlgorithmName.SHA256,
                passkey,
                complementKey,
                salt,
                info);
        }

        public static VaultShareDataModel WrapComplementSecret(
            ReadOnlySpan<byte> complementSecret,
            ReadOnlySpan<byte> wrappingKeyMaterial,
            string vaultId,
            string authenticationMethodId,
            int generation)
        {
            Span<byte> complementWrapKey = stackalloc byte[32];
            try
            {
                DeriveComplementKey(wrappingKeyMaterial, vaultId, authenticationMethodId, generation, complementWrapKey);

                var nonce = new byte[12];
                var tag = new byte[16];
                var wrapped = new byte[complementSecret.Length];
                RandomNumberGenerator.Fill(nonce);

                using (var aes = new AesGcm(complementWrapKey, 16))
                    aes.Encrypt(nonce, complementSecret, wrapped, tag);

                return new()
                {
                    AuthenticationMethodId = authenticationMethodId,
                    Nonce = nonce,
                    WrappedComplementSecret = wrapped,
                    Tag = tag
                };
            }
            finally
            {
                CryptographicOperations.ZeroMemory(complementWrapKey);
            }
        }

        public static byte[] UnwrapComplementSecret(
            ReadOnlySpan<byte> wrappingKeyMaterial,
            string vaultId,
            VaultShareDataModel shareDataModel,
            int generation)
        {
            ArgumentNullException.ThrowIfNull(shareDataModel.AuthenticationMethodId);
            ArgumentNullException.ThrowIfNull(shareDataModel.Nonce);
            ArgumentNullException.ThrowIfNull(shareDataModel.WrappedComplementSecret);
            ArgumentNullException.ThrowIfNull(shareDataModel.Tag);

            Span<byte> complementWrapKey = stackalloc byte[32];
            try
            {
                DeriveComplementKey(wrappingKeyMaterial, vaultId, shareDataModel.AuthenticationMethodId, generation, complementWrapKey);

                var complementSecret = new byte[shareDataModel.WrappedComplementSecret.Length];
                using var aes = new AesGcm(complementWrapKey, 16);
                aes.Decrypt(shareDataModel.Nonce, shareDataModel.WrappedComplementSecret, shareDataModel.Tag, complementSecret);

                return complementSecret;
            }
            finally
            {
                CryptographicOperations.ZeroMemory(complementWrapKey);
            }
        }

        /// <summary>
        /// Shared implementation for both <see cref="EncryptKeystore"/> and <see cref="ReEncryptKeystore"/>.
        /// Encrypts the provided entropy under the passkey and wraps DEK/MAC under the augmented KEK.
        /// </summary>
        [SkipLocalsInit]
        private static VaultKeystoreDataModel EncryptKeystoreWithEntropy(
            ReadOnlySpan<byte> passkey,
            ReadOnlySpan<byte> dekKey,
            ReadOnlySpan<byte> macKey,
            byte[] salt,
            ReadOnlySpan<byte> softwareEntropy)
        {
            // Step 1: Encrypt SoftwareEntropy under a bootstrap key derived from the raw passkey.
            //   Using the raw passkey (not the augmented one) means decrypting entropy
            //   always requires all active auth factors — same guarantee at both creation and unlock.
            Span<byte> bootstrapKey = stackalloc byte[32];
            HKDF.DeriveKey(
                HashAlgorithmName.SHA256,
                passkey,
                bootstrapKey,
                salt,
                "SFFSv4-EntropyBootstrap-v1"u8);

            var entropyNonce = new byte[12];
            var entropyTag = new byte[16];
            var encryptedEntropy = new byte[softwareEntropy.Length];
            RandomNumberGenerator.Fill(entropyNonce);

            using (var aes = new AesGcm(bootstrapKey, 16))
            {
                aes.Encrypt(entropyNonce, softwareEntropy, encryptedEntropy, entropyTag);
            }

            // Step 2: Augment passkey with SoftwareEntropy via HKDF-Extract before Argon2id.
            //   This is the same derivation performed at unlock in V4DeriveKeystore.
            Span<byte> augmentedPasskey = stackalloc byte[32];
            HKDF.DeriveKey(
                HashAlgorithmName.SHA256,
                passkey,
                augmentedPasskey,
                softwareEntropy,
                "SFFSv4-AugmentedPasskey-v1"u8);

            // Step 3: Derive KEK from augmented passkey
            Span<byte> kek = stackalloc byte[Cryptography.Constants.KeyTraits.ARGON2_KEK_LENGTH];
            Argon2id.DeriveKey(augmentedPasskey, salt, kek);

            // Step 4: Wrap keys
            using var rfc3394 = new Rfc3394KeyWrap();
            var wrappedDekKey = rfc3394.WrapKey(dekKey, kek);
            var wrappedMacKey = rfc3394.WrapKey(macKey, kek);

            return new()
            {
                WrappedDekKey = wrappedDekKey,
                WrappedMacKey = wrappedMacKey,
                Salt = salt,
                EncryptedSoftwareEntropy = encryptedEntropy,
                SoftwareEntropyNonce = entropyNonce,
                SoftwareEntropyTag = entropyTag
            };
        }
    }
}
