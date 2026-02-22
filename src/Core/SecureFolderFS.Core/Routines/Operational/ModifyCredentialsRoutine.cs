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
        public unsafe void SetCredentials(IKeyUsage passkey)
        {
            ArgumentNullException.ThrowIfNull(_keyPair);

            // Generate new salt
            var salt = new byte[Cryptography.Constants.KeyTraits.SALT_LENGTH];
            RandomNumberGenerator.Fill(salt);

            // Encrypt a new keystore
            passkey.UseKey(key =>
            {
                fixed (byte* keyPtr = key)
                {
                    var state = (keyPtr: (nint)keyPtr, keyLen: key.Length);
                    _keyPair.UseKeys(state, (dekKey, macKey, s) =>
                    {
                        var k = new ReadOnlySpan<byte>((byte*)s.keyPtr, s.keyLen);
                        _keystoreDataModel = VaultParser.EncryptKeystore(k, dekKey, macKey, salt);
                    });
                }
            });
        }

        /// <inheritdoc/>
        public async Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_keyPair);
            ArgumentNullException.ThrowIfNull(_configDataModel);

            // First, we need to fill in the PayloadMac of the content
            _keyPair.MacKey.UseKey(macKey =>
            {
                VaultParser.CalculateConfigMac(_configDataModel, macKey, _configDataModel.PayloadMac);
            });

            // Write the whole configuration
            await _vaultWriter.WriteKeystoreAsync(_keystoreDataModel, cancellationToken);
            await _vaultWriter.WriteConfigurationAsync(_configDataModel, cancellationToken);

            // Key copies need to be created because the original ones are disposed of here
            using (_keyPair)
                return new SecurityWrapper(_keyPair.CreateCopy(), _configDataModel);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _keyPair?.Dispose();
        }
    }
}
