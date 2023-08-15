using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Routines.UnlockRoutines
{
    /// <inheritdoc cref="IUnlockRoutine"/>
    internal sealed class UnlockRoutine : IUnlockRoutine
    {
        private readonly VaultReader _vaultReader;
        private VaultConfigurationDataModel? _configDataModel;
        private VaultKeystoreDataModel? _keystoreDataModel;
        private SecretKey? _encKey;
        private SecretKey? _macKey;

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
        public IUnlockRoutine SetCredentials(IPassword password, SecretKey? magic)
        {
            ArgumentNullException.ThrowIfNull(_configDataModel);
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);

            // Construct passkey
            using var passkey = VaultParser.ConstructPasskey(password, magic);

            var (encKey, macKey) = VaultParser.DeriveKeystore(passkey, _keystoreDataModel);
            _encKey = encKey;
            _macKey = macKey;

            return this;
        }

        /// <inheritdoc/>
        public async Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_encKey);
            ArgumentNullException.ThrowIfNull(_macKey);
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

                return new UnlockContract()
                {
                    // In this case, we rely on the consumer to take ownership of the keys, and thus manage their lifetimes
                    // Key copies need to be created because the original ones are disposed of here
                    Security = Security.CreateNew(_encKey.CreateCopy(), _macKey.CreateCopy(), _configDataModel.ContentCipherScheme, _configDataModel.FileNameCipherScheme),
                    ConfigurationDataModel = _configDataModel,
                    KeystoreDataModel = _keystoreDataModel
                };
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _encKey?.Dispose();
            _macKey?.Dispose();
        }
    }

    internal sealed class UnlockContract : IDisposable
    {
        public required Security Security { get; init; }

        public required VaultConfigurationDataModel ConfigurationDataModel { get; init; }

        public required VaultKeystoreDataModel KeystoreDataModel { get; init; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Security.Dispose();
        }
    }
}
