using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Core.Routines.CreationRoutines
{
    /// <inheritdoc cref="ICreationRoutine"/>
    internal sealed class CreationRoutine : ICreationRoutine
    {
        private readonly IFolder _vaultFolder;
        private readonly CipherProvider _cipherProvider;
        private readonly IVaultWriter _vaultWriter;
        private VaultConfigurationDataModel? _configDataModel;
        private VaultKeystoreDataModel? _keystoreDataModel;
        private SecretKey? _macKey;

        public CreationRoutine(IFolder vaultFolder, IVaultWriter vaultWriter)
        {
            _vaultFolder = vaultFolder;
            _vaultWriter = vaultWriter;
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
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);
            ArgumentNullException.ThrowIfNull(_configDataModel);

            // First we need to fill in the PayloadMac of the content
            FillPayloadMac(_configDataModel);

            // Write the whole config
            await _vaultWriter.WriteAsync(_keystoreDataModel, _configDataModel, cancellationToken);

            // Create content folder
            if (_vaultFolder is IModifiableFolder modifiableFolder)
                await modifiableFolder.CreateFolderAsync(Constants.CONTENT_FOLDERNAME, false, cancellationToken);
        }

        private void FillPayloadMac(VaultConfigurationDataModel configDataModel)
        {
            ArgumentNullException.ThrowIfNull(_macKey);

            using (_macKey)
            {
                using var hmacSha256Crypt = _cipherProvider.HmacSha256Crypt.GetInstance();
                hmacSha256Crypt.InitializeHmac(_macKey);
                hmacSha256Crypt.Update(BitConverter.GetBytes(Constants.VaultVersion.LATEST_VERSION));
                hmacSha256Crypt.Update(BitConverter.GetBytes((uint)configDataModel.FileNameCipherScheme));
                hmacSha256Crypt.Update(BitConverter.GetBytes((uint)configDataModel.ContentCipherScheme));

                // Fill the hash to payload
                hmacSha256Crypt.GetHash(configDataModel.PayloadMac);
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
