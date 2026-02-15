using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.DeviceLink.ViewModels
{
    /// <summary>
    /// Represents a secure credential with an encrypted signing key.
    /// The signing key is encrypted using the pairing shared secret.
    /// </summary>
    [Serializable]
    [Bindable(true)]
    public sealed partial class CredentialViewModel : ObservableObject, IDisposable
    {
        [JsonIgnore] private byte[]? _decryptedHmacKey;
        [JsonIgnore] private bool _disposed;
        [JsonIgnore] private const int HMAC_KEY_SIZE = 32;

        /// <summary>
        /// The Credential ID (CID) that binds this credential to a desktop vault.
        /// </summary>
        [JsonPropertyName("cid")] [ObservableProperty] private string? _CredentialId;

        /// <summary>
        /// Human-readable name for this credential.
        /// </summary>
        [JsonPropertyName("displayName")] [ObservableProperty] private string? _DisplayName;

        /// <summary>
        /// Name of the vault this credential is bound to.
        /// </summary>
        [JsonPropertyName("vaultName")] [ObservableProperty] private string? _VaultName;

        /// <summary>
        /// Name of the desktop device this credential is paired with.
        /// </summary>
        [JsonPropertyName("machineName")] [ObservableProperty] private string? _DesktopName;

        /// <summary>
        /// Type of the desktop device this credential is paired with.
        /// </summary>
        [JsonPropertyName("machineType")] [ObservableProperty] private string? _DesktopType;

        /// <summary>
        /// Unique pairing identifier shared with desktop.
        /// </summary>
        [JsonPropertyName("pairingId")] [ObservableProperty] private string? _PairingId;

        /// <summary>
        /// When this credential was created/enrolled.
        /// </summary>
        [JsonPropertyName("creationDate")] [ObservableProperty] private DateTime? _CreatedAt;

        /// <summary>
        /// The persistent challenge that must be signed during authentication.
        /// This is stored during enrollment and verified during each authentication.
        /// </summary>
        [JsonPropertyName("challenge")] [ObservableProperty] private byte[]? _Challenge;

        /// <summary>
        /// The public signing key (exported after enrollment).
        /// </summary>
        [JsonPropertyName("c_hmacKey")]
        public byte[]? EncryptedHmacKey { get; set; }

        /// <summary>
        /// Nonce used for encrypting the signing key.
        /// </summary>
        [JsonPropertyName("encryptionNonce")]
        public byte[]? EncryptionNonce { get; set; }

        /// <summary>
        /// Auth tag from encrypting the signing key.
        /// </summary>
        [JsonPropertyName("encryptionTag")]
        public byte[]? EncryptionTag { get; set; }

        /// <summary>
        /// Whether this credential has been enrolled (has signing key).
        /// </summary>
        [JsonIgnore]
        public bool IsEnrolled => EncryptedHmacKey is not null && EncryptedHmacKey.Length > 0;

        /// <summary>
        /// Generates a new HMAC key and encrypts it with the encryption key.
        /// Called during enrollment.
        /// </summary>
        /// <param name="encryptionKey">The key derived from pairing session.</param>
        public void GenerateAndEncryptHmacKey(byte[] encryptionKey)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            // Generate new HMAC key (256-bit)
            var hmacKey = new byte[HMAC_KEY_SIZE];
            RandomNumberGenerator.Fill(hmacKey);

            try
            {
                // Encrypt the HMAC key
                EncryptHmacKey(hmacKey, encryptionKey);
            }
            finally
            {
                // Clear unencrypted key
                CryptographicOperations.ZeroMemory(hmacKey);
            }
        }

        private void EncryptHmacKey(byte[] hmacKey, byte[] encryptionKey)
        {
            // Generate random nonce
            EncryptionNonce = new byte[12];
            RandomNumberGenerator.Fill(EncryptionNonce);

            // Encrypt using the provided encryption key directly
            using var aes = new AesGcm(encryptionKey, 16);
            EncryptedHmacKey = new byte[hmacKey.Length];
            EncryptionTag = new byte[16];

            aes.Encrypt(EncryptionNonce, hmacKey, EncryptedHmacKey, EncryptionTag);
        }

        /// <summary>
        /// Decrypts the HMAC key so it can be used for authentication.
        /// Must be called before ComputeHmac.
        /// </summary>
        /// <param name="encryptionKey">The encryption key (already derived).</param>
        public void DecryptHmacKey(byte[] encryptionKey)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (EncryptedHmacKey.Length == 0)
                throw new InvalidOperationException("No encrypted HMAC key available");

            try
            {
                // Decrypt the HMAC key
                using var aes = new AesGcm(encryptionKey, 16);
                _decryptedHmacKey = new byte[EncryptedHmacKey.Length];

                aes.Decrypt(EncryptionNonce, EncryptedHmacKey, EncryptionTag, _decryptedHmacKey);
            }
            catch (CryptographicException ex)
            {
                throw new InvalidOperationException("Failed to decrypt HMAC key", ex);
            }
        }

        /// <summary>
        /// Computes HMAC-SHA256 over the given data using the decrypted HMAC key.
        /// HMAC is deterministic: same key + same data = same result.
        /// </summary>
        /// <param name="data">The data to authenticate.</param>
        /// <returns>The HMAC (32 bytes for SHA256).</returns>
        public byte[] ComputeHmac(byte[] data)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_decryptedHmacKey == null)
                throw new InvalidOperationException("HMAC key not available. Call DecryptHmacKey first.");

            return HMACSHA256.HashData(_decryptedHmacKey, data);
        }

        /// <summary>
        /// Clears the decrypted HMAC key from memory.
        /// Should be called after authentication is complete.
        /// </summary>
        public void ClearDecryptedKey()
        {
            if (_decryptedHmacKey != null)
            {
                CryptographicOperations.ZeroMemory(_decryptedHmacKey);
                _decryptedHmacKey = null;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            ClearDecryptedKey();

            if (EncryptedHmacKey.Length > 0)
                CryptographicOperations.ZeroMemory(EncryptedHmacKey);

            if (Challenge is { Length: > 0 })
                CryptographicOperations.ZeroMemory(Challenge);
        }
    }
}
