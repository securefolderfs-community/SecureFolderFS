using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.PhoneLink.ViewModels;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using static SecureFolderFS.Sdk.PhoneLink.Constants;

namespace SecureFolderFS.Sdk.PhoneLink.Models
{
    public sealed class CredentialsStoreModel : IPersistable, IDisposable
    {
        private readonly IPropertyStore<string> _propertyStore;
        private readonly IAsyncSerializer<Stream> _streamSerializer;
        private readonly List<CredentialViewModel> _credentials = [];
        private bool _disposed;

        /// <summary>
        /// All stored credentials.
        /// </summary>
        public IReadOnlyList<CredentialViewModel> Credentials => _credentials;

        public CredentialsStoreModel(IPropertyStore<string> propertyStore, IAsyncSerializer<Stream> streamSerializer)
        {
            _propertyStore = propertyStore;
            _streamSerializer = streamSerializer;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var rawCredentials = await _propertyStore.GetValueAsync<string>(DATA_SOURCE_PHONE_LINK, null, cancellationToken);
            if (string.IsNullOrEmpty(rawCredentials))
                return;

            var deserialized = await _streamSerializer.TryDeserializeFromStringAsync<List<CredentialViewModel>>(rawCredentials, cancellationToken);
            if (deserialized is null)
                return;

            _credentials.Clear();
            _credentials.AddRange(deserialized);
        }

        /// <inheritdoc/>
        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var serialized = await _streamSerializer.TrySerializeToStringAsync(_credentials, cancellationToken);
            if (serialized is null)
                return;

            await _propertyStore.SetValueAsync(DATA_SOURCE_PHONE_LINK, serialized, cancellationToken);
        }

        /// <summary>
        /// Enrolls a credential with pairing data from desktop.
        /// Generates and encrypts the signing key.
        /// </summary>
        public async Task EnrollCredentialAsync(
            CredentialViewModel credential,
            string credentialId,
            string vaultName,
            string desktopName,
            string desktopType,
            string pairingId,
            byte[] challenge,
            byte[] sessionSecret)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            credential.CredentialId = credentialId;
            credential.VaultName = vaultName;
            credential.MachineName = desktopName;
            credential.MachineType = desktopType;
            credential.PairingId = pairingId;
            credential.Challenge = challenge;

            // Derive encryption key from session secret
            var encryptionKey = DeriveEncryptionKey(sessionSecret, credentialId);

            try
            {
                // Generate and encrypt the HMAC key
                credential.GenerateAndEncryptHmacKey(encryptionKey);

                // Add to credentials list if not already there
                if (!_credentials.Contains(credential))
                {
                    _credentials.Add(credential);
                }

                // Store the encryption key securely (not the session secret)
                await StoreEncryptionKeyAsync(pairingId, encryptionKey);

                // Save updated credentials
                await SaveAsync();
            }
            finally
            {
                CryptographicOperations.ZeroMemory(encryptionKey);
            }
        }

        /// <summary>
        /// Derives an encryption key from the session secret.
        /// </summary>
        private static byte[] DeriveEncryptionKey(byte[] sessionSecret, string credentialId)
        {
            return HKDF.DeriveKey(
                HashAlgorithmName.SHA256,
                sessionSecret,
                32,
                Encoding.UTF8.GetBytes(credentialId),
                "PhoneLink-SigningKeyEncryption-v2"u8.ToArray());
        }

        /// <summary>
        /// Stores an encryption key securely.
        /// </summary>
        private async Task StoreEncryptionKeyAsync(string pairingId, byte[] encryptionKey)
        {
            var encoded = Convert.ToBase64String(encryptionKey);
            await _propertyStore.SetValueAsync(SECRETS_KEY_PREFIX + pairingId, encoded);
        }

        /// <summary>
        /// Gets a credential by its CID.
        /// </summary>
        public CredentialViewModel? GetByCredentialId(string credentialId)
        {
            return _credentials.FirstOrDefault(c => c.CredentialId == credentialId);
        }

        /// <summary>
        /// Gets a credential by pairing ID.
        /// </summary>
        public CredentialViewModel? GetByPairingId(string pairingId)
        {
            return _credentials.FirstOrDefault(c => c.PairingId == pairingId);
        }

        /// <summary>
        /// Gets the encryption key for decrypting the signing key.
        /// </summary>
        public async Task<byte[]?> GetEncryptionKeyAsync(string pairingId)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            try
            {
                var encoded = await _propertyStore.GetValueAsync<string?>(SECRETS_KEY_PREFIX + pairingId);
                if (string.IsNullOrEmpty(encoded))
                    return null;

                return Convert.FromBase64String(encoded);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Deletes a credential.
        /// </summary>
        public async Task DeleteCredentialAsync(string id)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var credential = _credentials.FirstOrDefault(c => c.Id == id);
            if (credential == null)
                return;

            // Remove encryption key
            if (!string.IsNullOrEmpty(credential.PairingId))
                await _propertyStore.RemoveAsync(SECRETS_KEY_PREFIX + credential.PairingId);

            credential.Dispose();
            _credentials.Remove(credential);
            await SaveAsync();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _credentials.DisposeAll();
            _credentials.Clear();
        }
    }
}
