using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Models;
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
        private readonly SecretKey _dekKey;
        private readonly SecretKey _macKey;
        private readonly VaultReader _vaultReader;
        private VaultConfigurationDataModel? _configDataModel;

        public RecoverRoutine(VaultReader vaultReader)
        {
            _vaultReader = vaultReader;
            _dekKey = new SecureKey(Cryptography.Constants.KeyTraits.DEK_KEY_LENGTH);
            _macKey = new SecureKey(Cryptography.Constants.KeyTraits.MAC_KEY_LENGTH);
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken)
        {
            _configDataModel = await _vaultReader.ReadConfigurationAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void SetCredentials(SecretKey passkey)
        {
            // Copy the first part (DEK) of the master key
            passkey.Key.AsSpan(0, Cryptography.Constants.KeyTraits.DEK_KEY_LENGTH).CopyTo(_dekKey.Key);

            // Copy the second part (MAC) of the master key
            passkey.Key.AsSpan(Cryptography.Constants.KeyTraits.MAC_KEY_LENGTH).CopyTo(_macKey.Key);
        }

        /// <inheritdoc/>
        public async Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_configDataModel);

            using (_dekKey)
            using (_macKey)
            {
                // Create MAC key copy for the validator that can be disposed here
                using var macKeyCopy = _macKey.CreateCopy();

                // Check if the payload has not been tampered with
                var validator = new ConfigurationValidator(macKeyCopy);
                await validator.ValidateAsync(_configDataModel, cancellationToken);

                // In this case, we rely on the consumer to take ownership of the keys, and thus manage their lifetimes
                // Key copies need to be created because the original ones are disposed of here
                return new SecurityWrapper(KeyPair.ImportKeys(_dekKey, _macKey), _configDataModel);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _dekKey.Dispose();
            _macKey.Dispose();
        }
    }
}
