using SecureFolderFS.Core.CryptFiles;
using SecureFolderFS.Core.Cryptography.Storage;
using SecureFolderFS.Core.Directories;
using SecureFolderFS.Core.Dokany;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileNames;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.FileNames;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.FUSE;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Routines.UnlockRoutines;
using SecureFolderFS.Core.Streams;
using SecureFolderFS.Core.WebDav;
using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;

namespace SecureFolderFS.Core.Routines.StorageRoutines
{
    /// <inheritdoc cref="IStorageRoutine"/>
    internal sealed class StorageRoutine : IStorageRoutine
    {
        private readonly IFolder _vaultFolder;
        private UnlockContract? _unlockContract;
        private IGetItemRecursive? _storageRoot;

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

        // TODO: Maybe add a way of specifying the content folder here as well?
        /// <inheritdoc/>
        public IStorageRoutine SetStorageService(IGetItemRecursive storageRoot)
        {
            _storageRoot = storageRoot;
            return this;
        }

        /// <inheritdoc/>
        public async Task<IGetItemRecursive> CreateStorageRootAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_storageRoot);

            return new CryptoStorageService(_storageRoot);
        }

        /// <inheritdoc/>
        public async Task<IMountableFileSystem> CreateMountableAsync(FileSystemOptions options, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_unlockContract);
            ArgumentNullException.ThrowIfNull(_storageService);

            var contentFolder = await _vaultFolder.GetFolderAsync(Constants.Vault.Names.VAULT_CONTENT_FOLDERNAME, cancellationToken);
            var volumeName = options.VolumeName ?? _vaultFolder.Name;
            var (directoryIdCache, pathConverter, streamsAccess) = CreateStorageComponents(options, contentFolder.Id);

            return options.FileSystemId switch
            {
                Constants.FileSystemId.FS_DOKAN => DokanyMountable.CreateMountable(volumeName, contentFolder, _unlockContract.Security, directoryIdCache, pathConverter, streamsAccess, options.HealthStatistics),
                Constants.FileSystemId.FS_FUSE => FuseMountable.CreateMountable(volumeName, contentFolder, _unlockContract.Security, directoryIdCache, pathConverter, streamsAccess),
                Constants.FileSystemId.FS_WEBDAV => WebDavMountable.CreateMountable(volumeName, contentFolder, _unlockContract.Security, directoryIdCache, pathConverter, streamsAccess, _storageService),
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
                IFileNameAccess fileNameAccess = options.FileNameCachingStrategy switch
                {
                    FileNameCachingStrategy.NoCache => new FileNameAccess(_unlockContract.Security, options.FileSystemStatistics),
                    FileNameCachingStrategy.RandomAccessMemoryCache => new CachingFileNameAccess(_unlockContract.Security, options.FileSystemStatistics),
                    _ => throw new ArgumentOutOfRangeException(nameof(options))
                };

                pathConverter = new CiphertextPathConverter(vaultRootPath, fileNameAccess, directoryIdCache);
            }
            else
                pathConverter = new CleartextPathConverter(vaultRootPath);

            var cryptFileManager = new OpenCryptFileManager(_unlockContract.Security, options.ChunkCachingStrategy, options.FileSystemStatistics);
            var streamsAccess = new FileStreamAccess(_unlockContract.Security, cryptFileManager);

            return (directoryIdCache, pathConverter, streamsAccess);
        }
    }
}
