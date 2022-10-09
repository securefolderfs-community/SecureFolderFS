using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.SecureStore;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utils;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Routines.UnlockRoutines
{
    /// <inheritdoc cref="IUnlockRoutine"/>
    internal sealed class UnlockRoutine : IUnlockRoutine
    {
        private readonly CipherProvider _cipherProvider;
        private VaultConfigurationDataModel? _configDataModel;
        private VaultKeystoreDataModel? _keystoreDataModel;
        private IFolder? _contentFolder;
        private SecretKey? _encKey;
        private SecretKey? _macKey;

        public UnlockRoutine()
        {
            _cipherProvider = CipherProvider.CreateNew();
        }

        /// <inheritdoc/>
        public void SetContentFolder(IFolder contentFolder)
        {
            _contentFolder = contentFolder;
        }

        /// <inheritdoc/>
        public async Task ReadConfigurationAsync(Stream configStream, IAsyncSerializer<Stream> serializer,
            CancellationToken cancellationToken = default)
        {
            // Check if the presumed version is supported
            IAsyncValidator<Stream> versionValidator = new VersionValidator(serializer);
            var validationResult = await versionValidator.ValidateAsync(configStream, cancellationToken);
            if (!validationResult.Successful)
                throw validationResult.Exception ?? new UnsupportedVaultException();

            _configDataModel = await serializer.DeserializeAsync<Stream, VaultConfigurationDataModel?>(configStream, cancellationToken);
            if (_configDataModel is null)
                throw new SerializationException($"Data could not be deserialized into {nameof(VaultConfigurationDataModel)}.");
        }

        /// <inheritdoc/>
        public async Task ReadKeystoreAsync(Stream keystoreStream, IAsyncSerializer<Stream> serializer,
            CancellationToken cancellationToken = default)
        {
            _keystoreDataModel = await serializer.DeserializeAsync<Stream, VaultKeystoreDataModel?>(keystoreStream, cancellationToken);
            if (_keystoreDataModel is null)
                throw new SerializationException($"Data could not be deserialized into {nameof(VaultKeystoreDataModel)}.");
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public void DeriveKeystore(IPassword password)
        {
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);

            using (password)
            {
                using var encKey = new SecureKey(new byte[Constants.KeyChains.ENCRYPTIONKEY_LENGTH]);
                using var macKey = new SecureKey(new byte[Constants.KeyChains.MACKEY_LENGTH]);

                // Derive KEK
                Span<byte> kek = stackalloc byte[Constants.KeyChains.KEK_LENGTH];
                _cipherProvider.Argon2idCrypt.DeriveKey(password.GetPassword(), _keystoreDataModel.Salt, kek);

                // Unwrap keys
                _cipherProvider.Rfc3394KeyWrap.UnwrapKey(_keystoreDataModel.WrappedEncKey, kek, encKey.Key);
                _cipherProvider.Rfc3394KeyWrap.UnwrapKey(_keystoreDataModel.WrappedMacKey, kek, macKey.Key);

                // Create copies of keys for later use
                _encKey = encKey.CreateCopy();
                _macKey = macKey.CreateCopy();
            }
        }

        /// <inheritdoc/>
        public async Task<IMountableFileSystem> PrepareAndUnlockAsync(FileSystemOptions fileSystemOptions, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(_configDataModel);
            ArgumentNullException.ThrowIfNull(_macKey);

            // Create MAC key copy for the validator
            using var macKeyCopy = _macKey.CreateCopy();

            // Check if the payload has not been tampered with
            IAsyncValidator<VaultConfigurationDataModel> validator = new ConfigurationValidator(_cipherProvider, _macKey);
            var validationResult = await validator.ValidateAsync(_configDataModel, cancellationToken);
            if (!validationResult.Successful)
                throw validationResult.Exception ?? throw new CryptographicException();


        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _encKey?.Dispose();
            _macKey?.Dispose();
        }
    }
}
