using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using static SecureFolderFS.Core.Constants.Vault;
using static SecureFolderFS.Core.Cryptography.Constants;

namespace SecureFolderFS.Core.Routines.Operational
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
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void SetCredentials(SecretKey passkey)
        {
            // Allocate shallow keys which will be later disposed of
            using var encKey = new SecureKey(Cryptography.Constants.KeyChains.ENCKEY_LENGTH);
            using var macKey = new SecureKey(Cryptography.Constants.KeyChains.MACKEY_LENGTH);
            var salt = new byte[Cryptography.Constants.KeyChains.SALT_LENGTH];

            // Fill keys
            using var secureRandom = RandomNumberGenerator.Create();
            secureRandom.GetNonZeroBytes(encKey.Key);
            secureRandom.GetNonZeroBytes(macKey.Key);
            secureRandom.GetNonZeroBytes(salt);

            // Generate keystore
            _keystoreDataModel = VaultParser.EncryptKeystore(passkey, encKey, macKey, salt);

            // Create key copies for later use
            _macKey = macKey.CreateCopy();
            _encKey = encKey.CreateCopy();
        }

        /// <inheritdoc/>
        public void SetOptions(IDictionary<string, string?> options)
        {
            _configDataModel = new()
            {
                Version = Versions.LATEST_VERSION,
                ContentCipherId = options.Get(Associations.ASSOC_CONTENT_CIPHER_ID) ?? CipherId.XCHACHA20_POLY1305,
                FileNameCipherId = options.Get(Associations.ASSOC_FILENAME_CIPHER_ID) ?? CipherId.AES_SIV,
                AuthenticationMethod = options.Get(Associations.ASSOC_AUTHENTICATION) ?? throw new InvalidOperationException("Cannot create vault without specifying the authentication method."),
                Uid = options.Get(Associations.ASSOC_VAULT_ID) ?? Guid.NewGuid().ToString(),
                PayloadMac = new byte[HMACSHA256.HashSizeInBytes]
            };
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
                await modifiableFolder.CreateFolderAsync(Names.VAULT_CONTENT_FOLDERNAME, true, cancellationToken);

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
