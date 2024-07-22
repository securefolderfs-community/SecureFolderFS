using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Core.FileSystem.Storage;
using SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem;
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
        public async Task<IVFSRoot> MountAsync(MountOptions mountOptions, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            return new AndroidVFSRoot(_storageRoot, _options);
        }

        public static IMountableFileSystem CreateMountable(FileSystemSpecifics specifics)
        {
            var cryptoFolder = new CryptoFolder(specifics.ContentFolder, specifics);
            return new AndroidFileSystemMountable(cryptoFolder, specifics.FileSystemOptions);
        }
    }
}
