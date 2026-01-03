using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Core.VaultAccess;
using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography.Extensions;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Core.Routines.Operational
{
    /// <inheritdoc cref="ICredentialsRoutine"/>
    internal sealed class UnlockRoutine : ICredentialsRoutine
    {
        private readonly VaultReader _vaultReader;
        private VaultKeystoreDataModel? _keystoreDataModel;
        private VaultConfigurationDataModel? _configDataModel;
        private SecureKey? _dekKey;
        private SecureKey? _macKey;

        public UnlockRoutine(VaultReader vaultReader)
        {
            _vaultReader = vaultReader;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken)
        {
            _keystoreDataModel = await _vaultReader.ReadKeystoreAsync(cancellationToken);
            _configDataModel = await _vaultReader.ReadConfigurationAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void SetCredentials(IKeyUsage passkey)
        {
            ArgumentNullException.ThrowIfNull(_configDataModel);
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);

            // Derive keystore
            var derived = passkey.UseKey(key => VaultParser.DeriveKeystore(key, _keystoreDataModel));

            _dekKey = SecureKey.TakeOwnership(derived.dekKey);
            _macKey = SecureKey.TakeOwnership(derived.macKey);
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
