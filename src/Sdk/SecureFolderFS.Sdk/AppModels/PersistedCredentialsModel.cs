using System.Collections.Generic;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <summary>
    /// Represents a model that stores session credentials of vaults.
    /// </summary>
    public class PersistedCredentialsModel
    {
        private readonly Dictionary<string, IKeyUsage> _credentials;

        /// <summary>
        /// Gets the read-only collection of credentials.
        /// </summary>
        public IReadOnlyDictionary<string, IKeyUsage> Credentials => _credentials;

        /// <summary>
        /// A single instance of <see cref="PersistedCredentialsModel"/>.
        /// </summary>
        public static PersistedCredentialsModel Instance { get; } = new();

        public PersistedCredentialsModel()
        {
            _credentials = new();
        }

        /// <summary>
        /// Adds a new key-value pair to the credential store or updates the value for an existing key.
        /// </summary>
        /// <param name="vaultId">The identifier for the vault associated with the key.</param>
        /// <param name="keyUsage">An implementation of <see cref="IKeyUsage"/> that contains the key to be added or updated.</param>
        public void SetOrAdd(string vaultId, IKeyUsage keyUsage)
        {
            _credentials.Get(vaultId)?.Dispose();
            _credentials[vaultId] = keyUsage;
        }

        /// <summary>
        /// Removes a key-value pair associated with the specified vault identifier from the credential store.
        /// </summary>
        /// <param name="vaultId">The identifier for the vault whose associated key-value pair is to be removed.</param>
        /// <returns>
        /// <c>true</c> if the key-value pair was successfully removed; otherwise, <c>false</c>.
        /// </returns>
        public bool Remove(string vaultId)
        {
            if (_credentials.Remove(vaultId, out var value))
            {
                value.Dispose();
                return true;
            }

            return false;
        }
    }
}