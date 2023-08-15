using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Routines.CreationRoutines
{
    /// <inheritdoc cref="ICreationRoutine"/>
    internal sealed class CreationRoutine : ICreationRoutine
    {
        private readonly IFolder _vaultFolder;
        private readonly VaultWriter _vaultWriter;
        private VaultConfigurationDataModel? _configDataModel;
        private VaultKeystoreDataModel? _keystoreDataModel;
        private SecretKey? _macKey;
        private SecretKey? _encKey;

        public CreationRoutine(IFolder vaultFolder, VaultWriter vaultWriter)
        {
            _vaultFolder = vaultFolder;
            _vaultWriter = vaultWriter;
        }

        /// <inheritdoc/>
        public ICreationRoutine SetCredentials(IPassword password, SecretKey? magic)
        {
            using var encKey = new SecureKey(Constants.KeyChains.ENCKEY_LENGTH);
            using var macKey = new SecureKey(Constants.KeyChains.MACKEY_LENGTH);
            var salt = new byte[Constants.KeyChains.SALT_LENGTH];

            // Fill keys
            using var secureRandom = RandomNumberGenerator.Create();
            secureRandom.GetNonZeroBytes(encKey.Key);
            secureRandom.GetNonZeroBytes(macKey.Key);
            secureRandom.GetNonZeroBytes(salt);

            // Construct passkey
            using var passkey = VaultParser.ConstructPasskey(password, magic);

            // Generate keystore
            _keystoreDataModel = VaultParser.EncryptKeystore(passkey, encKey, macKey, salt);

            // Create key copies for later use
            _macKey = macKey.CreateCopy();
            _encKey = encKey.CreateCopy();

            return this;
        }

        /// <inheritdoc/>
        public ICreationRoutine SetOptions(VaultOptions vaultOptions)
        {
            _configDataModel = new()
            {
                ContentCipherScheme = vaultOptions.ContentCipher,
                FileNameCipherScheme = vaultOptions.FileNameCipher,
                Version = Constants.VaultVersion.LATEST_VERSION,
                Id = Guid.NewGuid().ToString(),
                AuthMethod = Constants.AuthenticationMethods.AUTH_PASSWORD,
                PayloadMac = new byte[HMACSHA256.HashSizeInBytes]
            };

            return this;
        }

        /// <inheritdoc/>
        public async Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);
            ArgumentNullException.ThrowIfNull(_configDataModel);
            ArgumentNullException.ThrowIfNull(_macKey);
            ArgumentNullException.ThrowIfNull(_encKey);

            // First we need to fill in the PayloadMac of the content
            VaultParser.CalculateConfigMac(_configDataModel, _macKey, _configDataModel.PayloadMac);

            // Write the whole config
            await _vaultWriter.WriteKeystoreAsync(_keystoreDataModel, cancellationToken);
            await _vaultWriter.WriteConfigurationAsync(_configDataModel, cancellationToken);

            // Create content folder
            if (_vaultFolder is IModifiableFolder modifiableFolder)
                await modifiableFolder.CreateFolderAsync(Constants.Vault.VAULT_CONTENT_FOLDERNAME, false, cancellationToken);

            // Key copies need to be created because the original ones are disposed of here
            return new CreationContract(_encKey.CreateCopy(), _macKey.CreateCopy());
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _encKey?.Dispose();
            _macKey?.Dispose();
        }
    }

    internal sealed class CreationContract : IDisposable
    {
        private readonly SecretKey _encKey;
        private readonly SecretKey _macKey;

        public CreationContract(SecretKey encKey, SecretKey macKey)
        {
            _encKey = encKey;
            _macKey = macKey;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Convert.ToBase64String(_encKey)}{Constants.KEY_TEXT_SEPARATOR}{Convert.ToBase64String(_macKey)}";
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _encKey.Dispose();
            _macKey.Dispose();
        }
    }
}
