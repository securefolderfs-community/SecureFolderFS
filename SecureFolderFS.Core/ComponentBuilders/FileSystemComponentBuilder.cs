using SecureFolderFS.Core.CryptFiles;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Cryptography.Enums;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Directories;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileNames;
using SecureFolderFS.Core.FileSystem.Analytics;
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
        public required VaultConfigurationDataModel ConfigDataModel { get; init; }

        public required FileSystemOptions FileSystemOptions { get; init; }

        public required IFolder ContentFolder { get; init; }

        public (Security, IDirectoryIdAccess, IPathConverter, IStreamsAccess) BuildComponents(SecretKey encKey, SecretKey macKey, IFileSystemHealthStatistics? healthStatistics)
        {
            var security = Security.CreateNew(encKey.CreateCopy(), macKey.CreateCopy(), ConfigDataModel.ContentCipherScheme, ConfigDataModel.FileNameCipherScheme);

            IDirectoryIdAccess directoryIdAccess = FileSystemOptions.DirectoryIdCachingStrategy switch
            {
                // TODO: Use correct impl
                DirectoryIdCachingStrategy.RandomAccessMemoryCache => new DeprecatedDirectoryIdAccessImpl(healthStatistics),
                DirectoryIdCachingStrategy.NoCache => new DeprecatedDirectoryIdAccessImpl(healthStatistics),
                _ => throw new ArgumentOutOfRangeException(nameof(FileSystemOptions.DirectoryIdCachingStrategy))
            };

            IPathConverter pathConverter;
            if (ConfigDataModel.FileNameCipherScheme == FileNameCipherScheme.AES_SIV)
            {
                IFileNameAccess fileNameAccess = FileSystemOptions.FileNameCachingStrategy switch
                {
                    FileNameCachingStrategy.RandomAccessMemoryCache => new CachingFileNameAccess(security, FileSystemOptions.FileSystemStatistics),
                    FileNameCachingStrategy.NoCache => new InstantFileNameAccess(security, FileSystemOptions.FileSystemStatistics),
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

            var cryptFileManager = new OpenCryptFileManager(security, FileSystemOptions.ChunkCachingStrategy, FileSystemOptions.FileSystemStatistics);
            var streamsAccess = new FileStreamAccess(security, cryptFileManager);

            return (security, directoryIdAccess, pathConverter, streamsAccess);
        }
    }
}
