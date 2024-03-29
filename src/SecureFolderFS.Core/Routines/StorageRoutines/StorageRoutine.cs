using OwlCore.Storage;
using SecureFolderFS.Core.CryptFiles;
using SecureFolderFS.Core.Cryptography.Storage;
using SecureFolderFS.Core.Directories;
using SecureFolderFS.Core.Dokany;
using SecureFolderFS.Core.FileNames;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.FUSE;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Routines.UnlockRoutines;
using SecureFolderFS.Core.Streams;
using SecureFolderFS.Core.WebDav;
using SecureFolderFS.Storage.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Routines.StorageRoutines
{
    /// <inheritdoc cref="IStorageRoutine"/>
    internal sealed class StorageRoutine : IStorageRoutine
    {
        private readonly IFolder _vaultFolder;
        private UnlockContract? _unlockContract;

        public StorageRoutine(IFolder vaultFolder)
        {
            _vaultFolder = vaultFolder;
        }

        /// <inheritdoc/>
        public IStorageRoutine SetUnlockContract(IDisposable unlockContract)
        {
            if (unlockContract is not UnlockContract contract)
                throw new ArgumentException($"The {nameof(unlockContract)} is invalid.");

            _unlockContract = contract;
            return this;
        }

        /// <inheritdoc/>
        public async Task<IFolder> CreateStorageRootAsync(FileSystemOptions options, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_unlockContract);

            var contentFolder = await _vaultFolder.GetFolderByNameAsync(Constants.Vault.Names.VAULT_CONTENT_FOLDERNAME, cancellationToken);
            var (directoryIdCache, pathConverter, streamsAccess) = CreateStorageComponents(options, contentFolder.Id);

            return new CryptoFolder(contentFolder, streamsAccess, pathConverter, directoryIdCache);
        }

        /// <inheritdoc/>
        public async Task<IMountableFileSystem> CreateMountableAsync(FileSystemOptions options, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_unlockContract);

            var contentFolder = await _vaultFolder.GetFolderByNameAsync(Constants.Vault.Names.VAULT_CONTENT_FOLDERNAME, cancellationToken);
            var (directoryIdCache, pathConverter, streamsAccess) = CreateStorageComponents(options, contentFolder.Id);

            return options.FileSystemId switch
            {
                Constants.FileSystemId.FS_DOKAN => DokanyMountable.CreateMountable(options, contentFolder, _unlockContract.Security, directoryIdCache, pathConverter, streamsAccess),
                Constants.FileSystemId.FS_FUSE => FuseMountable.CreateMountable(options, contentFolder, _unlockContract.Security, directoryIdCache, pathConverter, streamsAccess),
                Constants.FileSystemId.FS_WEBDAV => WebDavMountable.CreateMountable(options, contentFolder, _unlockContract.Security, directoryIdCache, pathConverter, streamsAccess),
                _ => throw new ArgumentOutOfRangeException(nameof(options))
            };
        }

        private (DirectoryIdCache, IPathConverter, IStreamsAccess) CreateStorageComponents(FileSystemOptions options, string vaultRootPath)
        {
            ArgumentNullException.ThrowIfNull(_unlockContract);

            var directoryIdCache = new DirectoryIdCache(options.FileSystemStatistics);
            IPathConverter pathConverter;
            if (_unlockContract.ConfigurationDataModel.FileNameCipherId != Cryptography.Constants.CipherId.NONE)
            {
                var fileNameAccess = options.EnableFileNameCache
                    ? new FileNameAccess(_unlockContract.Security, options.FileSystemStatistics)
                    : new CachingFileNameAccess(_unlockContract.Security, options.FileSystemStatistics);

                pathConverter = new CiphertextPathConverter(vaultRootPath, fileNameAccess, directoryIdCache);
            }
            else
                pathConverter = new CleartextPathConverter(vaultRootPath);

            var cryptFileManager = new OpenCryptFileManager(_unlockContract.Security, options.EnableChunkCache, options.FileSystemStatistics);
            var streamsAccess = new FileStreamAccess(_unlockContract.Security, cryptFileManager);

            return (directoryIdCache, pathConverter, streamsAccess);
        }
    }
}
