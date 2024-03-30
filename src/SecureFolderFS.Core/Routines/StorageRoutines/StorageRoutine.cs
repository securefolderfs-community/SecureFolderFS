using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.Routines.UnlockRoutines;
using System;

namespace SecureFolderFS.Core.Routines.StorageRoutines
{
    /// <inheritdoc cref="IStorageRoutine"/>
    internal sealed class StorageRoutine : IStorageRoutine
    {
        private UnlockContract? _unlockContract;

        /// <inheritdoc/>
        public IStorageRoutine SetUnlockContract(IDisposable unlockContract)
        {
            if (unlockContract is not UnlockContract contract)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            _unlockContract = contract;
            return this;
        }

        /// <inheritdoc/>
        public (DirectoryIdCache, Security, IPathConverter, IStreamsAccess) CreateStorageComponents(IFolder contentRoot, FileSystemOptions options)
        {
            ArgumentNullException.ThrowIfNull(_unlockContract);

            var directoryIdCache = new DirectoryIdCache(options.FileSystemStatistics);
            var streamsAccess = FileStreamAccess.CreateNew(_unlockContract.Security, options.EnableChunkCache, options.FileSystemStatistics);
            var pathConverter = _unlockContract.ConfigurationDataModel.FileNameCipherId != Cryptography.Constants.CipherId.NONE
                ? CiphertextPathConverter.CreateNew(_unlockContract.Security, contentRoot.Id, directoryIdCache, options.EnableFileNameCache, options.FileSystemStatistics)
                : CleartextPathConverter.CreateNew(contentRoot.Id);

            return (directoryIdCache, _unlockContract.Security, pathConverter, streamsAccess);
        }
    }
}
