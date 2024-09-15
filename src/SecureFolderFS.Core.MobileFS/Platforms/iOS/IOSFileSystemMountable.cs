using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Core.FileSystem.Storage;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.MobileFS.Platforms.iOS
{
    /// <inheritdoc cref="IMountableFileSystem"/>
    public sealed class IOSFileSystemMountable : IMountableFileSystem, IAvailabilityChecker
    {
        private readonly IFolder _storageRoot;
        private readonly FileSystemOptions _options;

        private IOSFileSystemMountable(IFolder storageRoot, FileSystemOptions options)
        {
            _storageRoot = storageRoot;
            _options = options;
        }
        
        /// <inheritdoc/>
        public static FileSystemAvailabilityType IsSupported()
        {
            return FileSystemAvailabilityType.Available;
        }

        /// <inheritdoc/>
        public async Task<IVFSRoot> MountAsync(MountOptions mountOptions, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            return new IOSVFSRoot(_storageRoot, _options);
        }
        
        public static IMountableFileSystem CreateMountable(FileSystemSpecifics specifics)
        {
            var cryptoFolder = new CryptoFolder(Path.DirectorySeparatorChar.ToString(), specifics.ContentFolder, specifics);
            return new IOSFileSystemMountable(cryptoFolder, specifics.FileSystemOptions);
        }
    }
}
