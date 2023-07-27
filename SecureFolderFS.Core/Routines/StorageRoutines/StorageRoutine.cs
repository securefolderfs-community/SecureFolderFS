using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.CryptFiles;
using SecureFolderFS.Core.Cryptography.Enums;
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
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;

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

        // TODO: Maybe add a way of specifying the content folder here as well?
        /// <inheritdoc/>
        public IStorageRoutine SetStorageService(IStorageService storageService)
        {
            _storageService = storageService;
            return this;
        }

        /// <inheritdoc/>
        public async Task<IStorageService> CreateStorageAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_storageService);

            var contentFolder = await _vaultFolder.GetFolderAsync(Constants.Vault.VAULT_CONTENT_FOLDERNAME, cancellationToken);
            return new CryptoStorageService(_storageService);
        }

        /// <inheritdoc/>
        public async Task<IMountableFileSystem> CreateMountableAsync(FileSystemOptions options, CancellationToken cancellationToken)
        {
            var (directoryIdCache, pathConverter, streamsAccess) = CreateStorageComponents(options);

            return options.AdapterType switch
            {
                FileSystemAdapterType.DokanAdapter => DokanyMountable.CreateMountable(_vaultFolder.Name, cont),
                FileSystemAdapterType.FuseAdapter => FuseMountable.CreateMountable(_vaultFolder.Name, ),
                FileSystemAdapterType.WebDavAdapter => WebDavMountable.CreateMountable(),
                _ => throw new ArgumentOutOfRangeException(nameof(options))
            };
        }

        private (DirectoryIdCache, IPathConverter, IStreamsAccess) CreateStorageComponents(FileSystemOptions options)
        {
            ArgumentNullException.ThrowIfNull(_unlockContract);

            var directoryIdCache = new DirectoryIdCache(options.FileSystemStatistics);
            IPathConverter pathConverter;
            if (_unlockContract.ConfigurationDataModel.FileNameCipherScheme != FileNameCipherScheme.None)
            {
                IFileNameAccess fileNameAccess = options.FileNameCachingStrategy switch
                {
                    FileNameCachingStrategy.NoCache => new InstantFileNameAccess(_unlockContract.Security, options.FileSystemStatistics),
                    FileNameCachingStrategy.RandomAccessMemoryCache => new CachingFileNameAccess(_unlockContract.Security, options.FileSystemStatistics),
                    _ => throw new ArgumentOutOfRangeException(nameof(options))
                };

                pathConverter = new CiphertextPathConverter(ContentFolder.Id, fileNameAccess, directoryIdCache);
            }
            else
                pathConverter = new CleartextPathConverter();

            var cryptFileManager = new OpenCryptFileManager(_unlockContract.Security, options.ChunkCachingStrategy, options.FileSystemStatistics);
            var streamsAccess = new FileStreamAccess(_unlockContract.Security, cryptFileManager);

            return (directoryIdCache, pathConverter, streamsAccess);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // UnlockContract is not disposed of here because it's populated into filesystem and storage objects
        }
    }
}
