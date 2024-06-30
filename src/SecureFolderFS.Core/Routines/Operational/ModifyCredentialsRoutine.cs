using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultAccess;
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
        private VaultKeystoreDataModel? _keystoreDataModel;
        private UnlockContract? _unlockContract;

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
            if (unlockContract is not UnlockContract contract)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            _unlockContract = contract;
        }

        /// <inheritdoc/>
        public void SetCredentials(SecretKey passkey)
        {
            ArgumentNullException.ThrowIfNull(_unlockContract);

            using var encKey = _unlockContract.Security.CopyEncryptionKey();
            using var macKey = _unlockContract.Security.CopyMacKey();
            using var secureRandom = RandomNumberGenerator.Create();

            // Generate new salt
            var salt = new byte[Cryptography.Constants.KeyChains.SALT_LENGTH];
            secureRandom.GetNonZeroBytes(salt);

            // Generate keystore
            _keystoreDataModel = VaultParser.EncryptKeystore(passkey, encKey, macKey, salt);
        }

        /// <inheritdoc/>
        public async Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);

            // Write only the keystore
            await _vaultWriter.WriteKeystoreAsync(_keystoreDataModel, cancellationToken);

            // TODO: Return UnlockContract
            return null!;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _unlockContract?.Dispose();
        }
    }
}
