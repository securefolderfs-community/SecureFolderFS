using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
        private readonly Dictionary<string, byte[]> _secretsCache = [];
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
            string pairingId,
            byte[] sharedSecret)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            credential.CredentialId = credentialId;
            credential.VaultName = vaultName;
            credential.MachineName = desktopName;
            credential.PairingId = pairingId;

            // Generate and encrypt the signing key
            credential.GenerateAndEncryptSigningKey(sharedSecret);

            // Add to credentials list if not already there
            if (!_credentials.Contains(credential))
            {
                _credentials.Add(credential);
            }

            // Store the shared secret securely
            await StoreSharedSecretAsync(pairingId, sharedSecret);

            // Save updated credentials
            await SaveAsync();
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
        /// Gets the shared secret for a pairing.
        /// </summary>
        public async Task<byte[]?> GetSharedSecretAsync(string pairingId)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            // Check cache
            if (_secretsCache.TryGetValue(pairingId, out var cached))
                return (byte[])cached.Clone();

            try
            {
                var encoded = await _propertyStore.GetValueAsync<string>(SECRETS_KEY_PREFIX + pairingId);
                if (string.IsNullOrEmpty(encoded))
                    return null;

                var secret = Convert.FromBase64String(encoded);
                _secretsCache[pairingId] = (byte[])secret.Clone();
                return secret;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Stores a shared secret securely.
        /// </summary>
        private async Task StoreSharedSecretAsync(string pairingId, byte[] sharedSecret)
        {
            var encoded = Convert.ToBase64String(sharedSecret);
            await _propertyStore.SetValueAsync(SECRETS_KEY_PREFIX + pairingId, encoded);
            _secretsCache[pairingId] = (byte[])sharedSecret.Clone();
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

            // Remove shared secret
            if (!string.IsNullOrEmpty(credential.PairingId))
            {
                await _propertyStore.RemoveAsync(SECRETS_KEY_PREFIX + credential.PairingId);
                _secretsCache.Remove(credential.PairingId);
            }

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

            foreach (var secret in _secretsCache.Values)
                CryptographicOperations.ZeroMemory(secret);

            _secretsCache.Clear();
        }
    }
}
