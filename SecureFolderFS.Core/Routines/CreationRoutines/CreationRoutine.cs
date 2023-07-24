using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Core.Routines.CreationRoutines
{
    /// <inheritdoc cref="ICreationRoutine"/>
    internal sealed class CreationRoutine : ICreationRoutine
    {
        private readonly IFolder _vaultFolder;
        private readonly CipherProvider _cipherProvider;
        private readonly IAsyncSerializer<Stream> _serializer;
        private VaultConfigurationDataModel? _configDataModel;
        private VaultKeystoreDataModel? _keystoreDataModel;
        private SecretKey? _macKey;

        public CreationRoutine(IFolder vaultFolder, IAsyncSerializer<Stream> serializer)
        {
            _vaultFolder = vaultFolder;
            _serializer = serializer;
            _cipherProvider = CipherProvider.CreateNew();
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public ICreationRoutine SetPassword(IPassword password)
        {
            using (password)
            {
                using var encKey = new SecureKey(new byte[Constants.KeyChains.ENCKEY_LENGTH]);
                using var macKey = new SecureKey(new byte[Constants.KeyChains.MACKEY_LENGTH]);
                var salt = new byte[Constants.KeyChains.SALT_LENGTH];

                // Fill keys
                using var secureRandom = RandomNumberGenerator.Create();
                secureRandom.GetNonZeroBytes(encKey.Key);
                secureRandom.GetNonZeroBytes(macKey.Key);
                secureRandom.GetNonZeroBytes(salt);

                // Derive KEK
                Span<byte> kek = stackalloc byte[Cryptography.Constants.ARGON2_KEK_LENGTH];
                _cipherProvider.Argon2idCrypt.DeriveKey(password.GetPassword(), salt, kek);

                // Wrap keys
                var wrappedEncKey = _cipherProvider.Rfc3394KeyWrap.WrapKey(encKey, kek);
                var wrappedMacKey = _cipherProvider.Rfc3394KeyWrap.WrapKey(macKey, kek);

                // Construct keystore data model
                _keystoreDataModel = new()
                {
                    WrappedEncKey = wrappedEncKey,
                    WrappedMacKey = wrappedMacKey,
                    Salt = salt
                };

                // Create MAC key copy for later use
                _macKey = macKey.CreateCopy();
            }
            
            return this;
        }

        /// <inheritdoc/>
        public ICreationRoutine SetOptions(VaultOptions vaultOptions)
        {
            _configDataModel = new()
            {
                ContentCipherScheme = vaultOptions.ContentCipherScheme,
                FileNameCipherScheme = vaultOptions.FileNameCipherScheme,
                Version = Constants.VaultVersion.LATEST_VERSION,
                Id = Guid.NewGuid().ToString(),
                AuthMethod = Constants.AuthenticationMethods.AUTH_PASSWORD,
                PayloadMac = new byte[_cipherProvider.HmacSha256Crypt.MacSize]
            };
            return this;
        }

        /// <inheritdoc/>
        public async Task FinalizeAsync(CancellationToken cancellationToken)
        {
            if (_vaultFolder is not IModifiableFolder modifiableFolder)
                throw new ArgumentException("The vault folder is not modifiable");

            // Create content folder
            _ = await modifiableFolder.CreateFolderAsync(Constants.CONTENT_FOLDERNAME, false, cancellationToken);

            // Create keystore file
            var keystoreFile = await modifiableFolder.CreateFileAsync(Constants.VAULT_KEYSTORE_FILENAME, true, cancellationToken);

            // Create configuration file
            var configFile = await modifiableFolder.CreateFileAsync(Constants.VAULT_CONFIGURATION_FILENAME, true, cancellationToken);

            // Write to keystore
            await using var keystoreStream = await keystoreFile.OpenStreamAsync(FileAccess.ReadWrite, cancellationToken);
            await WriteKeystoreAsync(keystoreStream, cancellationToken);

            // Write to configuration
            await using var configStream = await configFile.OpenStreamAsync(FileAccess.ReadWrite, cancellationToken);
            await WriteConfigurationAsync(configStream, cancellationToken);
        }

        private async Task WriteKeystoreAsync(Stream keystoreStream, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);

            await using var serializedKeystore = await _serializer.SerializeAsync(_keystoreDataModel, cancellationToken);
            await serializedKeystore.CopyToAsync(keystoreStream, cancellationToken);
            keystoreStream.Position = 0L;
        }

        private async Task WriteConfigurationAsync(Stream configStream, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(_macKey);
            ArgumentNullException.ThrowIfNull(_configDataModel);

            using (_macKey)
            {
                using var hmacSha256Crypt = _cipherProvider.HmacSha256Crypt.GetInstance();
                hmacSha256Crypt.InitializeHmac(_macKey);
                hmacSha256Crypt.Update(BitConverter.GetBytes(Constants.VaultVersion.LATEST_VERSION));
                hmacSha256Crypt.Update(BitConverter.GetBytes((uint)_configDataModel.FileNameCipherScheme));
                hmacSha256Crypt.Update(BitConverter.GetBytes((uint)_configDataModel.ContentCipherScheme));

                // Fill the hash to payload
                hmacSha256Crypt.GetHash(_configDataModel.PayloadMac);
                
                // Serialize data and write
                await using var serializedConfig = await _serializer.SerializeAsync(_configDataModel, cancellationToken);
                await serializedConfig.CopyToAsync(configStream, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _macKey?.Dispose();
            _cipherProvider.Dispose();
        }
    }
}
