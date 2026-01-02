using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Routines.Operational
{
    /// <inheritdoc cref="IModifyCredentialsRoutine"/>
    internal sealed class ModifyCredentialsRoutine : IModifyCredentialsRoutine
    {
        private readonly VaultWriter _vaultWriter;
        private KeyPair? _keyPair;
        private VaultKeystoreDataModel? _keystoreDataModel;
        private VaultConfigurationDataModel? _configDataModel;

        public ModifyCredentialsRoutine(VaultWriter vaultWriter)
        {
            _vaultWriter = vaultWriter;
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void SetUnlockContract(IDisposable unlockContract)
        {
            if (unlockContract is not IWrapper<Security> securityWrapper)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            _keyPair = securityWrapper.Inner.KeyPair;
        }

        /// <inheritdoc/>
        public void SetOptions(VaultOptions vaultOptions)
        {
            _configDataModel = VaultConfigurationDataModel.FromVaultOptions(vaultOptions);
        }

        /// <inheritdoc/>
        public void SetCredentials(ManagedKey passkey)
        {
            ArgumentNullException.ThrowIfNull(_keyPair);

            // Generate new salt
            using var secureRandom = RandomNumberGenerator.Create();
            var salt = new byte[Cryptography.Constants.KeyTraits.SALT_LENGTH];
            secureRandom.GetNonZeroBytes(salt);

            // Encrypt new keystore
            _keystoreDataModel = VaultParser.EncryptKeystore(passkey, _keyPair.DekKey, _keyPair.MacKey, salt);
        }

        /// <inheritdoc/>
        public async Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_keyPair);
            ArgumentNullException.ThrowIfNull(_configDataModel);

            // First we need to fill in the PayloadMac of the content
            VaultParser.CalculateConfigMac(_configDataModel, _keyPair.MacKey, _configDataModel.PayloadMac);

            // Write the whole configuration
            await _vaultWriter.WriteKeystoreAsync(_keystoreDataModel, cancellationToken);
            await _vaultWriter.WriteConfigurationAsync(_configDataModel, cancellationToken);

            // Key copies need to be created because the original ones are disposed of here
            using (_keyPair)
                return new SecurityWrapper(KeyPair.ImportKeys(_keyPair.DekKey, _keyPair.MacKey), _configDataModel);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _keyPair?.Dispose();
        }
    }
}
