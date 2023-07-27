using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.Routines.UnlockRoutines;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Core.Routines.StorageRoutines
{
    /// <inheritdoc cref="IStorageRoutine"/>
    internal sealed class StorageRoutine : IStorageRoutine
    {
        private readonly IFolder _vaultFolder;
        private UnlockContract? _unlockContract;
        private IStorageService? _storageService;

        public StorageRoutine(IFolder vaultFolder)
        {
            _vaultFolder = vaultFolder;
        }

        /// <inheritdoc/>
        public IStorageRoutine SetUnlockContract(IDisposable unlockContract)
        {
            if (unlockContract is not UnlockContract contract)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid");

            _unlockContract = contract;
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

            throw new NotImplementedException();
            //return new CryptoStorageService(storageService);
        }

        /// <inheritdoc/>
        public async Task<IMountableFileSystem> CreateMountableAsync(FileSystemOptions options, CancellationToken cancellationToken)
        {
            
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Security is not disposed of here because it's populated into filesystem and storage objects
        }
    }
}
