#if APP_PLATFORM_PRESENT
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.UI.ServiceImplementation
{
    public class SecurePropertyKeyStore : FileDeviceKeyStore
    {
        private const string PROTECTION_KEY_ALIAS = "app_platform_device_key_protection";
        private const int NONCE_SIZE = 12; // AesGcm.NonceByteSizes.MaxSize
        private const int TAG_SIZE = 16;   // AesGcm.TagByteSizes.MaxSize
        private const int KEY_SIZE = 32;   // AES-256
        
        private readonly SemaphoreSlim _keyLock = new(1, 1);
        private readonly IPropertyStore<string> _propertyStore;
        
        public SecurePropertyKeyStore(IPropertyStore<string> propertyStore, IModifiableFolder baseFolder)
            : base(baseFolder)
        {
            _propertyStore = propertyStore;
        }

        /// <inheritdoc/>
        protected override async Task<byte[]> ProtectAsync(byte[] data, CancellationToken cancellationToken)
        {
            var key = await GetOrCreateProtectionKeyAsync(cancellationToken);

            var nonce = RandomNumberGenerator.GetBytes(NONCE_SIZE);
            var ciphertext = new byte[NONCE_SIZE + data.Length + TAG_SIZE];
            var tag = new byte[TAG_SIZE];

            AesGcm256.Encrypt(
                data,
                key,
                nonce,
                tag,
                ciphertext.AsSpan(NONCE_SIZE, data.Length),
                ReadOnlySpan<byte>.Empty);
            
            // [nonce][ciphertext][tag]
            nonce.AsSpan().CopyTo(ciphertext.AsSpan(0, NONCE_SIZE));
            tag.AsSpan().CopyTo(ciphertext.AsSpan(NONCE_SIZE + data.Length, TAG_SIZE));
            
            return ciphertext;
        }

        /// <inheritdoc/>
        protected override async Task<byte[]> UnprotectAsync(byte[] data, CancellationToken cancellationToken)
        {
            if (data.Length < NONCE_SIZE + TAG_SIZE)
            {
                // A truncated/malformed blob can never be recovered; treat it like a key mismatch
                // so the caller discards it and regenerates fresh material instead of failing hard.
                throw new KeyMaterialUnrecoverableException("The protected data is malformed or truncated.");
            }

            var key = await GetOrCreateProtectionKeyAsync(cancellationToken);
            var plaintext = new byte[data.Length - NONCE_SIZE - TAG_SIZE];

            try
            {
                // [nonce][ciphertext][tag]
                AesGcm256.Decrypt(
                    data.AsSpan(NONCE_SIZE, data.Length - NONCE_SIZE - TAG_SIZE),
                    key,
                    data.AsSpan(0, NONCE_SIZE),
                    data.AsSpan(data.Length - TAG_SIZE, TAG_SIZE),
                    plaintext,
                    ReadOnlySpan<byte>.Empty
                    );
            }
            catch (CryptographicException ex)
            {
                // The stored protection key no longer matches this blob (e.g. the OS secret store was
                // reset, or the fallback store was regenerated). Surface it as unrecoverable so the
                // caller can drop the stale material and regenerate fresh keys instead of breaking.
                throw new KeyMaterialUnrecoverableException(
                    "The protected data could not be decrypted with the current protection key.", ex);
            }

            return plaintext;
        }
        
        private async Task<byte[]> GetOrCreateProtectionKeyAsync(CancellationToken cancellationToken)
        {
            await _keyLock.WaitAsync(cancellationToken);
            try
            {
                var existing = await _propertyStore.GetValueAsync<string?>(PROTECTION_KEY_ALIAS, null, cancellationToken);
                if (!string.IsNullOrEmpty(existing))
                    return Convert.FromBase64String(existing);

                var key = RandomNumberGenerator.GetBytes(KEY_SIZE);
                await _propertyStore.SetValueAsync(PROTECTION_KEY_ALIAS, Convert.ToBase64String(key), cancellationToken);

                return key;
            }
            finally
            {
                _keyLock.Release();
            }
        }
    }
}
#endif
