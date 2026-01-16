using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.PhoneLink.ViewModels
{
    /// <summary>
    /// Represents a secure credential with an encrypted signing key.
    /// The signing key is encrypted using the pairing shared secret.
    /// </summary>
    [Serializable]
    [Bindable(true)]
    public sealed partial class CredentialViewModel : ObservableObject, IDisposable
    {
        [JsonIgnore] private ECDsa? _signingKeyPair;
        [JsonIgnore] private byte[]? _decryptedPrivateKey;
        [JsonIgnore] private bool _disposed;

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
        [JsonPropertyName("machineName")] [ObservableProperty] private string? _MachineName;

        /// <summary>
        /// Unique pairing identifier shared with desktop.
        /// </summary>
        [JsonPropertyName("pairingId")] [ObservableProperty] private string? _PairingId;

        /// <summary>
        /// When this credential was created/enrolled.
        /// </summary>
        [JsonPropertyName("creationDate")] [ObservableProperty] private DateTime? _CreatedAt;

        /// <summary>
        /// Unique ID for this credential record.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// The public signing key (exported after enrollment).
        /// </summary>
        [JsonPropertyName("publicSigningKey")]
        public byte[]? PublicSigningKey { get; set; }

        /// <summary>
        /// The encrypted private signing key (for persistence).
        /// </summary>
        [JsonPropertyName("encryptedSigningKey")]
        public byte[]? EncryptedSigningKey { get; set; }

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
        public bool IsEnrolled => _signingKeyPair is not null || EncryptedSigningKey.Length > 0;

        /// <summary>
        /// Generates a new signing keypair and encrypts it with the shared secret.
        /// Called during enrollment when desktop provides the shared secret.
        /// </summary>
        /// <param name="sharedSecret">The ECDH shared secret from pairing.</param>
        public void GenerateAndEncryptSigningKey(byte[] sharedSecret)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            // Generate new ECDSA signing keypair
            _signingKeyPair?.Dispose();
            _signingKeyPair = ECDsa.Create(ECCurve.NamedCurves.nistP256);

            // Export keys
            PublicSigningKey = _signingKeyPair.ExportSubjectPublicKeyInfo();
            var privateKeyBytes = _signingKeyPair.ExportPkcs8PrivateKey();

            // Encrypt the private key with the shared secret
            EncryptPrivateKey(privateKeyBytes, sharedSecret);

            // Clear unencrypted private key
            CryptographicOperations.ZeroMemory(privateKeyBytes);
        }

        /// <summary>
        /// Decrypts the signing key so it can be used for signing.
        /// Must be called before SignChallenge.
        /// </summary>
        /// <param name="sharedSecret">The pairing shared secret.</param>
        public void DecryptSigningKey(byte[] sharedSecret)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(CredentialId);

            if (EncryptedSigningKey.Length == 0)
                throw new InvalidOperationException("No encrypted signing key available");

            // Derive decryption key from shared secret
            var decryptionKey = HKDF.DeriveKey(
                HashAlgorithmName.SHA256,
                sharedSecret,
                32,
                Encoding.UTF8.GetBytes(CredentialId),
                "PhoneLink-SigningKeyEncryption-v1"u8.ToArray());

            try
            {
                // Decrypt the private key
                using var aes = new AesGcm(decryptionKey, 16);
                _decryptedPrivateKey = new byte[EncryptedSigningKey.Length];

                // TODO: Tag mismatch exception. Possible cause: bad deserialization
                aes.Decrypt(EncryptionNonce, EncryptedSigningKey, EncryptionTag, _decryptedPrivateKey);

                // Import the key
                _signingKeyPair?.Dispose();
                _signingKeyPair = ECDsa.Create();
                _signingKeyPair.ImportPkcs8PrivateKey(_decryptedPrivateKey, out _);
            }
            catch (Exception ex)
            {
                _ = ex;
                throw;
            }
            finally
            {
                CryptographicOperations.ZeroMemory(decryptionKey);
            }
        }

        /// <summary>
        /// Signs a challenge with the (decrypted) signing key.
        /// </summary>
        /// <param name="data">The data to sign.</param>
        /// <returns>The signature.</returns>
        public byte[] SignData(byte[] data)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_signingKeyPair == null)
                throw new InvalidOperationException("Signing key not available. Call DecryptSigningKey first.");

            return _signingKeyPair.SignData(data, HashAlgorithmName.SHA256);
        }

        private void EncryptPrivateKey(byte[] privateKey, byte[] sharedSecret)
        {
            ArgumentNullException.ThrowIfNull(CredentialId);

            // Derive encryption key from shared secret
            var encryptionKey = HKDF.DeriveKey(
                HashAlgorithmName.SHA256,
                sharedSecret,
                32,
                Encoding.UTF8.GetBytes(CredentialId),
                "PhoneLink-SigningKeyEncryption-v1"u8.ToArray());

            try
            {
                // Generate random nonce
                EncryptionNonce = new byte[12];
                RandomNumberGenerator.Fill(EncryptionNonce);

                // Encrypt
                using var aes = new AesGcm(encryptionKey, 16);
                EncryptedSigningKey = new byte[privateKey.Length];
                EncryptionTag = new byte[16];

                aes.Encrypt(EncryptionNonce, privateKey, EncryptedSigningKey, EncryptionTag);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(encryptionKey);
            }
        }

        /// <summary>
        /// Clears the decrypted signing key from memory.
        /// Should be called after signing is complete.
        /// </summary>
        public void ClearDecryptedKey()
        {
            if (_decryptedPrivateKey is not null)
            {
                CryptographicOperations.ZeroMemory(_decryptedPrivateKey);
                _decryptedPrivateKey = null;
            }

            _signingKeyPair?.Dispose();
            _signingKeyPair = null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            ClearDecryptedKey();

            if (EncryptedSigningKey.Length > 0)
                CryptographicOperations.ZeroMemory(EncryptedSigningKey);

            if (PublicSigningKey.Length > 0)
                CryptographicOperations.ZeroMemory(PublicSigningKey);
        }
    }
}
