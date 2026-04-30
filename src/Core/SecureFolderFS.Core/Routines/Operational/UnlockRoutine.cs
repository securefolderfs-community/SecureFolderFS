using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.SecureStore;

namespace SecureFolderFS.Core.Routines.Operational
{
    /// <inheritdoc cref="ICredentialsRoutine"/>
    internal sealed class UnlockRoutine : ICredentialsRoutine
    {
        private readonly VaultReader _vaultReader;
        private V3VaultKeystoreDataModel? _keystoreDataModel;
        private V4VaultKeystoreDataModel? _v4KeystoreDataModel;
        private VaultConfigurationDataModel? _configDataModel;
        private V4VaultConfigurationDataModel? _v4ConfigDataModel;
        private SecureKey? _dekKey;
        private SecureKey? _macKey;

        public UnlockRoutine(VaultReader vaultReader)
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

            if (_configDataModel.Version >= Constants.Vault.Versions.V4)
                _v4KeystoreDataModel = await _vaultReader.ReadKeystoreAsync<V4VaultKeystoreDataModel>(cancellationToken);
            else
                _keystoreDataModel = await _vaultReader.ReadKeystoreAsync<V3VaultKeystoreDataModel>(cancellationToken);
        }

        /// <inheritdoc/>
        public void SetCredentials(IKeyUsage passkey)
        {
            ArgumentNullException.ThrowIfNull(_configDataModel);

            if (_v4KeystoreDataModel is not null)
            {
                var derived = passkey.UseKey(key => VaultParser.V4DeriveKeystore(key, _v4KeystoreDataModel));
                _dekKey = SecureKey.TakeOwnership(derived.dekKey);
                _macKey = SecureKey.TakeOwnership(derived.macKey);
            }
            else
            {
                ArgumentNullException.ThrowIfNull(_keystoreDataModel);

                // V3 path: unchanged
                var derived = passkey.UseKey(key => VaultParser.V3DeriveKeystore(key, _keystoreDataModel));
                _dekKey = SecureKey.TakeOwnership(derived.dekKey);
                _macKey = SecureKey.TakeOwnership(derived.macKey);
            }
        }

        /// <inheritdoc/>
        public async Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_dekKey);
            ArgumentNullException.ThrowIfNull(_macKey);
            ArgumentNullException.ThrowIfNull(_configDataModel);
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);

            using (_dekKey)
            using (_macKey)
            {
                // Check if the payload has not been tampered with
                var validator = new ConfigurationValidator(_macKey);
                if (_v4ConfigDataModel is not null)
                    await validator.V4ValidateAsync(_v4ConfigDataModel, cancellationToken);
                else
                    await validator.ValidateAsync(_configDataModel, cancellationToken);

                // In this case, we rely on the consumer to take ownership of the keys, and thus manage their lifetimes
                // Key copies need to be created because the original ones are disposed of here
                return new SecurityWrapper(KeyPair.ImportKeys(_dekKey, _macKey), _configDataModel);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _dekKey?.Dispose();
            _macKey?.Dispose();
        }
    }
}
