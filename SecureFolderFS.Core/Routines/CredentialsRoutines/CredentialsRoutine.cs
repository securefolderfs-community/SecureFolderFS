using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Routines.UnlockRoutines;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.Utilities;

namespace SecureFolderFS.Core.Routines.CredentialsRoutines
{
    /// <inheritdoc cref="ICredentialsRoutine"/>
    internal sealed class CredentialsRoutine : ICredentialsRoutine
    {
        private readonly IVaultWriter _vaultWriter;
        private VaultKeystoreDataModel? _keystoreDataModel;
        private UnlockContract? _unlockContract;

        public CredentialsRoutine(IVaultWriter vaultWriter)
        {
            _vaultWriter = vaultWriter;
        }

        /// <inheritdoc/>
        public ICredentialsRoutine SetUnlockContract(IDisposable unlockContract)
        {
            if (unlockContract is not UnlockContract contract)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid");

            _unlockContract = contract;
            return this;
        }

        /// <inheritdoc/>
        public ICredentialsRoutine SetCredentials(IPassword password, SecretKey? magic)
        {
            ArgumentNullException.ThrowIfNull(_unlockContract);

            using var encKey = _unlockContract.Security.CopyEncryptionKey();
            using var macKey = _unlockContract.Security.CopyMacKey();
            using var secureRandom = RandomNumberGenerator.Create();

            // Generate new salt
            var salt = new byte[Constants.KeyChains.SALT_LENGTH];
            secureRandom.GetNonZeroBytes(salt);

            // Construct passkey
            using var passkey = VaultParser.ConstructPasskey(password, magic);

            // Generate keystore
            _keystoreDataModel = VaultParser.EncryptKeystore(passkey, encKey, macKey, salt, _unlockContract.Security.CipherProvider);

            return this;
        }

        /// <inheritdoc/>
        public async Task FinalizeAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);

            // Write only the keystore
            await _vaultWriter.WriteAsync(_keystoreDataModel, null, null, cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _unlockContract?.Dispose();
        }
    }
}
