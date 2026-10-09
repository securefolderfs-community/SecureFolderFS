using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
        /// The passkey is stretched with Argon2id to produce the KEK, which unwraps the stored
        /// keys. Argon2id is the only step between the passkey and the KEK, so the sole way to
        /// test a candidate passkey against the keystore is the RFC3394 unwrap.
        /// </summary>
        /// <param name="passkey">The passkey credential that combines all active auth factor outputs.</param>
        /// <param name="keystoreDataModel">The keystore that holds wrapped keys.</param>
        /// <returns>A tuple containing the DEK and MAC keys respectively.</returns>
        public static (byte[] dekKey, byte[] macKey) DeriveKeystore(ReadOnlySpan<byte> passkey, VaultKeystoreDataModel keystoreDataModel)
        {
            ArgumentNullException.ThrowIfNull(keystoreDataModel.Salt);

            Span<byte> kek = stackalloc byte[Cryptography.Constants.KeyTraits.ARGON2_KEK_LENGTH];
            try
            {
                Argon2id.DeriveKey(passkey, keystoreDataModel.Salt, kek);
                return UnwrapKeys(kek, keystoreDataModel);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(kek);
            }
        }

        /// <inheritdoc cref="DeriveKeystore"/>
        /// <remarks>
        /// The awaitable form exists because the Argon2id step must not block the calling thread on single-threaded runtimes (browser WASM).
        /// <br/>
        /// Caller retains ownership of <paramref name="passkey"/>.
        /// </remarks>
        public static async Task<(byte[] dekKey, byte[] macKey)> DeriveKeystoreAsync(byte[] passkey, VaultKeystoreDataModel keystoreDataModel)
        {
            ArgumentNullException.ThrowIfNull(keystoreDataModel.Salt);

            var kek = new byte[Cryptography.Constants.KeyTraits.ARGON2_KEK_LENGTH];
            try
            {
                await Argon2id.DeriveKeyAsync(passkey, keystoreDataModel.Salt, kek).ConfigureAwait(false);
                return UnwrapKeys(kek, keystoreDataModel);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(kek);
            }
        }

        /// <summary>
        /// Confirms that <paramref name="passkey"/> opens <paramref name="keystoreDataModel"/>, without
        /// returning the unwrapped keys. Used by credential- and complementation-change routines to
        /// authenticate a supplied credential against the existing keystore before re-keying it.
        /// </summary>
        /// <param name="passkey">The passkey credential to verify.</param>
        /// <param name="keystoreDataModel">The existing keystore to verify against.</param>
        public static void VerifyKeystoreKey(ReadOnlySpan<byte> passkey, VaultKeystoreDataModel keystoreDataModel)
        {
            var (dekKey, macKey) = DeriveKeystore(passkey, keystoreDataModel);
            CryptographicOperations.ZeroMemory(dekKey);
            CryptographicOperations.ZeroMemory(macKey);
        }

        /// <summary>
        /// Unwraps the stored DEK and MAC keys with the supplied KEK. The RFC3394 unwrap is
        /// integrity-checked, so a wrong KEK throws instead of yielding garbage keys.
        /// </summary>
        private static (byte[] dekKey, byte[] macKey) UnwrapKeys(ReadOnlySpan<byte> kek, VaultKeystoreDataModel keystoreDataModel)
        {
            // A keystore missing either wrapped key would otherwise reach the unwrap as an empty span,
            // whose failure mode differs per backend. Unlock's fallback chain only catches
            // CryptographicException, so fail here with a definite exception type instead.
            ArgumentNullException.ThrowIfNull(keystoreDataModel.WrappedDekKey);
            ArgumentNullException.ThrowIfNull(keystoreDataModel.WrappedMacKey);

            var dekKey = new byte[Cryptography.Constants.KeyTraits.DEK_KEY_LENGTH];
            var macKey = new byte[Cryptography.Constants.KeyTraits.MAC_KEY_LENGTH];
            try
            {
                using var rfc3394 = new Rfc3394KeyWrap();
                rfc3394.UnwrapKey(keystoreDataModel.WrappedDekKey, kek, dekKey);
                rfc3394.UnwrapKey(keystoreDataModel.WrappedMacKey, kek, macKey);

                return (dekKey, macKey);
            }
            catch
            {
                CryptographicOperations.ZeroMemory(dekKey);
                CryptographicOperations.ZeroMemory(macKey);
                throw;
            }
        }

        /// <summary>
        /// Encrypts cryptographic keys and creates a new instance of <see cref="VaultKeystoreDataModel"/>.
        /// The KEK is derived from the passkey with Argon2id alone; the DEK and MAC keys it wraps are
        /// already full-width CSPRNG values, so no additional key material is stored alongside them.
        /// </summary>
        /// <param name="passkey">The passkey credential that combines all active auth factor outputs.</param>
        /// <param name="dekKey">The DEK key.</param>
        /// <param name="macKey">The MAC key.</param>
        /// <param name="salt">The salt used during KEK derivation.</param>
        /// <returns>A new instance of <see cref="VaultKeystoreDataModel"/> containing the encrypted cryptographic keys.</returns>
        [SkipLocalsInit]
        public static VaultKeystoreDataModel EncryptKeystore(
            ReadOnlySpan<byte> passkey,
            ReadOnlySpan<byte> dekKey,
            ReadOnlySpan<byte> macKey,
            byte[] salt)
        {
            Span<byte> kek = stackalloc byte[Cryptography.Constants.KeyTraits.ARGON2_KEK_LENGTH];
            try
            {
                // Derive the KEK from the passkey, then wrap the keys under it. Mirrors DeriveKeystore.
                Argon2id.DeriveKey(passkey, salt, kek);

                using var rfc3394 = new Rfc3394KeyWrap();
                var wrappedDekKey = rfc3394.WrapKey(dekKey, kek);
                var wrappedMacKey = rfc3394.WrapKey(macKey, kek);

                return new()
                {
                    WrappedDekKey = wrappedDekKey,
                    WrappedMacKey = wrappedMacKey,
                    Salt = salt
                };
            }
            finally
            {
                CryptographicOperations.ZeroMemory(kek);
            }
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

                AesGcm256.Encrypt(complementSecret, complementWrapKey, nonce, tag, wrapped, ReadOnlySpan<byte>.Empty);

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
                AesGcm256.Decrypt(
                    shareDataModel.WrappedComplementSecret,
                    complementWrapKey,
                    shareDataModel.Nonce,
                    shareDataModel.Tag,
                    complementSecret,
                    ReadOnlySpan<byte>.Empty);

                return complementSecret;
            }
            finally
            {
                CryptographicOperations.ZeroMemory(complementWrapKey);
            }
        }
    }
}
