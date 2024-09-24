using SecureFolderFS.Core.Contracts;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Core.VaultAccess;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Routines.Operational
{
    /// <inheritdoc cref="ICredentialsRoutine"/>
    public sealed class RecoverRoutine : ICredentialsRoutine
    {
        private readonly SecretKey _encKey;
        private readonly SecretKey _macKey;
        private readonly VaultReader _vaultReader;
        private VaultKeystoreDataModel? _keystoreDataModel;
        private VaultConfigurationDataModel? _configDataModel;

        public RecoverRoutine(VaultReader vaultReader)
        {
            _vaultReader = vaultReader;
            _encKey = new SecureKey(Cryptography.Constants.KeyChains.ENCKEY_LENGTH);
            _macKey = new SecureKey(Cryptography.Constants.KeyChains.MACKEY_LENGTH);
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken)
        {
            _keystoreDataModel = await _vaultReader.ReadKeystoreAsync(cancellationToken);
            _configDataModel = await _vaultReader.ReadConfigurationAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void SetCredentials(SecretKey masterKey)
        {
            // Copy the first part (DEK) of the master key
            masterKey.Key.AsSpan(0, Cryptography.Constants.KeyChains.ENCKEY_LENGTH).CopyTo(_encKey.Key);

            // Copy the second part (MAC) of the master key
            masterKey.Key.AsSpan(Cryptography.Constants.KeyChains.MACKEY_LENGTH).CopyTo(_macKey.Key);
        }

        /// <inheritdoc/>
        public async Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_configDataModel);
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);

            using (_encKey)
            using (_macKey)
            {
                // Create MAC key copy for the validator that can be disposed here
                using var macKeyCopy = _macKey.CreateCopy();

                // Check if the payload has not been tampered with
                var validator = new ConfigurationValidator(macKeyCopy);
                await validator.ValidateAsync(_configDataModel, cancellationToken);

                // In this case, we rely on the consumer to take ownership of the keys, and thus manage their lifetimes
                // Key copies need to be created because the original ones are disposed of here
                return new SecurityContract(_encKey.CreateCopy(), _macKey.CreateCopy(), _keystoreDataModel, _configDataModel);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _encKey.Dispose();
            _macKey.Dispose();
        }
    }
}
