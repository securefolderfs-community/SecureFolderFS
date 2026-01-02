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
    public sealed class RecoverRoutine : ICredentialsRoutine, IFinalizationRoutine
    {
        private readonly VaultReader _vaultReader;
        private VaultConfigurationDataModel? _configDataModel;
        private KeyPair? _keyPair;

        public RecoverRoutine(VaultReader vaultReader)
        {
            _vaultReader = vaultReader;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken)
        {
            _configDataModel = await _vaultReader.ReadConfigurationAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void SetCredentials(ManagedKey passkey)
        {
            _keyPair = KeyPair.CopyFromRecoveryKey(passkey);
        }

        /// <inheritdoc/>
        public async Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_keyPair);
            ArgumentNullException.ThrowIfNull(_configDataModel);

            using (_keyPair)
            {
                // Create MAC key copy for the validator that can be disposed here
                using var macKeyCopy = _keyPair.MacKey.CreateCopy();

                // Check if the payload has not been tampered with
                var validator = new ConfigurationValidator(macKeyCopy);
                await validator.ValidateAsync(_configDataModel, cancellationToken);

                // In this case, we rely on the consumer to take ownership of the keys, and thus manage their lifetimes
                // Key copies need to be created because the original ones are disposed of here
                return new SecurityWrapper(KeyPair.ImportKeys(_keyPair.DekKey, _keyPair.MacKey), _configDataModel);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _keyPair?.Dispose();
        }
    }
}
