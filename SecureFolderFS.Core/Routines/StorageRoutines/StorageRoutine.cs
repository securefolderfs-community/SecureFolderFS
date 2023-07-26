using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Core.Routines.StorageRoutines
{
    /// <inheritdoc cref="IStorageRoutine"/>
    internal sealed class StorageRoutine : IStorageRoutine
    {
        private IStorageService? _storageService;
        private CipherProvider _cipherProvider;

        public StorageRoutine()
        {
            _cipherProvider = CipherProvider.CreateNew();
        }

        /// <inheritdoc/>
        public IStorageRoutine SetUnlockFinalizer(IDisposable unlockFinalizer)
        {
            return this;
        }

        /// <inheritdoc/>
        public IStorageRoutine SetStorageService(IStorageService storageService)
        {
            _storageService = storageService;
            return this;
        }

        /// <inheritdoc/>
        public async Task<IStorageService> CreateStorageAsync(CancellationToken cancellationToken)
        {
            // First, validate the config file
            await ValidateConfigurationAsync(cancellationToken);

            throw new NotImplementedException();
            //return new CryptoStorageService(storageService);
        }

        /// <inheritdoc/>
        public async Task<IMountableFileSystem> CreateMountableAsync(FileSystemOptions options, CancellationToken cancellationToken)
        {
            // First, validate the config file
            await ValidateConfigurationAsync(cancellationToken);
        }

        private async Task ValidateConfigurationAsync(CancellationToken cancellationToken)
        {
            // Create MAC key copy for the validator that can be disposed here
            using var macKeyCopy = _macKey.CreateCopy();

            // Check if the payload has not been tampered with
            var validator = new ConfigurationValidator(_cipherProvider.HmacSha256Crypt, macKeyCopy);
            var validationResult = await validator.ValidateAsync(_configDataModel, cancellationToken);
            if (!validationResult.Successful)
                throw validationResult.Exception ?? throw new CryptographicException();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _cipherProvider.Dispose();
        }
    }
}
