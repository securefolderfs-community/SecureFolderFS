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
        private readonly FileSystemOptions _options;
        private readonly IFolder _opaqueRoot;

        private AndroidFileSystemMountable(FileSystemOptions options, IFolder opaqueRoot)
        {
            _options = options;
            _opaqueRoot = opaqueRoot;
        }

        /// <inheritdoc/>
        public static FileSystemAvailabilityType IsSupported()
        {
            // TODO: Check if available
            return FileSystemAvailabilityType.Available;
        }

        /// <inheritdoc/>
        public Task<IVFSRootFolder> MountAsync(MountOptions mountOptions, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedError();
        }

        public static IMountableFileSystem CreateMountable(FileSystemOptions options, IFolder contentFolder,
            Security security, DirectoryIdCache directoryIdCache, IPathConverter pathConverter,
            IStreamsAccess streamsAccess)
        {
            var cryptoFolder = new CryptoFolder(contentFolder, streamsAccess, pathConverter, directoryIdCache);
            return new AndroidFileSystemMountable(options, cryptoFolder);
        }
    }
}
