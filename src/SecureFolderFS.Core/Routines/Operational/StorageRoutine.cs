using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using System;

namespace SecureFolderFS.Core.Routines.Operational
{
    /// <inheritdoc cref="IStorageRoutine"/>
    internal sealed class StorageRoutine : IStorageRoutine
    {
        private UnlockContract? _unlockContract;

        /// <inheritdoc/>
        public void SetUnlockContract(IDisposable unlockContract)
        {
            if (unlockContract is not UnlockContract contract)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            _unlockContract = contract;
        }

        /// <inheritdoc/>
        public (DirectoryIdCache, Security, IPathConverter, StreamsAccess) CreateStorageComponents(IFolder contentFolder, FileSystemOptions options)
        {
            ArgumentNullException.ThrowIfNull(_unlockContract);

            var directoryIdCache = new DirectoryIdCache(options.FileSystemStatistics);
            var streamsAccess = StreamsAccess.CreateNew(_unlockContract.Security, options.EnableChunkCache, options.FileSystemStatistics);
            var pathConverter = _unlockContract.ConfigurationDataModel.FileNameCipherId != Cryptography.Constants.CipherId.NONE
                ? CiphertextPathConverter.CreateNew(_unlockContract.Security, contentFolder, directoryIdCache, options.EnableFileNameCache, options.FileSystemStatistics)
                : CleartextPathConverter.CreateNew(contentFolder);

            return (directoryIdCache, _unlockContract.Security, pathConverter, streamsAccess);
        }

        /// <inheritdoc/>
        public FileSystemSpecifics GetSpecifics(IFolder contentFolder, FileSystemOptions options)
        {
            ArgumentNullException.ThrowIfNull(_unlockContract);
            return FileSystemSpecifics.CreateNew(_unlockContract.Security, contentFolder, options);
        }
    }
}
