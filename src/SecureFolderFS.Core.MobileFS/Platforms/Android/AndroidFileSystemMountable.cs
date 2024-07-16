using Kotlin;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Directories;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Core.FileSystem.Paths;
using SecureFolderFS.Core.FileSystem.Storage;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android
{
    /// <inheritdoc cref="IMountableFileSystem"/>
    public sealed class AndroidFileSystemMountable : IMountableFileSystem, IAvailabilityChecker
    {
        private readonly IFolder _storageRoot;
        private readonly FileSystemOptions _options;

        private AndroidFileSystemMountable(IFolder storageRoot, FileSystemOptions options)
        {
            _storageRoot = storageRoot;
            _options = options;
        }

        /// <inheritdoc/>
        public static FileSystemAvailabilityType IsSupported()
        {
            // TODO: Check if available
            return FileSystemAvailabilityType.Available;
        }

        /// <inheritdoc/>
        public Task<IVFSRoot> MountAsync(MountOptions mountOptions, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedError();
        }

        public static IMountableFileSystem CreateMountable(FileSystemOptions options, IFolder contentFolder,
            Security security, DirectoryIdCache directoryIdCache, IPathConverter pathConverter,
            IStreamsAccess streamsAccess)
        {
            var cryptoFolder = new CryptoFolder(contentFolder, streamsAccess, pathConverter, directoryIdCache);
            return new AndroidFileSystemMountable(cryptoFolder, options);
        }
    }
}
