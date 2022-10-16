using SecureFolderFS.Core.CryptFiles;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.Enums;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Directories;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileNames;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.FileNames;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Streams;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System;

namespace SecureFolderFS.Core.ComponentBuilders
{
    internal sealed class FileSystemComponentBuilder // Terrible.
    { 
        // TODO: Add required modifier
        public VaultConfigurationDataModel ConfigDataModel { get; init; }

        public FileSystemOptions FileSystemOptions { get; init; }

        public IFolder ContentFolder { get; init; }

        public (Security, IDirectoryIdAccess, IPathConverter, IStreamsAccess) BuildComponents(SecretKey encKey, SecretKey macKey)
        {
            var security = Security.CreateNew(encKey, macKey, ConfigDataModel.ContentCipherScheme, ConfigDataModel.FileNameCipherScheme);

            IDirectoryIdAccess directoryIdAccess = FileSystemOptions.DirectoryIdCachingStrategy switch
            {
                // TODO: Use correct impl
                DirectoryIdCachingStrategy.RandomAccessMemoryCache => new DeprecatedDirectoryIdAccessImpl(),
                DirectoryIdCachingStrategy.NoCache => new DeprecatedDirectoryIdAccessImpl(),
                _ => throw new ArgumentOutOfRangeException(nameof(FileSystemOptions.DirectoryIdCachingStrategy))
            };

            IPathConverter pathConverter;
            if (ConfigDataModel.FileNameCipherScheme == FileNameCipherScheme.AES_SIV)
            {
                IFileNameAccess fileNameAccess = FileSystemOptions.FileNameCachingStrategy switch
                {
                    FileNameCachingStrategy.RandomAccessMemoryCache => new CachingFileNameAccess(security, FileSystemOptions.FileSystemStatsTracker),
                    FileNameCachingStrategy.NoCache => new InstantFileNameAccess(security, FileSystemOptions.FileSystemStatsTracker),
                    _ => throw new ArgumentOutOfRangeException(nameof(FileSystemOptions.FileNameCachingStrategy))
                };

                if (ContentFolder is not ILocatableFolder locatableContentFolder)
                    throw new ArgumentException($"{nameof(ContentFolder)} is not locatable vault folder.");

                pathConverter = new CiphertextPathConverter(locatableContentFolder.Path, fileNameAccess, directoryIdAccess);
            }
            else if (ConfigDataModel.FileNameCipherScheme == FileNameCipherScheme.None)
            {
                pathConverter = new CleartextPathConverter();
            }
            else
                throw new ArgumentOutOfRangeException(nameof(VaultConfigurationDataModel.FileNameCipherScheme));

            var cryptFileManager = new OpenCryptFileManager(security, FileSystemOptions.ChunkCachingStrategy, FileSystemOptions.FileSystemStatsTracker);
            var streamsAccess = new FileStreamAccess(security, cryptFileManager);

            return (security, directoryIdAccess, pathConverter, streamsAccess);
        }
    }
}
