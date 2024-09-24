using OwlCore.Storage;
using SecureFolderFS.Core.Contracts;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Paths;
using System;

namespace SecureFolderFS.Core.Routines.Operational
{
    /// <inheritdoc cref="IStorageRoutine"/>
    internal sealed class StorageRoutine : IStorageRoutine
    {
        private SecurityContract? _unlockContract;

        /// <inheritdoc/>
        public void SetUnlockContract(IDisposable unlockContract)
        {
            if (unlockContract is not SecurityContract contract)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            _unlockContract = contract;
        }

        /// <inheritdoc/>
        public IPathConverter CreateStorageComponents(IFolder contentFolder, FileSystemOptions options)
        {
            ArgumentNullException.ThrowIfNull(_unlockContract);

            var directoryIdCache = new DirectoryIdCache(options.FileSystemStatistics);
            var pathConverter = _unlockContract.ConfigurationDataModel.FileNameCipherId != Cryptography.Constants.CipherId.NONE
                ? CiphertextPathConverter.CreateNew(_unlockContract.Security, contentFolder, directoryIdCache, options.EnableFileNameCache, options.FileSystemStatistics)
                : CleartextPathConverter.CreateNew(contentFolder);

            return pathConverter;
        }

        /// <inheritdoc/>
        public FileSystemSpecifics GetSpecifics(IFolder contentFolder, FileSystemOptions options)
        {
            ArgumentNullException.ThrowIfNull(_unlockContract);
            return FileSystemSpecifics.CreateNew(_unlockContract.Security, contentFolder, options);
        }
    }
}
