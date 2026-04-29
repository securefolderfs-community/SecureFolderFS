using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Core.Routines.Operational
{
    /// <inheritdoc cref="ICredentialsRoutine"/>
    public sealed class RecoverRoutine : ICredentialsRoutine, IFinalizationRoutine
    {
        private readonly VaultReader _vaultReader;
        private VaultConfigurationDataModel? _configDataModel;
        private V4VaultConfigurationDataModel? _v4ConfigDataModel;
        private KeyPair? _keyPair;

        public RecoverRoutine(VaultReader vaultReader)
        {
            _vaultReader = vaultReader;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken)
        {
            _configDataModel = await _vaultReader.ReadConfigurationAsync(cancellationToken);

            if (_configDataModel.AuthenticationMethod.Contains(Constants.Vault.Authentication.AUTH_APP_PLATFORM, StringComparison.Ordinal))
            {
                try
                {
                    _v4ConfigDataModel = await _vaultReader.ReadV4ConfigurationAsync(cancellationToken);
                }
                catch (Exception)
                {
                    _v4ConfigDataModel = null;
                }
            }
        }

        /// <inheritdoc/>
        public void SetCredentials(IKeyUsage passkey)
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
                // Check if the payload has not been tampered with
                var validator = new ConfigurationValidator(_keyPair.MacKey);
                if (_v4ConfigDataModel is not null)
                    await validator.V4ValidateAsync(_v4ConfigDataModel, cancellationToken);
                else
                    await validator.ValidateAsync(_configDataModel, cancellationToken);

                // In this case, we rely on the consumer to take ownership of the keys, and thus manage their lifetimes
                // Key copies need to be created because the original ones are disposed of here
                return new SecurityWrapper(_keyPair.CreateCopy(), _configDataModel);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _keyPair?.Dispose();
        }
    }
}
